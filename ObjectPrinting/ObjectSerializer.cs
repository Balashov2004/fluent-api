using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ObjectPrinting.Interface;

namespace ObjectPrinting;

public class ObjectSerializer
{
    public string Print<TOwner>(TOwner obj, PrintingConfig<TOwner> config)
    {
        var visited = new HashSet<object>(new ReferenceEqualityComparer());
        return PrintInternal(obj, 0, config, visited);
    }

    private string PrintInternal(
        object obj,
        int level,
        IPrintingConfigInternal config,
        HashSet<object> visited)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        var type = obj.GetType();

        if (IsSimpleType(type))
            return obj + Environment.NewLine;

        if (!type.IsValueType)
        {
            if (!visited.Add(obj))
                return $"<cyclic reference to {type.Name}>" + Environment.NewLine;
        }

        if (obj is IEnumerable numerable)
        {
            return PrintEnumerable(numerable, type, level, config, visited);
        }

        var indentObj = new string('\t', level);
        var sbObj = new StringBuilder();

        sbObj.AppendLine(type.Name);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetIndexParameters().Length == 0);
        foreach (var property in properties)
        {
            if (config.ExcludedTypes.Contains(property.PropertyType)) continue;
            if (config.ExcludedProperties.Contains(property.Name)) continue;

            var value = property.GetValue(obj);
            var printed = PrintMember(property.Name, value, property.PropertyType,
                config, level + 1, visited);

            sbObj.Append(indentObj + "\t" + printed);
        }

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (config.ExcludedTypes.Contains(field.FieldType)) continue;
            if (config.ExcludedProperties.Contains(field.Name)) continue;

            var value = field.GetValue(obj);
            var printed = PrintMember(field.Name, value, field.FieldType,
                config, level + 1, visited);

            sbObj.Append(indentObj + "\t" + printed);
        }

        return sbObj.ToString();
    }

    private string PrintMember(
        string memberName,
        object value,
        Type memberType,
        IPrintingConfigInternal config,
        int level,
        HashSet<object> visited)
    {
        if (config.PropertySerializers.TryGetValue(memberName, out var serializer))
        {
            var val = serializer(value);
            return memberName + " = " + val + Environment.NewLine;
        }

        if (config.TypeSerializers.TryGetValue(memberType, out var typeSerializer))
        {
            var val = typeSerializer(value);
            return memberName + " = " + val + Environment.NewLine;
        }

        if (value is string s)
        {
            int? maxLen = null;
            
            if (config.TrimLengths.TryGetValue(memberName, out var propLen))
                maxLen = propLen;
            
            else if (config.GlobalStringTrimLength.HasValue)
                maxLen = config.GlobalStringTrimLength.Value;

            if (maxLen.HasValue && s.Length > maxLen.Value)
                s = s.Substring(0, maxLen.Value);

            return memberName + " = " + s + Environment.NewLine;
        }

        if (IsSimpleType(memberType))
            return memberName + " = " + value + Environment.NewLine;

        return memberName + " = " + PrintInternal(value, level, config, visited);
    }

    private string PrintEnumerable(
        IEnumerable enumerable,
        Type type,
        int level,
        IPrintingConfigInternal config,
        HashSet<object> visited)
    {
        var indent = new string('\t', level);
        var sb = new StringBuilder();

        sb.AppendLine(type.Name);

        if (enumerable is IDictionary nonGenericDict)
        {
            foreach (DictionaryEntry entry in nonGenericDict)
            {
                sb.Append(indent + "\t");
                sb.Append($"[{entry.Key}] = {PrintInternal(entry.Value, level + 1, config, visited)}");
            }
        }
        else
        {
            var i = 0;
            foreach (var item in enumerable)
            {
                sb.Append(indent + "\t");

                var itemType = item.GetType();
                if (itemType.IsGenericType &&
                    itemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    dynamic kv = item;
                    sb.Append($"[{kv.Key}] = {PrintInternal(kv.Value, level + 1, config, visited)}");
                }
                else
                {
                    sb.Append($"[{i}] = {PrintInternal(item, level + 1, config, visited)}");
                }

                i++;
            }
        }

        return sb.ToString();
    }

    private bool IsSimpleType(Type type)
    {
        if (type.IsPrimitive) return true;
        if (type == typeof(string)) return true;
        if (type == typeof(decimal)) return true;
        if (type == typeof(DateTime)) return true;
        if (type == typeof(TimeSpan)) return true;
        if (type.IsEnum) return true;

        return false;
    }
}