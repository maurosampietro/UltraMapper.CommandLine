using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UltraMapper.CommandLine.UnitTest
{
    [TestClass]
    public class BooleanTests
    {
        public class Commands
        {
            public bool Open { get; set; }
        }

        [TestMethod]
        [Ignore]
        public void ExplicitSetToTrueViaConstant1()
        {
            string args = "--open 1";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Open );
        }

        [TestMethod]
        [Ignore]
        public void ExplicitSetToFalseViaConstant0()
        {
            string args = "--open 0";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
            Assert.IsTrue( !parsed.Open );
        }

        [TestMethod]
        public void ExplicitSetToTrue()
        {
            string args = "--open true";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Open );
        }

        [TestMethod]
        public void ExplicitSetToFalse()
        {
            string args = "--open false";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
            Assert.IsTrue( !parsed.Open );
        }

        [TestMethod]
        public void ImplicitSetToTrue()
        {
            string args = "--open";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Open );
        }

        [TestMethod]
        public void WrongArg()
        {
            string args = "--open wrongarg";
            Assert.ThrowsException<FormatException>( () =>
                CommandLine.Instance.Parse<Commands>( args ) );
        }
    }
}
