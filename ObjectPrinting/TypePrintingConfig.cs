using System;

namespace ObjectPrinting;

public class TypePrintingConfig<TOwner, TProp>
{
    private readonly PrintingConfig<TOwner> config;

    internal TypePrintingConfig(PrintingConfig<TOwner> config)
    {
        this.config = config;
    }
    
    public PrintingConfig<TOwner> Using(Func<TProp, string> serializer)
    {
        config.TypeSerializers[typeof(TProp)] = obj => serializer((TProp)obj);
        return config;
    }
}