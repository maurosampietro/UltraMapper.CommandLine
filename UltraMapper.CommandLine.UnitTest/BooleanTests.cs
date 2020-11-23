using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CommandLine.AutoParser.UnitTest
{
    [TestClass]
    public class BooleanTests
    {
        public class Commands
        {
            public bool Open { get; set; }
        }

        [TestMethod]
        public void ExplicitSetToTrue()
        {
            string args = "--open true";
            var parsed = AutoParser.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Open );
        }

        [TestMethod]
        public void ExplicitSetToFalse()
        {
            string args = "--open false";
            var parsed = AutoParser.Instance.Parse<Commands>( args );
            Assert.IsTrue( !parsed.Open );
        }

        [TestMethod]
        public void ImplicitSetToTrue()
        {
            string args = "--open";
            var parsed = AutoParser.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Open );
        }

        [TestMethod]
        public void WrongArg()
        {
            string args = "--open wrongarg";
            Assert.ThrowsException<ArgumentException>( () =>
                AutoParser.Instance.Parse<Commands>( args ) );
        }
    }
}
