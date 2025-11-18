using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        public string PrintToString(TOwner obj)
        {
            var visited = new HashSet<object>(new ReferenceEqualityComparer());
            return PrintToString(obj, 0, visited);
        }

        private string PrintToString(object obj, int nestingLevel, HashSet<object> visited)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            var type = obj.GetType();
            
            if (IsFinalType(type))
                return obj.ToString() + Environment.NewLine;
            
            if (visited.Contains(obj))
                return $"<cyclic reference to {type.Name}>" + Environment.NewLine;

            visited.Add(obj);
            

            var sb = new StringBuilder();
            var indent = new string('\t', nestingLevel);
            sb.AppendLine(type.Name);

            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.GetIndexParameters().Length == 0))
            {
                object value = property.GetValue(obj);
                
                sb.Append(indent + "\t" +  property.Name + " = " + PrintToString(value, nestingLevel + 1, visited));
            }

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = field.GetValue(obj);
                sb.Append(indent + "\t" + field.Name + " = " +
                         PrintToString(value, nestingLevel + 1, visited));
            }
            
            visited.Remove(obj);

            
            return sb.ToString();
        }
        
        private static bool IsFinalType(Type type)
        {
            if (type.IsPrimitive) return true;
            if (type.IsEnum) return true;
            if (type == typeof(string)) return true;
            if (type == typeof(decimal)) return true;
            if (type == typeof(DateTime)) return true;
            if (type == typeof(TimeSpan)) return true;

            return false;
        }

    }
}