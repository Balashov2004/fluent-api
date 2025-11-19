// using System;
// using NUnit.Framework;
//
// namespace ObjectPrinting.Tests
// {
//     [TestFixture]
//     public class ObjectPrinterAcceptanceTests
//     {
//         [Test]
//         public void Demo()
//         {
//             var person = new Person { Name = "Alex", Age = 19 };
//
//             var printer = ObjectPrinter.For<Person>()
//                 //1. Исключить из сериализации свойства определенного типа
//                 .Excluding<Guid>()
//                 //2. Указать альтернативный способ сериализации для определенного типа
//                 .Printing<int>().Using(i => $"Num is {i}")
//                 //3. Для числовых типов указать культуру
//                 .Printing<double>(TypeNumber.WithDot)
//                 //4. Настроить сериализацию конкретного свойства
//                 .SelectMember(p => p.Name).Using(i => $"Age is {i}")
//                 //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
//                 .SelectMember(p => p.Name).Trim(3)
//                 //6. Исключить из сериализации конкретного свойства
//                 .Excluding(info => info.Age);
//             
//             printer.PrintToString(person);
//             //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
//             person.PrintToString();
//             //8. ...с конфигурированием
//             person.PrintToString(info => info.Excluding(p => p.Age));
//         }
//     }
// }