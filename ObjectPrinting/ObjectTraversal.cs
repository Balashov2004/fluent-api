using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public static class ObjectTraversal
    {
        public static string Print<TOwner>(TOwner obj, PrintingConfig<TOwner> config)
        {
            var visited = new HashSet<object>(new ReferenceEqualityComparer());
            return PrintInternal(obj, 0, config, visited);
        }

        private static string PrintInternal(
            object obj,
            int level,
            IPrintingConfigInternal config,
            HashSet<object> visited)
        {
            if (obj == null)
                return "null\n";

            var type = obj.GetType();

            if (IsFinalType(type))
                return obj + "\n";
            
            if (!type.IsValueType)
            {
                if (visited.Contains(obj))
                    return $"<cyclic reference to {type.Name}>\n";

                visited.Add(obj);
            }

            if (obj is IEnumerable numerable && obj is not string)
            {
                return PrintEnumerable(numerable, type, level, config, visited);
            }

            string indentObj = new string('\t', level);
            var sbObj = new StringBuilder();

            sbObj.AppendLine(type.Name);
            
            
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.GetIndexParameters().Length == 0))
            {
                if (config.ExcludedTypes.Contains(property.PropertyType)) continue;
                if (config.ExcludedProperties.Contains(property.Name)) continue;

                var value = property.GetValue(obj);
                string printed = PrintMember(property.Name, value, property.PropertyType,
                    config, level + 1, visited);

                sbObj.Append(indentObj + "\t" + printed);
            }
            
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (config.ExcludedTypes.Contains(field.FieldType)) continue;
                if (config.ExcludedProperties.Contains(field.Name)) continue;

                var value = field.GetValue(obj);
                string printed = PrintMember(field.Name, value, field.FieldType,
                    config, level + 1, visited);

                sbObj.Append(indentObj + "\t" + printed);
            }

            return sbObj.ToString();
        }

        private static string PrintMember(
            string memberName,
            object value,
            Type memberType,
            IPrintingConfigInternal config,
            int level,
            HashSet<object> visited)
        {
            if (config.PropertySerializers.TryGetValue(memberName, out var serializer))
            {
                string val = serializer(value);
                return memberName + " = " + val + "\n";
            }
            
            if (config.TypeSerializers.TryGetValue(memberType, out var typeSerializer))
            {
                string val = typeSerializer(value);
                return memberName + " = " + val + "\n";
            }
            
            if (config.TrimLengths.TryGetValue(memberName, out var maxLen) && value is string s)
            {
                if (s.Length > maxLen)
                    s = s.Substring(0, maxLen);

                return memberName + " = " + s + "\n";
            }

            if (value == null)
                return memberName + " = null\n";

            if (IsFinalType(memberType))
                return memberName + " = " + value + "\n";
            
            return memberName + " = " + PrintInternal(value, level, config, visited);
        }
        
        private static string PrintEnumerable(
            IEnumerable enumerable,
            Type type,
            int level,
            IPrintingConfigInternal  config,
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
                int i = 0;
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

        private static bool IsFinalType(Type type)
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
}
