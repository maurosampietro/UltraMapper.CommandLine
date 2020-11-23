using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommandLine.AutoParser.UnitTest
{
    [TestClass]
    public class OrderOverride
    {
        public class CommandsOrderOverride
        {
            public class MoveCommand
            {
                [Option( IsRequired = true, Order = 1 )]
                public string From { get; set; }

                [Option( IsRequired = true, Order = 0 )]
                public string To { get; set; }
            }

            public MoveCommand Move { get; set; }
        }

        public class CommandsOrderOverrideWithOptionalParams
        {
            public class MoveCommand
            {
                [Option( IsRequired = true, Order = 1 )]
                public string From { get; set; }

                [Option( IsRequired = false, Order = 0 )]
                public string To { get; set; }
            }

            public MoveCommand Move { get; set; }
        }

        [TestMethod]
        public void RequiredConvention()
        {
            var args = "--move tohere fromhere";
            var parsed = AutoParser.Instance.Parse<CommandsOrderOverride>( args );
            Assert.IsTrue( parsed.Move.From == "fromhere" );
            Assert.IsTrue( parsed.Move.To == "tohere" );
        }

        [TestMethod]
        public void RequiredConvention2()
        {
            var args = "--move fromhere tohere";
            var parsed = AutoParser.Instance.Parse<CommandsOrderOverrideWithOptionalParams>( args );
            Assert.IsTrue( parsed.Move.From == "fromhere" );
            Assert.IsTrue( parsed.Move.To == "tohere" );
        }
    }
}
