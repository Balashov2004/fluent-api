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
    private readonly MemberPrinter _memberPrinter = new MemberPrinter();
    private readonly EnumerablePrinter _enumerablePrinter = new EnumerablePrinter();
    
    public string Print<TOwner>(TOwner obj, PrintingConfig<TOwner> config)
    {
        var visited = new HashSet<object>(new ReferenceEqualityComparer());
        return PrintInternal(obj, 0, config, visited);
    }

    internal string PrintInternal(
        object obj,
        int level,
        IPrintingConfigInternal config,
        HashSet<object> visited)
    {
        if (obj == null)
            return "null" + Environment.NewLine;

        var type = obj.GetType();

        if (TypeHelper.IsSimpleType(type))
            return obj + Environment.NewLine;

        if (!type.IsValueType && !visited.Add(obj))
            return $"<cyclic reference to {type.Name}>" + Environment.NewLine;

        if (obj is IEnumerable numerable)
            return _enumerablePrinter.PrintEnumerable(numerable, type, level, config, visited, this);
        

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
            sbObj.Append(indentObj + "\t" +
                         _memberPrinter.PrintMember(property.Name, value, property.PropertyType, config, level + 1, visited, this));
        }
        
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (config.ExcludedTypes.Contains(field.FieldType)) continue;
            if (config.ExcludedProperties.Contains(field.Name)) continue;

            var value = field.GetValue(obj);
            sbObj.Append(indentObj + "\t" +
                         _memberPrinter.PrintMember(field.Name, value, field.FieldType, config, level + 1, visited, this));
        }

        return sbObj.ToString();
    }
}
