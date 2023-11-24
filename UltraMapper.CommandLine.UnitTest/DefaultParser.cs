using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Parsing;

namespace UltraMapper.CommandLine.UnitTest
{
    [TestClass]
    public class DefaultParserTests
    {
        private readonly DefaultParser _textParser = new DefaultParser();

        [TestMethod]
        public void WrongCommandSyntax()
        {
            var args = "open true";
            ShouldThrow<SyntaxErrorException>( args );
        }

        [TestMethod]
        public void WrongCommandSyntax2()
        {
            var args = "open true --move a b";
            ShouldThrow<SyntaxErrorException>( args );
        }

        [TestMethod]
        public void CommandNull()
        {
            string args = null;
            ShouldThrow<ArgumentNullException>( args );
        }

        [TestMethod]
        public void CommandWhiteSpaces()
        {
            //spaces, tabs
            string args = "                             ";
            ShouldThrow<ArgumentNullException>( args );
        }

        [TestMethod]
        public void OpenPath1()
        {
            //il : del percorso è in conflitto con la sintassi dei nomi dei parametri

            string args = @"--open C:\";
            var res = _textParser.Parse( args );

            Assert.IsTrue( (res.First().Param as SimpleParam).Value == @"C:\" );
        }

        [TestMethod]
        public void OpenPath2()
        {
            string args = "--open \"C:\\\\\"";
            var res = _textParser.Parse( args );

            Assert.IsTrue( (res.First().Param as SimpleParam).Value == @"C:\\" );
        }

        [TestMethod]
        public void OpenPath3()
        {
            string args = "--open \"C:\\\\dir\\dir with spaces\\\"";
            var res = _textParser.Parse( args );

            Assert.IsTrue( (res.First().Param as SimpleParam).Value == @"C:\\dir\dir with spaces\" );
        }

        [TestMethod]
        public void OpenPath12()
        {
            //il : del percorso è in conflitto con la sintassi dei nomi dei parametri

            string args = @"--open ""C:\""";
            var res = _textParser.Parse( args );

            Assert.IsTrue( (res.First().Param as SimpleParam).Value == @"C:\" );
        }

        [TestMethod]
        public void OpenPath4()
        {
            string args = "--open \"\\\"ciao\\suka\\\"\"";
            var res = _textParser.Parse( args );

            Assert.IsTrue( (res.First().Param as SimpleParam).Value == "\"\\\"ciao\\suka\\\"\"" );
        }

        [TestMethod]
        public void AnythingQuoted()
        {
            string args = @"--command ""[([\""])]""";
            var res = _textParser.Parse( args );

            Assert.IsTrue( (res.First().Param as SimpleParam).Value == @"[([\""])]" );
        }

        [TestMethod]
        public void NamedArrayItemNameIgnored()
        {
            string args = @"--command [ a=1 b=2 ]";
            var res = _textParser.Parse( args );

            Assert.IsTrue( (res.First().Param as ArrayParam).Simple.First().Name == @"a" ); //array item name never used
            Assert.IsTrue( (res.First().Param as ArrayParam).Simple.First().Value == @"1" ); //array item name never used
        }

        [TestMethod]
        public void AQuote()
        {
            string args = @"--command ""\""""";
            var res = _textParser.Parse( args );

            Assert.IsTrue( (res.First().Param as SimpleParam).Value == @"\""" );
        }

        [TestMethod]
        public void NamedNested()
        {
            string args = @"--command sublevel2.sublevel3.g=g";
            var res = _textParser.Parse( args );

            Assert.IsTrue( res.First().Param.Name == "sublevel2.sublevel3.g" );
            Assert.IsTrue( (res.First().Param as SimpleParam).Value == "g" );
        }

        [TestMethod]
        public void NamedComplex()
        {
            string args = @"--command prop=(a b)";
            var res = _textParser.Parse( args );

            Assert.IsTrue( res.First().Param.Name == "prop" );
            Assert.IsTrue( ((res.First().Param as ComplexParam)[ 0 ] as SimpleParam).Value == "a" );
            Assert.IsTrue( ((res.First().Param as ComplexParam)[ 1 ] as SimpleParam).Value == "b" );
        }

        [TestMethod]
        public void NamedArray()
        {
            string args = @"--command prop=[a b]";
            var res = _textParser.Parse( args );

            Assert.IsTrue( res.First().Param.Name == "prop" );
            Assert.IsTrue( (res.First().Param as ArrayParam).Simple.ElementAt( 0 ).Value == "a");
            Assert.IsTrue( (res.First().Param as ArrayParam).Simple.ElementAt( 1 ).Value == "b");
        }

        [TestMethod]
        public void CommandWithParamNameValueDelimiter()
        {
            string args = @"--open ""this=path""";
            var res = _textParser.Parse( args );
            var param = res.First().Param as SimpleParam;

            Assert.IsTrue( param.Name == "" );
            Assert.IsTrue( param.Value == "this=path" );
        }

        [TestMethod]
        public void CommandWithParamNameValueDelimiter2()
        {
            string args = @"--open this=""this=path""";
            var res = _textParser.Parse( args );
            var param = res.First().Param as SimpleParam;

            Assert.IsTrue( param.Name == "this" );
            Assert.IsTrue( param.Value == "this=path" );
        }

        [TestMethod]
        public void CollectionParse()
        {
            var args = "--numbers [1 2 3 4 5]";
            var res = _textParser.Parse( args );

            var array = res.First().Param as ArrayParam;
            var arrayValues = array.Simple.Select( v => ((SimpleParam)v).Value );

            Assert.IsTrue( arrayValues.SequenceEqual( new[] { "1", "2", "3", "4", "5" } ) );
        }

        [TestMethod]
        public void CollectionParseQuoted()
        {
            var args = @"--numbers [""1"" ""2"" ""3"" ""4"" ""5""]";
            var res = _textParser.Parse( args );

            var array = res.First().Param as ArrayParam;
            var arrayValues = array.Simple.Select( v => ((SimpleParam)v).Value );

            Assert.IsTrue( arrayValues.SequenceEqual( new[] { "1", "2", "3", "4", "5" } ) );
        }

        [TestMethod]
        public void MultipleQuotedSpaces()
        {
            var args = @"--param ""param   containing   multiple  spaces""";
            var res = _textParser.Parse( args );
            var param = res.First().Param as SimpleParam;

            Assert.IsTrue( param.Value == "param   containing   multiple  spaces" );
        }

        [TestMethod]
        public void MultipleArraysMethod()
        {
            var args = @"--print a [ b c] [d e ]";
            var command = _textParser.Parse( args );

            var complexParam = command.First().Param as ComplexParam;

            var simple0 = complexParam.SubParams[ 0 ] as SimpleParam;
            var array1 = complexParam.SubParams[ 1 ] as ArrayParam;
            var array2 = complexParam.SubParams[ 2 ] as ArrayParam;

            Assert.IsTrue( simple0.Value == "a" );
            Assert.IsTrue( array1.Simple[ 0 ].Value == "b" );
            Assert.IsTrue( array1.Simple[ 1 ].Value == "c" );
            Assert.IsTrue( array2.Simple[ 0 ].Value == "d" );
            Assert.IsTrue( array2.Simple[ 1 ].Value == "e" );
        }

        [TestMethod]
        public void ComplexObject()
        {
            var args = @"--fake (a (b [ (1 2) (3 4) (5 6) ] d)) ( x y ) z";
            var command = _textParser.Parse( args );

            var complexParam = command.First().Param as ComplexParam;
            var complexParam1 = complexParam.Complex.ElementAt( 0 );
            var complexParam2 = complexParam.Complex.ElementAt( 1 );
            var param3 = complexParam.SubParams[ 2 ] as SimpleParam;

            Assert.IsTrue( complexParam1.Simple.ElementAt( 0 ).Value == "a" );
            Assert.IsTrue( complexParam1.Complex.ElementAt( 0 ).Simple.ElementAt( 0 ).Value == "b" );
            Assert.IsTrue( complexParam1.Complex.ElementAt( 0 ).Array.ElementAt( 0 ).Complex.ElementAt( 0 ).Simple.ElementAt( 0 ).Value == "1" );
            Assert.IsTrue( complexParam1.Complex.ElementAt( 0 ).Array.ElementAt( 0 ).Complex.ElementAt( 0 ).Simple.ElementAt( 1 ).Value == "2" );
            Assert.IsTrue( complexParam1.Complex.ElementAt( 0 ).Array.ElementAt( 0 ).Complex.ElementAt( 1 ).Simple.ElementAt( 0 ).Value == "3" );
            Assert.IsTrue( complexParam1.Complex.ElementAt( 0 ).Array.ElementAt( 0 ).Complex.ElementAt( 1 ).Simple.ElementAt( 1 ).Value == "4" );
            Assert.IsTrue( complexParam1.Complex.ElementAt( 0 ).Array.ElementAt( 0 ).Complex.ElementAt( 2 ).Simple.ElementAt( 0 ).Value == "5" );
            Assert.IsTrue( complexParam1.Complex.ElementAt( 0 ).Array.ElementAt( 0 ).Complex.ElementAt( 2 ).Simple.ElementAt( 1 ).Value == "6" );
            Assert.IsTrue( ((complexParam1[ 1 ] as ComplexParam)[ 2 ] as SimpleParam).Value == "d" );

            Assert.IsTrue( (complexParam2[ 0 ] as SimpleParam).Value == "x" );
            Assert.IsTrue( (complexParam2[ 1 ] as SimpleParam).Value == "y" );

            Assert.IsTrue( param3.Value == "z" );
        }

        [TestMethod]
        public void CommandIdentifierInsideParameter()
        {
            //need quotes on last '--z' param (otherwise interpreted as command)

            var args = @"--command (--a (--b [ (--1 --2) (--3 --4) (--5 --6) ] --d)) ( --x --y ) ""--z""";
            var command = _textParser.Parse( args ).ToList();

            Assert.IsTrue( command.Single().Name == "command" );
            Assert.IsTrue( ((command.Single().Param as ComplexParam)[ 2 ] as SimpleParam).Value == "--z" );

            //args = @"--command (--a (--b [ (--1 --2) (--3 --4) (--5 --6) ] --d)) ( --x --y ) ""\""--z\""""";
            //command = _textParser.Parse( args ).ToList();

            //Assert.IsTrue( command.Single().Name == "command" );
            //Assert.IsTrue( ((command.Single().Param as ComplexParam)[ 2 ] as SimpleParam).Value == @"""\""--z\""""" );

            args = @"--command (--a (--b [ (--1 --2) (--3 --4) (--5 --6) ] --d)) ( --x --y ) --z";
            command = _textParser.Parse( args ).ToList();

            Assert.IsTrue( command.Count == 2 );
            Assert.IsTrue( command.First().Name == "command" );
            Assert.IsTrue( command.Last().Name == "z" );
        }

        private void ShouldThrow<T>( string args ) where T : Exception
        {
            //toarray needed cause the method is lazy
            Assert.ThrowsException<T>( () => _textParser.Parse( args ).ToArray() );
        }
    }
}
