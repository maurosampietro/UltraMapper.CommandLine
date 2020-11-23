using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CommandLine.AutoParser.UnitTest
{
    [TestClass]
    public class IntegerTests
    {
        public class Commands
        {
            public int Option { get; set; }
        }

        [TestMethod]
        public void SetToInteger()
        {
            string args = $"--{nameof( Commands.Option )} 1";
            var parsed = AutoParser.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Option == 1 );
        }

        [TestMethod]
        public void SetToNegativeInteger()
        {
            string args = $"--{nameof( Commands.Option )} -1";
            var parsed = AutoParser.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Option == -1 );
        }

        [TestMethod]
        public void SetToNegativeIntegerFromQuotedString()
        {
            string args = $@"--{nameof( Commands.Option )} ""-1""";
            var parsed = AutoParser.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Option == -1 );
        }

        [TestMethod]
        public void SetToIntegerFromDecimalNumber()
        {
            string args = $"--{nameof( Commands.Option )} 1.0";
            Assert.ThrowsException<ArgumentException>( () =>
            {
                var parsed = AutoParser.Instance.Parse<Commands>( args );
                Assert.IsTrue( parsed.Option == 0 );
            } );
        }

        [TestMethod]
        public void SetToNegativeIntegerFromDecimalNumber()
        {
            string args = $"--{nameof( Commands.Option )} -1.0";
            Assert.ThrowsException<ArgumentException>( () =>
            {
                var parsed = AutoParser.Instance.Parse<Commands>( args );
                Assert.IsTrue( parsed.Option == 0 );
            } );
        }

        [TestMethod]
        public void WrongArg()
        {
            string args = $"--{nameof( Commands.Option )} suka";
            Assert.ThrowsException<ArgumentException>( () =>
            {
                var parsed = AutoParser.Instance.Parse<Commands>( args );
                Assert.IsTrue( parsed.Option == 0 );
            } );
        }
    }
}
