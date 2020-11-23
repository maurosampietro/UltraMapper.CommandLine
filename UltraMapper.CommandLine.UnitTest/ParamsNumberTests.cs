using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CommandLine.AutoParser.UnitTest
{
    [TestClass]
    public class ParamsNumberTests
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
            Assert.ThrowsException<ArgumentException>( () =>
                AutoParser.Instance.Parse<Commands0>( args ) );
        }

        [TestMethod]
        public void MoreParamsThanRequiredPlusOptionals()
        {
            var args = $"--{nameof( Commands0.Command1 )} a b c d";

            Assert.ThrowsException<ArgumentException>( () =>
                AutoParser.Instance.Parse<Commands0>( args ) );
        }

        [TestMethod]
        public void RequiredPlusOptionalParams()
        {
            var args = $"--{nameof( Commands0.Command1 )} a b c";
            AutoParser.Instance.Parse<Commands0>( args );
        }
    }
}
