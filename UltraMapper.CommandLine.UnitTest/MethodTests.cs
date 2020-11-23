using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandLine.AutoParser.UnitTest
{
    [TestClass]
    [TestCategory( "Methods" )]
    public class MethodZeroParamsTests
    {
        private class Commands
        {
            [Option( IsIgnored = true )]
            public bool IsExecuted { get; protected set; } = false;

            public void ParameterlessMethod()
            {
                this.IsExecuted = true;
            }
        }

        [TestMethod]
        public void ParameterlessMethod()
        {
            var args = $"--{nameof( Commands.ParameterlessMethod )}";
            var parsed = AutoParser.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.IsExecuted );
        }

        [TestMethod]
        public void TooManyParams()
        {
            var args = $"--{nameof( Commands.ParameterlessMethod )} thisparamnotneeded";
            Assert.ThrowsException<ArgumentException>(
                () => AutoParser.Instance.Parse<Commands>( args ) );
        }
    }

    [TestClass]
    [TestCategory( "Methods" )]
    public class MethodOneParamTests
    {
        private class Commands1
        {
            [Option( IsIgnored = true )] public bool IsExecuted { get; protected set; } = false;
            [Option( IsIgnored = true )] public string IsOpenPathParam { get; private set; } = String.Empty;

            public void Open( string path )
            {
                this.IsExecuted = true;
                this.IsOpenPathParam = path;
            }
        }

        public class Commands2
        {
            [Option( IsIgnored = true )] public bool IsExecuted { get; protected set; } = false;
            [Option( IsIgnored = true )] public bool? IsCallBoolParam { get; private set; } = null;

            public void CallBool( bool param )
            {
                this.IsExecuted = true;
                this.IsCallBoolParam = param;
            }
        }

        [TestMethod]
        public void StandardCall()
        {
            var args = $"--{nameof( Commands1.Open )} thispath";
            var parsed = AutoParser.Instance.Parse<Commands1>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.IsOpenPathParam == "thispath" );
        }

        [TestMethod]
        public void StandardCallNamedParam()
        {
            var args = $"--{nameof( Commands1.Open )} path=thispath";
            var parsed = AutoParser.Instance.Parse<Commands1>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.IsOpenPathParam == "thispath" );
        }

        [TestMethod]
        public void MissingParams()
        {
            var args = $"--{nameof( Commands1.Open )}";
            Assert.ThrowsException<ArgumentException>(
                () => AutoParser.Instance.Parse<Commands1>( args ) );
        }

        [TestMethod]
        public void TooManyParams()
        {
            var args = $"--{nameof( Commands1.Open )} thispath notneededparam";
            Assert.ThrowsException<ArgumentException>(
                () => AutoParser.Instance.Parse<Commands1>( args ) );
        }

        [TestMethod]
        public void BoolParamExplicitSetFalse()
        {
            var args = $"--{nameof( Commands2.CallBool )} false";
            var parsed = AutoParser.Instance.Parse<Commands2>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.IsCallBoolParam == false );
        }

        [TestMethod]
        public void BoolParamExplicitSetTrue()
        {
            var args = $"--{nameof( Commands2.CallBool )} true";
            var parsed = AutoParser.Instance.Parse<Commands2>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.IsCallBoolParam == true );
        }

        [TestMethod]
        public void BoolParamImplicitSetTrue()
        {
            var args = $"--{nameof( Commands2.CallBool )}";
            var parsed = AutoParser.Instance.Parse<Commands2>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.IsCallBoolParam == true );
        }
    }

    [TestClass]
    [TestCategory( "Methods" )]
    public class MethodTwoParamsTests
    {
        private class Commands1
        {
            [Option( IsIgnored = true )] public bool IsExecuted { get; protected set; } = false;
            [Option( IsIgnored = true )] public string FromParam { get; private set; } = String.Empty;
            [Option( IsIgnored = true )] public string ToParam { get; private set; } = String.Empty;

            public void Move( string from, string to )
            {
                this.IsExecuted = true;
                this.FromParam = from;
                this.ToParam = to;
            }
        }

        public class Commands2
        {
            [Option( IsIgnored = true )] public bool IsExecuted { get; protected set; } = false;
            [Option( IsIgnored = true )] public string FromParam { get; private set; } = String.Empty;
            [Option( IsIgnored = true )] public string ToParam { get; private set; } = String.Empty;

            public void MoveParamNameOverride(
                [Option( Name = "f" )] string from,
                [Option( Name = "t" )] string to )
            {
                this.IsExecuted = true;
                this.FromParam = from;
                this.ToParam = to;
            }
        }

        public class Commands3
        {
            public bool IsExecuted { get; protected set; } = false;
            public string FromParam { get; private set; } = String.Empty;
            public string ToParam { get; private set; } = String.Empty;

            public void MoveParamOrderOverride(
                [Option( Order = 1 )] string from,
                [Option( Order = 0 )] string to )
            {
                this.IsExecuted = true;
                this.FromParam = from;
                this.ToParam = to;
            }
        }

        [TestMethod]
        public void StandardCall()
        {
            var args = "--move fromhere tohere";
            var parsed = AutoParser.Instance.Parse<Commands1>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.FromParam == "fromhere" );
            Assert.IsTrue( parsed.ToParam == "tohere" );
        }

        [TestMethod]
        public void StandardCallNamedParam()
        {
            var args = "--move from=fromhere to=tohere";
            var parsed = AutoParser.Instance.Parse<Commands1>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.FromParam == "fromhere" );
            Assert.IsTrue( parsed.ToParam == "tohere" );
        }

        [TestMethod]
        public void StandardCallNamedParamRandomParamsOrder()
        {
            var args = "--move to=tohere from=fromhere";
            var parsed = AutoParser.Instance.Parse<Commands1>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.FromParam == "fromhere" );
            Assert.IsTrue( parsed.ToParam == "tohere" );
        }

        [TestMethod]
        public void MixedNamedNonNamedParamsWrongOrder()
        {
            var args = "--move to=tohere fromhere";
            Assert.ThrowsException<ArgumentException>(
                () => AutoParser.Instance.Parse<Commands1>( args ) );
        }

        [TestMethod]
        public void MissingParams()
        {
            var args = "--move";
            Assert.ThrowsException<ArgumentException>(
                () => AutoParser.Instance.Parse<Commands1>( args ) );

            args = "--move to=tohere";
            Assert.ThrowsException<ArgumentException>(
                   () => AutoParser.Instance.Parse<Commands1>( args ) );

            args = "--move from=fromhere";
            Assert.ThrowsException<ArgumentException>(
                   () => AutoParser.Instance.Parse<Commands1>( args ) );
        }

        [TestMethod]
        public void TooManyParams1()
        {
            var args = "--move fromhere tohere notneeded";
            Assert.ThrowsException<ArgumentException>(
                () => AutoParser.Instance.Parse<Commands1>( args ) );
        }

        [TestMethod]
        public void TooManyParams2()
        {
            var args = "--move to=tohere from=fromhere p3=notneeded";
            Assert.ThrowsException<ArgumentException>(
                () => AutoParser.Instance.Parse<Commands1>( args ) );
        }

        [TestMethod]
        public void TooManyParams3()
        {
            var args = "--move fromhere tohere p3=notneeded";
            Assert.ThrowsException<ArgumentException>(
                () => AutoParser.Instance.Parse<Commands1>( args ) );
        }

        [TestMethod]
        public void TooManyParams4()
        {
            var args = "--move fromhere to=tohere p3=notneeded";
            Assert.ThrowsException<ArgumentException>(
                () => AutoParser.Instance.Parse<Commands1>( args ) );
        }

        [TestMethod]
        public void MethodParamNameOverride()
        {
            var args = $"--{nameof( Commands2.MoveParamNameOverride )} t=tohere f=fromhere";
            var parsed = AutoParser.Instance.Parse<Commands2>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.FromParam == "fromhere" );
            Assert.IsTrue( parsed.ToParam == "tohere" );
        }

        [TestMethod]
        public void MethodParamNameOverride2()
        {
            var args = $"--{nameof( Commands2.MoveParamNameOverride )} f=fromhere t=tohere";
            var parsed = AutoParser.Instance.Parse<Commands2>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.FromParam == "fromhere" );
            Assert.IsTrue( parsed.ToParam == "tohere" );
        }

        [TestMethod]
        public void MethodParamOrderOverride()
        {
            var args = $"--{nameof( Commands3.MoveParamOrderOverride )} tohere fromhere";
            var parsed = AutoParser.Instance.Parse<Commands3>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.FromParam == "fromhere" );
            Assert.IsTrue( parsed.ToParam == "tohere" );
        }
    }

    [TestClass]
    [TestCategory( "Methods" )]
    public class MultipleHeterogeneousParamsMethods
    {
        private class ComplexNumber
        {
            public int A { get; set; }
            public int B { get; set; }
        }

        private class ComplexLine
        {
            public string A { get; set; }
            public string B { get; set; }
        }

        private class Commands5
        {
            [Option( IsIgnored = true )] public bool IsExecuted { get; protected set; } = false;
            [Option( IsIgnored = true )] public string[] PrintLines { get; private set; }
            [Option( IsIgnored = true )] public int[] PrintNumbers { get; private set; }

            public void PrintSimpleParams( string line, int number )
            {
                this.IsExecuted = true;
                this.PrintLines = new[] { line };
                this.PrintNumbers = new[] { number };
            }

            public void PrintComplexLineAndNumber( ComplexLine line, int number )
            {
                this.IsExecuted = true;
                this.PrintLines = new[] { line.A, line.B };
                this.PrintNumbers = new[] { number };
            }

            public void PrintLineAndComplexNumber( string line, ComplexNumber number )
            {
                this.IsExecuted = true;
                this.PrintLines = new[] { line };
                this.PrintNumbers = new[] { number.A, number.B };
            }

            public void PrintCollectionAndNumber( string[] lines, int number )
            {
                this.IsExecuted = true;
                this.PrintLines = lines;
                this.PrintNumbers = new[] { number };
            }

            public void PrintNumberAndCollection( int number, string[] lines )
            {
                this.IsExecuted = true;
                this.PrintLines = lines;
                this.PrintNumbers = new[] { number };
            }

            public void PrintComplexLineAndArrayOfNumbers( ComplexLine lines, int[] numbers )
            {
                this.IsExecuted = true;
                this.PrintLines = new[] { lines.A, lines.B };
                this.PrintNumbers = numbers;
            }

            public void PrintComplexNumberAndArrayOfLines( ComplexNumber numbers, string[] lines )
            {
                this.IsExecuted = true;
                this.PrintLines = lines;
                this.PrintNumbers = new[] { numbers.A, numbers.B };
            }

            public void PrintCollections( string[] lines, IEnumerable<int> numbers )
            {
                this.IsExecuted = true;
                this.PrintLines = lines;
                this.PrintNumbers = numbers.ToArray();
            }
        }

        [TestMethod]
        public void Method2SimpleParams()
        {
            var args = $"--{nameof( Commands5.PrintSimpleParams )} a 1";
            var parsed = AutoParser.Instance.Parse<Commands5>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.PrintLines.SequenceEqual( new[] { "a" } ) );
            Assert.IsTrue( parsed.PrintNumbers.SequenceEqual( new[] { 1 } ) );
        }

        [TestMethod]
        public void Method2ParamsComplexLineAndNumber()
        {
            var args = $"--{nameof( Commands5.PrintComplexLineAndNumber )} (b c) 1";
            var parsed = AutoParser.Instance.Parse<Commands5>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.PrintLines.SequenceEqual( new[] { "b", "c" } ) );
            Assert.IsTrue( parsed.PrintNumbers.SequenceEqual( new[] { 1 } ) );
        }

        [TestMethod]
        public void Method2ParamsLineAndComplexNumber()
        {
            var args = $"--{nameof( Commands5.PrintLineAndComplexNumber )} b (0 1)";
            var parsed = AutoParser.Instance.Parse<Commands5>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.PrintLines.SequenceEqual( new[] { "b" } ) );
            Assert.IsTrue( parsed.PrintNumbers.SequenceEqual( new[] { 0, 1 } ) );
        }

        [TestMethod]
        public void Method2ParamsCollectionAndNumber()
        {
            var args = $"--{nameof( Commands5.PrintCollectionAndNumber )} [b c] 1";
            var parsed = AutoParser.Instance.Parse<Commands5>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.PrintLines.SequenceEqual( new[] { "b", "c" } ) );
            Assert.IsTrue( parsed.PrintNumbers.SequenceEqual( new[] { 1 } ) );
        }

        [TestMethod]
        public void Method2ParamsNumberAndCollection()
        {
            var args = $"--{nameof( Commands5.PrintNumberAndCollection )} 1 [b c]";
            var parsed = AutoParser.Instance.Parse<Commands5>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.PrintLines.SequenceEqual( new[] { "b", "c" } ) );
            Assert.IsTrue( parsed.PrintNumbers.SequenceEqual( new[] { 1 } ) );
        }

        [TestMethod]
        public void PrintComplexLineAndArrayOfNumbers()
        {
            var args = $"--{nameof( Commands5.PrintComplexLineAndArrayOfNumbers )} (b c) [0 1]";
            var parsed = AutoParser.Instance.Parse<Commands5>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.PrintLines.SequenceEqual( new[] { "b", "c" } ) );
            Assert.IsTrue( parsed.PrintNumbers.SequenceEqual( new[] { 0, 1 } ) );
        }

        [TestMethod]
        public void PrintComplexNumberAndArrayOfLines()
        {
            var args = $"--{nameof( Commands5.PrintComplexNumberAndArrayOfLines )} (0 1) [b c]";
            var parsed = AutoParser.Instance.Parse<Commands5>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.PrintLines.SequenceEqual( new[] { "b", "c" } ) );
            Assert.IsTrue( parsed.PrintNumbers.SequenceEqual( new[] { 0, 1 } ) );
        }

        [TestMethod]
        public void Method2ParamsCollections()
        {
            var args = $"--{nameof( Commands5.PrintCollections )} [b c] [0 1]";
            var parsed = AutoParser.Instance.Parse<Commands5>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.PrintLines.SequenceEqual( new[] { "b", "c" } ) );
            Assert.IsTrue( parsed.PrintNumbers.SequenceEqual( new[] { 0, 1 } ) );
        }
    }
}
