using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UltraMapper.CommandLine.Tests
{
    [TestClass]
    public class DateTimeTests
    {
        public class CommandLineTestClass
        {
            public DateTime DateTime { get; set; }
        }

        public class CommandLineTestClass2
        {
            public Inner InnerType { get; set; } = new Inner();

            public class Inner
            {
                public DateTime DateTime { get; set; }        
            }
        }

        [TestMethod]
        public void Basic()
        {
            var command = $"--{nameof( CommandLineTestClass.DateTime )} 2023-11-24";
            var result = CommandLine.Instance.Parse<CommandLineTestClass>( command );

            Assert.IsTrue( result.DateTime.Equals( new DateTime( 2023, 11, 24 ) ) );
        }

        [TestMethod]
        public void NestedComplexSyntax()
        {
            var command = $"--{nameof( CommandLineTestClass2.InnerType )} (2023-11-11)";
            var result = CommandLine.Instance.Parse<CommandLineTestClass2>( command );

            Assert.IsTrue( result.InnerType.DateTime.Equals( new DateTime( 2023, 11, 24 ) ) );
        }

        [TestMethod]
        public void NestedSimpleSyntax()
        {
            //using complex type syntax, custom converters is not used.
            var command = $"--{nameof( CommandLineTestClass2.InnerType )} 2023-11-11";
            var result = CommandLine.Instance.Parse<CommandLineTestClass2>( command );

            Assert.IsTrue( result.InnerType.DateTime.Equals( new DateTime( 2023, 11, 24 ) ) );
        }
    }
}
