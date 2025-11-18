using System;

namespace ObjectPrinting;

public class MemberPrintingConfig<TOwner, TProp>
{
    private readonly PrintingConfig<TOwner> config;
    private readonly string memberName;

    internal MemberPrintingConfig(PrintingConfig<TOwner> config, string memberName)
    {
        this.config = config;
        this.memberName = memberName;
    }
    public PrintingConfig<TOwner> Using(Func<TProp, string> serializer)
    {
        config.PropertySerializers[memberName] = obj => serializer((TProp)obj);
        return config;
    }
    
    public PrintingConfig<TOwner> Trim(int maxLength)
    {
        if (typeof(TProp) != typeof(string))
            throw new InvalidOperationException("Trim только для строк");

        config.TrimLengths[memberName] = maxLength;
        return config;
    }
}