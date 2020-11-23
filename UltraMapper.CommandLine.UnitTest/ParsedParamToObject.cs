using CommandLine.AutoParser.Parsers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CommandLine.AutoParser.UnitTest
{
    [TestClass]
    public class ParsedParamToObject
    {
        public class T0
        {
            public string A { get; set; }
            public string B { get; set; }
        }

        public class T1
        {
            public T0 T0 { get; set; }

            public string C { get; set; }
            public string D { get; set; }
        }

        public class T2
        {
            public T1[] T1 { get; set; }
            public string[] Strings { get; set; }
            public int[] Ints { get; set; }
            public double[] Doubles { get; set; }
        }

        [TestMethod]
        public void Test0()
        {
            var mapper = new CommandLine.AutoParser.ParsedParamToObjectViaReflection();

            var parser = new CommandParser();
            var parsedParams = parser.Parse( "--cmd (a b)" );

            var t0 = mapper.Map<T0>( parsedParams.First().Params.ToArray() );

            Assert.IsTrue( t0.A == "a" );
            Assert.IsTrue( t0.B == "b" );
        }

        [TestMethod]
        public void Test1()
        {
            var mapper = new CommandLine.AutoParser.ParsedParamToObjectViaReflection();

            var parser = new CommandParser();
            var parsedParams = parser.Parse( "--cmd ((a b) c d)" );

            var t1 = mapper.Map<T1>( parsedParams.First().Params.ToArray() );

            Assert.IsTrue( t1.C == "c" );
            Assert.IsTrue( t1.D == "d" );
            Assert.IsTrue( t1.T0.A == "a" );
            Assert.IsTrue( t1.T0.B == "b" );
        }

        [TestMethod]
        public void Test2()
        {
            var mapper = new CommandLine.AutoParser.ParsedParamToObjectViaReflection();

            var parser = new CommandParser();
            var parsedParams = parser.Parse( "--cmd [((a0 b0) c0 d0) ((a1 b1) c1 d1)] [sa sb sc] [1 2 3] [1.1 2.2 3.3]" );

            var t2 = mapper.Map<T2>( parsedParams.First().Params.ToArray() );

            Assert.IsTrue( t2.T1[ 0 ].T0.A == "a0" );
            Assert.IsTrue( t2.T1[ 0 ].T0.B == "b0" );
            Assert.IsTrue( t2.T1[ 0 ].D == "d0" );
            Assert.IsTrue( t2.T1[ 0 ].C == "c0" );

            Assert.IsTrue( t2.T1[ 1 ].T0.A == "a1" );
            Assert.IsTrue( t2.T1[ 1 ].T0.B == "b1" );
            Assert.IsTrue( t2.T1[ 1 ].C == "c1" );
            Assert.IsTrue( t2.T1[ 1 ].D == "d1" );

            Assert.IsTrue( t2.Strings[ 0 ] == "sa" );
            Assert.IsTrue( t2.Strings[ 1 ] == "sb" );
            Assert.IsTrue( t2.Strings[ 2 ] == "sc" );

            Assert.IsTrue( t2.Ints[ 0 ] == 1 );
            Assert.IsTrue( t2.Ints[ 1 ] == 2 );
            Assert.IsTrue( t2.Ints[ 2 ] == 3 );

            Assert.IsTrue( t2.Doubles[ 0 ] == 1.1 );
            Assert.IsTrue( t2.Doubles[ 1 ] == 2.2 );
            Assert.IsTrue( t2.Doubles[ 2 ] == 3.3 );
        }
    }
}