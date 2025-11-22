using System;
using System.Globalization;

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
            throw new InvalidOperationException("Trim can be used only on string properties");

        config.TrimLengths[memberName] = maxLength;
        return config;
    }

    public PrintingConfig<TOwner> Using(CultureInfo culture)
    {
        if (!typeof(IFormattable).IsAssignableFrom(typeof(TProp)))
            throw new InvalidOperationException(
                "Culture can be applied only to IFormattable properties");

        config.PropertySerializers[memberName] = obj =>
            ((IFormattable)obj).ToString(null, culture);

        return config;
    }
}