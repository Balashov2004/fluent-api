using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ObjectPrinting.Interface;

namespace ObjectPrinting;

public class PrintingConfig<TOwner> : IPrintingConfigInternal
{
    public HashSet<Type> ExcludedTypes { get; } = new();
    public HashSet<string> ExcludedProperties { get; } = new();

    public Dictionary<Type, Func<object, string>> TypeSerializers { get; } = new();
    public Dictionary<string, Func<object, string>> PropertySerializers { get; } = new();
    public Dictionary<string, int> TrimLengths { get; } = new();
    public int? GlobalStringTrimLength { get; private set; }
    
    private ObjectSerializer objectSerializer = new ObjectSerializer();
    
    public PrintingConfig<TOwner> TrimStringsToLength(int maxLength)
    {
        GlobalStringTrimLength = maxLength;
        return this;
    }


    public PrintingConfig<TOwner> Excluding<TProp>()
    {
        ExcludedTypes.Add(typeof(TProp));
        return this;
    }

    public PrintingConfig<TOwner> Excluding<TProp>(Expression<Func<TOwner, TProp>> selector)
    {
        var name = ((MemberExpression)selector.Body).Member.Name;
        ExcludedProperties.Add(name);
        return this;
    }

    public PrintingConfig<TOwner> Printing<TProp>(Func<TProp, string> selector)
    {
        TypeSerializers[typeof(TProp)] = o => selector((TProp)o);
        return this;
    }

    public MemberPrintingConfig<TOwner, TProp> SelectMember<TProp>(
        Expression<Func<TOwner, TProp>> selector)
    {
        var name = ((MemberExpression)selector.Body).Member.Name;
        return new MemberPrintingConfig<TOwner, TProp>(this, name);
    }

    public string PrintToString(TOwner obj)
    {
        return objectSerializer.Print(obj, this);
    }
}