
using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace ObjectPrinting.Tests;

public class Test
{
    [Test]
    public void PrintToString_ShouldHandleBasicObject()
    {
        var person = new Person
        {
            Name = "Alex",
            Age = 30
        };
        
        var result = ObjectPrinter.For<Person>().PrintToString(person);

        StringAssert.Contains("Person", result);
        StringAssert.Contains("Name = Alex", result);
        StringAssert.Contains("Age = 30", result); 
        StringAssert.Contains("Age = 30", result);
    }
}