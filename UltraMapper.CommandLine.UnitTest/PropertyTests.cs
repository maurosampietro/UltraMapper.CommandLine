using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UltraMapper.CommandLine.UnitTest
{
    [TestClass]
    public class PropertyTests
    {
        public class Commands
        {
            public class MoveCommand
            {
                [Option( IsRequired = true, Order = 0 )]
                public string From { get; set; }

                [Option( IsRequired = true, Order = 1 )]
                public string To { get; set; }
            }

            public class ExchangeCommand
            {
                [Option( IsRequired = false )]
                public string A { get; set; }
                [Option( IsRequired = false )]
                public string B { get; set; }
            }

            public bool Open { get; set; }
            public MoveCommand Move { get; set; }
            public ExchangeCommand Exchange { get; set; }
        }

        public class DuplicateCommandNames
        {
            public string Move { get; set; }

            [Option( Name = "Move" )]
            public void MoveCommand( string a, string b )
            {
            }
        }

        [TestMethod]
        public void MultipleCommandWithSameOption()
        {
            var args = "--move a b";
            Assert.ThrowsException<ArgumentException>(
                () => CommandLine.Instance.Parse<DuplicateCommandNames>( args ) );
        }

        [TestMethod]
        public void WrongTypeOfArg()
        {
            var args = "--open thisisnotcovertibletoboolean";
            Assert.ThrowsException<ArgumentException>(
                () => CommandLine.Instance.Parse<Commands>( args ) );
        }

        [TestMethod]
        public void WrongNumberOfArgs()
        {
            var args = "--open false true false";
            Assert.ThrowsException<ArgumentException>(
                () => CommandLine.Instance.Parse<Commands>( args ) );
        }

        [TestMethod]
        public void NamedParamsExactOrder()
        {
            var args = "--move from=fromhere to=tohere";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Move.From == "fromhere" );
            Assert.IsTrue( parsed.Move.To == "tohere" );
        }

        [TestMethod]
        public void MultipleIdenticalNamedParams()
        {
            var args = "--move from=fromhere to=tohere from=fromhere2 to=tohere2";
            Assert.ThrowsException<DuplicateArgumentException>(
                () => CommandLine.Instance.Parse<Commands>( args ) );
        }

        [TestMethod]
        public void MultipleIdenticalNamedParamsDifferentCases()
        {
            var args = "--move from=fromhere TO=tohere FROM=fromhere2 to=tohere2";
            Assert.ThrowsException<DuplicateArgumentException>(
                () => CommandLine.Instance.Parse<Commands>( args ) );
        }

        [TestMethod]
        public void MultipleIdenticalNamedParamsNested()
        {
            var args = "--move from=fromhere to=tohere (from=something (a=a a=b))";
            Assert.ThrowsException<DuplicateArgumentException>(
                () => CommandLine.Instance.Parse<Commands>( args ) );
        }

        [TestMethod]
        public void NamedParamsSparseOrder()
        {
            var args = "--move to=tohere from=fromhere";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Move.From == "fromhere" );
            Assert.IsTrue( parsed.Move.To == "tohere" );
        }

        [TestMethod]
        public void MixedNamedNonNamedParams()
        {
            var args = "--move fromhere to=tohere";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Move.From == "fromhere" );
            Assert.IsTrue( parsed.Move.To == "tohere" );
        }

        [TestMethod]
        public void MixedNamedNonNamedParamsWrongOrder()
        {
            var args = "--move to=tohere fromhere";

            Assert.ThrowsException<ArgumentException>(
                () => CommandLine.Instance.Parse<Commands>( args ) );
        }

        [TestMethod]
        public void MissingParam()
        {
            var args = "--move fromhere";

            Assert.ThrowsException<ArgumentException>(
                () => CommandLine.Instance.Parse<Commands>( args ) );

            args = "--move to=toherehere";
            Assert.ThrowsException<ArgumentException>(
                () => CommandLine.Instance.Parse<Commands>( args ) );
        }

        [TestMethod]
        public void Optional()
        {
            var args = "--exchange a=this";
            var parsed = CommandLine.Instance.Parse<Commands>( args );

            Assert.IsTrue( parsed.Exchange.A == "this" );
            Assert.IsTrue( String.IsNullOrEmpty( parsed.Exchange.B ) );

            args = "--exchange b=that";
            parsed = CommandLine.Instance.Parse<Commands>( args );
            Assert.IsTrue( String.IsNullOrEmpty( parsed.Exchange.A ) );
            Assert.IsTrue( parsed.Exchange.B == "that" );
        }

        [TestMethod]
        public void Optional2()
        {
            var args = "--exchange b=that a=this";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Exchange.A == "this" );
            Assert.IsTrue( parsed.Exchange.B == "that" );
        }

        [TestMethod]
        public void NonExistingNamedParam()
        {
            var args = "--move SomeWrongParam=this SomeOtherWrongParam=that";
            Assert.ThrowsException<UndefinedParameterException>(
                () => CommandLine.Instance.Parse<Commands>( args ) );
        }

        [TestMethod]
        public void NonExistingCommand()
        {
            var args = "--someNonExistingCommand SomeWrongParam=this SomeOtherWrongParam=that";
            Assert.ThrowsException<UndefinedParameterException>(
                 () => CommandLine.Instance.Parse<Commands>( args ) );
        }
    }
}
