using System;
using System.Collections.Generic;

namespace ObjectPrinting;

public interface IPrintingConfigInternal
{
    HashSet<Type> ExcludedTypes { get; }
    HashSet<string> ExcludedProperties { get; }

    Dictionary<Type, Func<object, string>> TypeSerializers { get; }
    Dictionary<string, Func<object, string>> PropertySerializers { get; }
    Dictionary<string, int> TrimLengths { get; }
}
