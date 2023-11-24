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

        [TestMethod]
        public void Basic()
        {
            var command = "--DateTime 2023-11-24";
            var result = CommandLine.Instance.Parse<CommandLineTestClass>( command );

            Assert.IsTrue( result.DateTime.Equals( new DateTime( 2023, 11, 24 ) ) );
        }        
    }
}
