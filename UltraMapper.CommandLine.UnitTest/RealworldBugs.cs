using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UltraMapper.CommandLine.UnitTest
{
    [TestClass]
    public class RealworldBugs
    {
        public class Commands2
        {
            public class InnerType
            {
                [Option( IsRequired = true, Order = 0 )]
                public string A { get; set; }

                [Option( IsRequired = true, Order = 1 )]
                public string B { get; set; }

                [Option( IsRequired = false )]
                public InnerType Inner2 { get; set; }
            }

            public InnerType Move { get; set; }
        }

        [TestMethod]
        public void DuplicateArgumentInnerParamAndCircularReference()
        {
            var args = "--move a=fromhere b=tohere inner2=(a=a a=b)";
            Assert.ThrowsException<DuplicateArgumentException>(
                () => CommandLine.Instance.Parse<Commands2>( args ) );
        }

        [TestMethod]
        public void CircularReference()
        {
            var args = "--move a=a b=b inner2=(a=aa b=bb inner2=(a=aaa b=bbb inner2=(a=aaaa b=bbbb)))";
            var parsed = CommandLine.Instance.Parse<Commands2>( args );

            Assert.IsTrue( parsed.Move.A == "a" );
            Assert.IsTrue( parsed.Move.B == "b" );
            Assert.IsTrue( parsed.Move.Inner2.A == "aa" );
            Assert.IsTrue( parsed.Move.Inner2.B == "bb" );
            Assert.IsTrue( parsed.Move.Inner2.Inner2.A == "aaa" );
            Assert.IsTrue( parsed.Move.Inner2.Inner2.B == "bbb" );
            Assert.IsTrue( parsed.Move.Inner2.Inner2.Inner2.A == "aaaa" );
            Assert.IsTrue( parsed.Move.Inner2.Inner2.Inner2.B == "bbbb" );
        }

        [TestMethod]
        [Ignore] //this should throw and it's ok, but it's not easy to understand where the problem is
        public void ComplexAnonymousParamOnSimpleParam()
        {
            var args = "--move (a=aaa b=bbb) a=aa b=bb";
            var parsed = CommandLine.Instance.Parse<Commands2>( args );
        }

        [TestMethod]
        public void Bug1dot0()
        {
            var args = "--EXEC1 a=[aaa] b=[bbb] x=1";
            var parsed = CommandLine.Instance.Parse<ElabCommands>( args );
        }

        [TestMethod]
        public void Bug1dot1()
        {
            var args = "--EXEC2 a=[] b=[bbb] x=1";
            var parsed = CommandLine.Instance.Parse<ElabCommands>( args );
        }

        [TestMethod]
        public void Bug1dot2()
        {
            var args = "--EXEC3 [] [bbb] 1";
            var parsed = CommandLine.Instance.Parse<ElabCommands>( args );
        }
    }

    public class ElabCommands
    {
        [Option( Name = "EXEC1" )]
        public void Elab1( string[] a = null, string[] b = null, int x = -1 )
        {
            Assert.IsTrue( a[ 0 ] == "aaa" );
            Assert.IsTrue( b[ 0 ] == "bbb" );
            Assert.IsTrue( x == 1 );
        }

        [Option( Name = "EXEC2" )]
        public void Elab2( string[] a = null, string[] b = null, int x = -1 )
        {
            Assert.IsTrue( a.Length == 0 );
            Assert.IsTrue( b[ 0 ] == "bbb" );
            Assert.IsTrue( x == 1 );
        }

        [Option( Name = "EXEC3" )]
        public void Elab3( string[] a = null, string[] b = null, int x = -1 )
        {
            Assert.IsTrue( a.Length == 0 );
            Assert.IsTrue( b[ 0 ] == "bbb" );
            Assert.IsTrue( x == 1 );
        }
    }
}
