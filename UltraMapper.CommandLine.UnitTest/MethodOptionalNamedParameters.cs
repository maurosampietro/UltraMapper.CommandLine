using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UltraMapper.CommandLine.UnitTest
{
    [TestClass]
    public class MethodOptionalNamedParameters
    {
        private class Commands
        {
            public void NoOption( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a.Length == 0 );
                Assert.IsTrue( b.Length == 0 );
                Assert.IsTrue( c == -1 );
            }

            public void AllOptions( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a[ 0 ] == "aaa" );
                Assert.IsTrue( b[ 0 ] == "bbb" );
                Assert.IsTrue( c == 11 );
            }

            public void OptionA( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a[ 0 ] == "aaa" );
                Assert.IsTrue( b.Length == 0 );
                Assert.IsTrue( c == -1 );
            }

            public void OptionAEmpty( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a.Length == 0 );
                Assert.IsTrue( b.Length == 0 );
                Assert.IsTrue( c == -1 );
            }

            public void OptionB( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a.Length == 0 );
                Assert.IsTrue( b[ 0 ] == "bbb" );
                Assert.IsTrue( c == -1 );
            }

            public void OptionBEmpty( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a.Length == 0 );
                Assert.IsTrue( b.Length == 0 );
                Assert.IsTrue( c == -1 );
            }

            public void OptionC( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a.Length == 0 );
                Assert.IsTrue( b.Length == 0 );
                Assert.IsTrue( c == 11 );
            }

            public void OptionAB( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a[ 0 ] == "aaa" );
                Assert.IsTrue( b[ 0 ] == "bbb" );
                Assert.IsTrue( c == -1 );
            }

            public void OptionAC( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a[ 0 ] == "aaa" );
                Assert.IsTrue( b.Length == 0 );
                Assert.IsTrue( c == 11 );
            }

            public void OptionBC( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a.Length == 0 );
                Assert.IsTrue( b[ 0 ] == "bbb" );
                Assert.IsTrue( c == 11 );
            }

            public void OptionBA( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a[ 0 ] == "aaa" );
                Assert.IsTrue( b[ 0 ] == "bbb" );
                Assert.IsTrue( c == -1 );
            }

            public void OptionCA( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a[ 0 ] == "aaa" );
                Assert.IsTrue( b.Length == 0 );
                Assert.IsTrue( c == 11 );
            }

            public void OptionCB( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a.Length == 0 );
                Assert.IsTrue( b[ 0 ] == "bbb" );
                Assert.IsTrue( c == 11 );
            }
        }

        [TestMethod]
        public void NoOption()
        {
            var args = "--nooption";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void AllOptionsNamed()
        {
            var args = $"--{nameof( Commands.AllOptions )} a=[aaa] b=[bbb] c=11";
            var parsed = CommandLine.Instance.Parse<Commands>( args );

            args = $"--{nameof( Commands.AllOptions )} a=[aaa] c=11 b=[bbb]";
            parsed = CommandLine.Instance.Parse<Commands>( args );

            args = $"--{nameof( Commands.AllOptions )} b=[bbb] a=[aaa] c=11";
            parsed = CommandLine.Instance.Parse<Commands>( args );

            args = $"--{nameof( Commands.AllOptions )} b=[bbb] c=11 a=[aaa]";
            parsed = CommandLine.Instance.Parse<Commands>( args );

            args = $"--{nameof( Commands.AllOptions )} c=11 b=[bbb] a=[aaa]";
            parsed = CommandLine.Instance.Parse<Commands>( args );

            args = $"--{nameof( Commands.AllOptions )} c=11 b=[bbb] a=[aaa]";
            parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void AllOptionsOrder()
        {
            var args = $"--{nameof( Commands.AllOptions )} [aaa] [bbb] 11";
            var parsed = CommandLine.Instance.Parse<Commands>( args );

            Assert.ThrowsException<InvalidCastException>( () =>
            {
                args = $"--{nameof( Commands.AllOptions )} [aaa] 11 [bbb]";
                parsed = CommandLine.Instance.Parse<Commands>( args );
            } );

            Assert.ThrowsException<AssertFailedException>( () =>
            {
                args = $"--{nameof( Commands.AllOptions )} [bbb] [aaa] 11";
                parsed = CommandLine.Instance.Parse<Commands>( args );
            } );

            Assert.ThrowsException<InvalidCastException>( () =>
            {
                args = $"--{nameof( Commands.AllOptions )} [bbb] 11 [aaa]";
                parsed = CommandLine.Instance.Parse<Commands>( args );
            } );

            Assert.ThrowsException<InvalidCastException>( () =>
            {
                args = $"--{nameof( Commands.AllOptions )} 11 [bbb] [aaa]";
                parsed = CommandLine.Instance.Parse<Commands>( args );
            } );

            Assert.ThrowsException<InvalidCastException>( () =>
            {
                args = $"--{nameof( Commands.AllOptions )} 11 [bbb] [aaa]";
                parsed = CommandLine.Instance.Parse<Commands>( args );
            } );
        }

        [TestMethod]
        public void OptionA()
        {
            var args = $"--{nameof( Commands.OptionA )} a=[aaa]";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionAEmpty()
        {
            var args = $"--{nameof( Commands.OptionAEmpty )} a=[]";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionB()
        {
            var args = $"--{nameof( Commands.OptionB )} b=[bbb]";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionBEmpty()
        {
            var args = $"--{nameof( Commands.OptionBEmpty )} b=[]";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionC()
        {
            var args = $"--{nameof( Commands.OptionC )} c=11";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionAB()
        {
            var args = $"--{nameof( Commands.OptionAB )} a=[aaa] b=[bbb]";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionAC()
        {
            var args = $"--{nameof( Commands.OptionAC )} a=[aaa] c=11";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionBC()
        {
            var args = $"--{nameof( Commands.OptionBC )} b=[bbb] c=11";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionBA()
        {
            var args = $"--{nameof( Commands.OptionBA )} b=[bbb] a=[aaa]";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionCA()
        {
            var args = $"--{nameof( Commands.OptionCA )} c=11 a=[aaa]";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionCB()
        {
            var args = $"--{nameof( Commands.OptionCB )} c=11 b=[bbb]";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }
    }
}
