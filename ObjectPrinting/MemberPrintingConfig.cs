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
        config.TrimLengths[memberName] = maxLength;
        return config;
    }
}