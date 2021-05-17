using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UltraMapper.CommandLine.UnitTest
{
    [TestClass]
    public class MethodOptionalParametersOrder
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
                Assert.IsTrue( a[ 0 ] == "bbb" );
                Assert.IsTrue( b.Length == 0 );
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

            [Option( Name = "EXEC2" )]
            public void Elab2( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a.Length == 0 );
                Assert.IsTrue( b[ 0 ] == "bbb" );
                Assert.IsTrue( c == 1 );
            }

            [Option( Name = "EXEC3" )]
            public void Elab3( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a.Length == 0 );
                Assert.IsTrue( b[ 0 ] == "bbb" );
                Assert.IsTrue( c == 1 );
            }

            [Option( Name = "EXEC4" )]
            public void Elab4( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a[ 0 ] == "89" );
                Assert.IsTrue( b.Length == 0 );
                Assert.IsTrue( c == -1 );
            }

            [Option( Name = "EXEC5" )]
            public void Elab5( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a[ 0 ] == "89" );
                Assert.IsTrue( b.Length == 0 );
                Assert.IsTrue( c == -1 );
            }

            [Option( Name = "EXEC6" )]
            public void Elab6( string[] a = null, string[] b = null, int c = -1 )
            {
                Assert.IsTrue( a[ 0 ] == "11" );
            }
        }

        [TestMethod]
        public void NoOption()
        {
            var args = "--nooption";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
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
            var args = $"--{nameof( Commands.OptionA )} [aaa]";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionAEmpty()
        {
            var args = $"--{nameof( Commands.OptionAEmpty )} []";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionB()
        {
            var args = $"--{nameof( Commands.OptionB )} [bbb]";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionBEmpty()
        {
            var args = $"--{nameof( Commands.OptionBEmpty )} []";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionC()
        {
            Assert.ThrowsException<InvalidCastException>( () =>
            {
                var args = $"--{nameof( Commands.OptionC )} 11";
                var parsed = CommandLine.Instance.Parse<Commands>( args );
            } );
        }

        [TestMethod]
        public void OptionAB()
        {
            var args = $"--{nameof( Commands.OptionAB )} [aaa] [bbb]";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void OptionAC()
        {
            Assert.ThrowsException<InvalidCastException>( () =>
            {
                var args = $"--{nameof( Commands.OptionAC )} [aaa] 11";
                var parsed = CommandLine.Instance.Parse<Commands>( args );
            } );
        }

        [TestMethod]
        public void OptionBC()
        {
            Assert.ThrowsException<InvalidCastException>( () =>
            {
                var args = $"--{nameof( Commands.OptionBC )} [bbb] 11";
                var parsed = CommandLine.Instance.Parse<Commands>( args );
            } );
        }

        [TestMethod]
        public void OptionBA()
        {
            Assert.ThrowsException<AssertFailedException>( () =>
            {
                var args = $"--{nameof( Commands.OptionBA )} [bbb] [aaa]";
                var parsed = CommandLine.Instance.Parse<Commands>( args );
            } );
        }

        [TestMethod]
        public void OptionCA()
        {
            Assert.ThrowsException<InvalidCastException>( () =>
            {
                var args = $"--{nameof( Commands.OptionCA )} 11 [aaa]";
                var parsed = CommandLine.Instance.Parse<Commands>( args );
            } );
        }

        [TestMethod]
        public void OptionCB()
        {
            Assert.ThrowsException<InvalidCastException>( () =>
            {
                var args = $"--{nameof( Commands.OptionCB )} 11 [bbb]";
                var parsed = CommandLine.Instance.Parse<Commands>( args );
            } );
        }

        [TestMethod]
        public void Bug1dot1()
        {
            var args = "--EXEC2 [] [bbb] 1";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void Bug1dot2()
        {
            var args = "--EXEC3 [] [bbb] 1";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void Bug1dot3()
        {
            var args = "--EXEC4 [89] [] ";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void Bug1dot4()
        {
            var args = "--EXEC5 [89] ";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }

        [TestMethod]
        public void Bug1dot6()
        {
            var args = "--EXEC6 [11]";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
        }
    }
}
