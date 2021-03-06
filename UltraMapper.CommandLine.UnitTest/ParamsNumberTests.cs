﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UltraMapper.CommandLine.UnitTest
{
    [TestClass]
    //Tests the behavior when passing a wrong number of params to commands
    public class CommandParamsNumberTests
    {
        private class Commands0
        {
            public class Level1
            {
                public string A { get; set; }
                public string B { get; set; }
                [Option( IsRequired = false )]
                public string C { get; set; }
            }

            public Level1 Command1 { get; set; }
        }

        [TestMethod]
        public void RequiredParamsMissing()
        {
            var args = $"--{nameof( Commands0.Command1 )}";
            Assert.ThrowsException<ArgumentNumberException>( () =>
                CommandLine.Instance.Parse<Commands0>( args ) );
        }

        [TestMethod]
        public void MoreParamsThanRequiredPlusOptionals()
        {
            var args = $"--{nameof( Commands0.Command1 )} a b c d";

            Assert.ThrowsException<ArgumentNumberException>( () =>
                CommandLine.Instance.Parse<Commands0>( args ) );
        }

        [TestMethod]
        public void RequiredPlusOptionalParams()
        {
            var args = $"--{nameof( Commands0.Command1 )} a b c";
            CommandLine.Instance.Parse<Commands0>( args );
        }
    }

    [TestClass]
    //Tests the behavior when passing a wrong number of params
    //to a first-level param
    public class InnerParamsNumberTests
    {
        private class Commands0
        {
            public class Level1
            {
                public string A { get; set; }
                public string B { get; set; }
                [Option( IsRequired = false )]
                public string C { get; set; }
            }

            public Level1 Command1 { get; set; }
        }

        [TestMethod]
        public void RequiredParamsMissing()
        {
            var args = $"--{nameof( Commands0.Command1 )}";
            Assert.ThrowsException<ArgumentNumberException>( () =>
                CommandLine.Instance.Parse<Commands0>( args ) );
        }

        [TestMethod]
        public void MoreParamsThanRequiredPlusOptionals()
        {
            var args = $"--{nameof( Commands0.Command1 )} a b c d";

            Assert.ThrowsException<ArgumentNumberException>( () =>
                CommandLine.Instance.Parse<Commands0>( args ) );
        }

        [TestMethod]
        public void RequiredPlusOptionalParams()
        {
            var args = $"--{nameof( Commands0.Command1 )} a b c";
            CommandLine.Instance.Parse<Commands0>( args );
        }
    }
}
