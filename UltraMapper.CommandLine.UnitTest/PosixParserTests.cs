using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Parsing;

namespace UltraMapper.CommandLine.UnitTest
{
    [TestClass]
    [TestCategory( "POSIX parser" )]
    public class PosixParserTests
    {
        private readonly POSIXStyleCommandLineParser _posixParser =
            new POSIXStyleCommandLineParser();

        [TestMethod]
        public void CommandWithoutOptions()
        {
            var args = "list";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "list", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( SimpleParam ) );
            Assert.AreEqual( string.Empty, ((SimpleParam)res.Param).Name );
            Assert.IsNull( ((SimpleParam)res.Param).Value );
        }

        [TestMethod]
        public void CommandWithSimpleOption()
        {
            var args = "list -a";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "list", res.Name );
            Assert.AreEqual( "a", (res.Param as ComplexParam).SubParams[0].Name );
            Assert.AreEqual( "True", ((res.Param as ComplexParam).SubParams[ 0 ] as SimpleParam).Value );
        }

        [TestMethod]
        public void CommandWithComplexOption()
        {
            var args = "list --file=myfile.txt";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "list", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "file", complexParam.Name );
            Assert.AreEqual( 1, complexParam.Simple.Count );
            Assert.AreEqual( "myfile.txt", complexParam.Simple[ 0 ].Value );
        }

        [TestMethod]
        public void CommandWithArrayOption()
        {
            var args = "copy --files=file1,file2,file3";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "copy", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ArrayParam ) );
            var arrayParam = (ArrayParam)res.Param;

            Assert.AreEqual( "files", arrayParam.Name );
            Assert.AreEqual( 3, arrayParam.Simple.Count );
            Assert.AreEqual( "file1", arrayParam.Simple[ 0 ].Value );
            Assert.AreEqual( "file2", arrayParam.Simple[ 1 ].Value );
            Assert.AreEqual( "file3", arrayParam.Simple[ 2 ].Value );
        }

        [TestMethod]
        public void CommandWithMixedOptions()
        {
            var args = "run --config=config.yaml --log-level=debug -v";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "run", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "config", complexParam.Name );
            Assert.AreEqual( 2, complexParam.Simple.Count );
            Assert.AreEqual( "debug", complexParam.Simple[ 0 ].Value );
            Assert.AreEqual( "v", complexParam.Simple[ 1 ].Name );
        }

        [TestMethod]
        public void CommandWithSubParams()
        {
            var args = "build --options=opt1,opt2 --flags=flag1,flag2";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "build", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "options", complexParam.Name );
            Assert.AreEqual( 2, complexParam.Array.Count );
            Assert.AreEqual( "opt1", complexParam.Array[ 0 ].Name );
            Assert.AreEqual( "opt2", complexParam.Array[ 1 ].Name );
        }

        [TestMethod]
        public void CommandWithQuotedValue()
        {
            var args = "echo \"Hello, World!\"";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "echo", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( SimpleParam ) );
            Assert.AreEqual( "Hello, World!", ((SimpleParam)res.Param).Value );
        }

        [TestMethod]
        public void InvalidCommandShouldThrow()
        {
            var args = "-invalid";
            Assert.ThrowsException<SyntaxErrorException>( () => _posixParser.Parse( args ).ToArray() );
        }

        [TestMethod]
        public void EmptyCommandShouldThrow()
        {
            var args = string.Empty;
            Assert.ThrowsException<ArgumentNullException>( () => _posixParser.Parse( args ).ToArray() );
        }

        [TestMethod]
        public void CommandWithSingleShortOption()
        {
            var args = "ls -l";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "ls", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var simpleParam = (ComplexParam)res.Param;

            Assert.AreEqual( "l", simpleParam.SubParams[0].Name );
            Assert.AreEqual( "True", (simpleParam.SubParams[0] as SimpleParam).Value );
        }

        [TestMethod]
        public void CommandWithMultipleShortOptions()
        {
            var args = "ls -ltr";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "ls", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( 3, complexParam.SubParams.Count );
            Assert.AreEqual( "l", complexParam.SubParams[ 0 ].Name );
            Assert.AreEqual( "True", (complexParam.SubParams[0] as SimpleParam).Value );

            Assert.AreEqual( "t", complexParam.SubParams[ 1 ].Name );
            Assert.AreEqual( "True", (complexParam.SubParams[ 1 ] as SimpleParam).Value );

            Assert.AreEqual( "r", complexParam.SubParams[ 2 ].Name );
            Assert.AreEqual( "True", (complexParam.SubParams[ 2 ] as SimpleParam).Value );
        }

        [TestMethod]
        public void CommandWithShortOptionAndValue()
        {
            var args = "grep -c 5";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "grep", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "c", complexParam.Name );
            Assert.AreEqual( 1, complexParam.Simple.Count );
            Assert.AreEqual( "5", complexParam.Simple[ 0 ].Value );
        }

        [TestMethod]
        public void CommandWithLongOptionAndNoValue()
        {
            var args = "rm --recursive";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "rm", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( SimpleParam ) );
            var simpleParam = (SimpleParam)res.Param;

            Assert.AreEqual( "recursive", simpleParam.Name );
            Assert.IsNull( simpleParam.Value );
        }

        [TestMethod]
        public void CommandWithLongOptionAndValue()
        {
            var args = "cat --file=/etc/passwd";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "cat", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "file", complexParam.Name );
            Assert.AreEqual( 1, complexParam.Simple.Count );
            Assert.AreEqual( "/etc/passwd", complexParam.Simple[ 0 ].Value );
        }

        [TestMethod]
        public void CommandWithMultipleLongOptions()
        {
            var args = "cp --force --verbose";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "cp", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( 2, complexParam.Simple.Count );
            Assert.AreEqual( "force", complexParam.Simple[ 0 ].Name );
            Assert.AreEqual( "verbose", complexParam.Simple[ 1 ].Name );
        }

        [TestMethod]
        public void CommandWithOptionValueWithSpaces()
        {
            var args = "echo \"This is a test\"";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "echo", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( SimpleParam ) );
            var simpleParam = (SimpleParam)res.Param;

            Assert.AreEqual( string.Empty, simpleParam.Name );
            Assert.AreEqual( "This is a test", simpleParam.Value );
        }

        [TestMethod]
        public void CommandWithQuotedLongOptionValue()
        {
            var args = "set --value=\"A complex value with spaces\"";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "set", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "value", complexParam.Name );
            Assert.AreEqual( 1, complexParam.Simple.Count );
            Assert.AreEqual( "A complex value with spaces", complexParam.Simple[ 0 ].Value );
        }

        [TestMethod]
        public void CommandWithMultipleShortOptionsAndValues()
        {
            var args = "find -n 10 -m 5";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "find", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( 2, complexParam.Simple.Count );
            Assert.AreEqual( "n", complexParam.Simple[ 0 ].Name );
            Assert.AreEqual( "10", complexParam.Simple[ 0 ].Value );
            Assert.AreEqual( "m", complexParam.Simple[ 1 ].Name );
            Assert.AreEqual( "5", complexParam.Simple[ 1 ].Value );
        }

        [TestMethod]
        public void CommandWithEmptyStringArgument()
        {
            var args = "touch \"\"";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "touch", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( SimpleParam ) );
            var simpleParam = (SimpleParam)res.Param;

            Assert.AreEqual( string.Empty, simpleParam.Name );
            Assert.AreEqual( string.Empty, simpleParam.Value );
        }

        [TestMethod]
        public void CommandWithSpecialCharactersInValue()
        {
            var args = "echo \"Hello, @world! #hashtag $value\"";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "echo", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( SimpleParam ) );
            var simpleParam = (SimpleParam)res.Param;

            Assert.AreEqual( string.Empty, simpleParam.Name );
            Assert.AreEqual( "Hello, @world! #hashtag $value", simpleParam.Value );
        }

        [TestMethod]
        public void CommandWithOptionAndMixedQuotes()
        {
            var args = "grep --pattern='some \"quoted\" text'";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "grep", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "pattern", complexParam.Name );
            Assert.AreEqual( 1, complexParam.Simple.Count );
            Assert.AreEqual( "some \"quoted\" text", complexParam.Simple[ 0 ].Value );
        }

        [TestMethod]
        public void CommandWithEmptyOptionsAndArguments()
        {
            var args = string.Empty;
            Assert.ThrowsException<ArgumentNullException>( () => _posixParser.Parse( args ).ToArray() );
        }

        [TestMethod]
        public void CommandWithOnlyHyphen()
        {
            var args = "-";
            Assert.ThrowsException<SyntaxErrorException>( () => _posixParser.Parse( args ).ToArray() );
        }

        [TestMethod]
        public void CommandWithOnlyDoubleHyphen()
        {
            var args = "--";
            Assert.ThrowsException<SyntaxErrorException>( () => _posixParser.Parse( args ).ToArray() );
        }

        [TestMethod]
        public void CommandWithMixedShortAndLongOptionsAndValues()
        {
            var args = "tar -czvf archive.tar.gz --exclude=*.tmp";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "tar", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( 5, complexParam.SubParams.Count );

            // Short options
            Assert.AreEqual( "c", complexParam.SubParams[ 0 ].Name );
            Assert.AreEqual( "True", ((SimpleParam)complexParam.SubParams[ 0 ]).Value );

            Assert.AreEqual( "z", complexParam.SubParams[ 1 ].Name );
            Assert.AreEqual( "True", ((SimpleParam)complexParam.SubParams[ 1 ]).Value );

            Assert.AreEqual( "v", complexParam.SubParams[ 2 ].Name );
            Assert.AreEqual( "True", ((SimpleParam)complexParam.SubParams[ 2 ]).Value );

            Assert.AreEqual( "f", complexParam.SubParams[ 3 ].Name );
            Assert.AreEqual( "archive.tar.gz", ((SimpleParam)complexParam.SubParams[ 3 ]).Value );

            // Long option
            Assert.AreEqual( "exclude", complexParam.SubParams[ 4 ].Name );
            Assert.AreEqual( "*.tmp", ((SimpleParam)complexParam.SubParams[ 4 ]).Value );
        }

        [TestMethod]
        public void CommandWithDeeplyNestedSubParams()
        {
            var args = "cmd --group={--inner1=value1 --inner2={--deep1 --deep2=value2}}";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "cmd", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "group", complexParam.Name );
            Assert.AreEqual( 1, complexParam.Complex.Count );

            var innerComplex = complexParam.Complex[ 0 ];
            Assert.AreEqual( "inner1", innerComplex.Name );
            Assert.AreEqual( 1, innerComplex.Simple.Count );
            Assert.AreEqual( "value1", innerComplex.Simple[ 0 ].Value );

            var innerComplex2 = complexParam.Complex[ 1 ];
            Assert.AreEqual( "inner2", innerComplex2.Name );
            Assert.AreEqual( 2, innerComplex2.Complex.Count );
            Assert.AreEqual( "deep1", innerComplex2.Complex[ 0 ].Name );
            Assert.AreEqual( "deep2", innerComplex2.Complex[ 1 ].Name );
            Assert.AreEqual( "value2", innerComplex2.Complex[ 1 ].Simple[ 0 ].Value );
        }

        [TestMethod]
        public void CommandWithArrayOptionsAndValues()
        {
            var args = "program --options=[opt1,opt2,opt3]";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "program", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "options", complexParam.Name );
            Assert.AreEqual( 1, complexParam.Array.Count );

            var arrayParam = complexParam.Array[ 0 ];
            Assert.AreEqual( 3, arrayParam.Simple.Count );
            Assert.AreEqual( "opt1", arrayParam.Simple[ 0 ].Value );
            Assert.AreEqual( "opt2", arrayParam.Simple[ 1 ].Value );
            Assert.AreEqual( "opt3", arrayParam.Simple[ 2 ].Value );
        }

        [TestMethod]
        public void CommandWithNestedArrayParams()
        {
            var args = "tool --data=[{name=John,value=123},{name=Jane,value=456}]";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "tool", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "data", complexParam.Name );
            Assert.AreEqual( 1, complexParam.Array.Count );

            var arrayParam = complexParam.Array[ 0 ];
            Assert.AreEqual( 2, arrayParam.Complex.Count );

            var firstItem = arrayParam.Complex[ 0 ];
            Assert.AreEqual( "name", firstItem.Name );
            Assert.AreEqual( "John", firstItem.Simple[ 0 ].Value );
            Assert.AreEqual( "value", firstItem.Complex[ 0 ].Name );
            Assert.AreEqual( "123", firstItem.Complex[ 0 ].Simple[ 0 ].Value );

            var secondItem = arrayParam.Complex[ 1 ];
            Assert.AreEqual( "name", secondItem.Name );
            Assert.AreEqual( "Jane", secondItem.Simple[ 0 ].Value );
            Assert.AreEqual( "value", secondItem.Complex[ 0 ].Name );
            Assert.AreEqual( "456", secondItem.Complex[ 0 ].Simple[ 0 ].Value );
        }

        [TestMethod]
        public void CommandWithUnclosedQuotes()
        {
            var args = "echo \"This string is not closed";
            Assert.ThrowsException<SyntaxErrorException>( () => _posixParser.Parse( args ).ToArray() );
        }

        [TestMethod]
        public void CommandWithUnmatchedBraces()
        {
            var args = "cmd --options={value1,value2";
            Assert.ThrowsException<SyntaxErrorException>( () => _posixParser.Parse( args ).ToArray() );
        }

        [TestMethod]
        public void CommandWithOptionAppearingMultipleTimes()
        {
            var args = "build --target=debug --target=release";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "build", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "target", complexParam.Name );
            Assert.AreEqual( 2, complexParam.Simple.Count );
            Assert.AreEqual( "debug", complexParam.Simple[ 0 ].Value );
            Assert.AreEqual( "release", complexParam.Simple[ 1 ].Value );
        }

        [TestMethod]
        public void CommandWithEmptyArrayParam()
        {
            var args = "app --list=[]";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "app", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "list", complexParam.Name );
            Assert.AreEqual( 1, complexParam.Array.Count );

            var arrayParam = complexParam.Array[ 0 ];
            Assert.AreEqual( 0, arrayParam.Simple.Count );
        }

        [TestMethod]
        public void CommandWithEscapedQuotesInValues()
        {
            var args = "print --message=\"Hello \\\"world\\\"\"";
            var res = _posixParser.Parse( args ).First();

            Assert.AreEqual( "print", res.Name );
            Assert.IsInstanceOfType( res.Param, typeof( ComplexParam ) );
            var complexParam = (ComplexParam)res.Param;

            Assert.AreEqual( "message", complexParam.Name );
            Assert.AreEqual( 1, complexParam.Simple.Count );
            Assert.AreEqual( "Hello \"world\"", complexParam.Simple[ 0 ].Value );
        }
    }
}
