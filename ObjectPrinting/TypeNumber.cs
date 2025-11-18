using System;
using System.Globalization;

namespace ObjectPrinting;

public class TypeNumber
{
    public static Func<double, string> WithDot =>
        d => d.ToString(CultureInfo.InvariantCulture);
    public static Func<double, string> WithComma =>
        d => d.ToString(new CultureInfo("ru-RU"));
}