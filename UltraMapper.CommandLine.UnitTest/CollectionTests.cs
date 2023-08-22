using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace UltraMapper.CommandLine.UnitTest
{
    [TestClass]
    [TestCategory( "Collections" )]
    public class CollectionTests
    {
        private class Complex
        {
            public int A { get; set; }
            public int B { get; set; }

            public Complex() { }

            public Complex( int a, int b )
            {
                this.A = a;
                this.B = b;
            }

            public override int GetHashCode()
            {
                return this.A ^ this.B;
            }

            public override bool Equals( object obj )
            {
                if( obj is Complex other )
                    return this.A == other.A && this.B == other.B;

                return false;
            }
        }

        private class CommandsArray
        {
            public string[] ArrayNoConversionProperty { get; set; }
            public int[] ArrayConversionProperty { get; set; }
            public Complex[] ArrayComplexTypeProperty { get; set; }
        }

        private class CommandsList
        {
            public List<string> ListNoConversionProperty { get; set; }
            public List<int> ListConversionProperty { get; set; }
            public List<Complex> ListComplexProperty { get; set; }
        }

        private class CommandsEnumerable
        {
            public IEnumerable<string> EnumerableNoConversionProperty { get; set; }
            public IEnumerable<int> EnumerableConversionProperty { get; set; }
            public IEnumerable<Complex> EnumerableComplexTypeProperty { get; set; }
        }

        [TestMethod]
        public void ArrayNoConversionProperty()
        {
            var args = $"--{nameof( CommandsArray.ArrayNoConversionProperty )} [1 2 3 4 5]";
            var parsed = CommandLine.Instance.Parse<CommandsArray>( args );

            Assert.IsTrue( parsed.ArrayNoConversionProperty
                .SequenceEqual( new[] { "1", "2", "3", "4", "5" } ) );
        }

        [TestMethod]
        public void ArrayConversionProperty()
        {
            var args = $"--{nameof( CommandsArray.ArrayConversionProperty )} [1 2 3 4 5]";
            var parsed = CommandLine.Instance.Parse<CommandsArray>( args );

            Assert.IsTrue( parsed.ArrayConversionProperty
                .SequenceEqual( new[] { 1, 2, 3, 4, 5 } ) );
        }

        [TestMethod]
        public void ListComplexProperty()
        {
            var args = $"--{nameof( CommandsList.ListComplexProperty )} [(1 2) (3 4) (5 6)]";
            var parsed = CommandLine.Instance.Parse<CommandsList>( args );

            Assert.IsTrue( parsed.ListComplexProperty
                .SequenceEqual( new[] { new Complex( 1, 2 ), new Complex( 3, 4 ), new Complex( 5, 6 ) } ) );
        }

        [TestMethod]
        public void ListNoConversionProperty()
        {
            var args = $"--{nameof( CommandsList.ListNoConversionProperty )} [1 2 3 4 5]";
            var parsed = CommandLine.Instance.Parse<CommandsList>( args );

            Assert.IsTrue( parsed.ListNoConversionProperty
                .SequenceEqual( new[] { "1", "2", "3", "4", "5" } ) );
        }

        [TestMethod]
        public void ListConversionProperty()
        {
            var args = $"--{nameof( CommandsList.ListConversionProperty )} [1 2 3 4 5]";
            var parsed = CommandLine.Instance.Parse<CommandsList>( args );

            Assert.IsTrue( parsed.ListConversionProperty
                .SequenceEqual( new[] { 1, 2, 3, 4, 5 } ) );
        }

        [TestMethod]
        public void EnumerableNoConversionProperty()
        {
            var args = $"--{nameof( CommandsEnumerable.EnumerableNoConversionProperty )} [1 2 3 4 5]";
            var parsed = CommandLine.Instance.Parse<CommandsEnumerable>( args );

            Assert.IsTrue( parsed.EnumerableNoConversionProperty
                .SequenceEqual( new[] { "1", "2", "3", "4", "5" } ) );
        }

        [TestMethod]
        public void EnumerableConversionProperty()
        {
            var args = $"--{nameof( CommandsEnumerable.EnumerableConversionProperty )} [1 2 3 4 5]";
            var parsed = CommandLine.Instance.Parse<CommandsEnumerable>( args );

            Assert.IsTrue( parsed.EnumerableConversionProperty
                .SequenceEqual( new[] { 1, 2, 3, 4, 5 } ) );
        }

        [TestMethod]
        public void ArrayComplex()
        {
            var args = $"--{nameof( CommandsArray.ArrayComplexTypeProperty )} [(1 2) (3 4) (5 6)]";
            var parsed = CommandLine.Instance.Parse<CommandsArray>( args );

            Assert.IsTrue( parsed.ArrayComplexTypeProperty
                .SequenceEqual( new[] { new Complex( 1, 2 ), new Complex( 3, 4 ), new Complex( 5, 6 ) } ) );
        }

        [TestMethod]
        public void EnumerableComplexProperty()
        {
            var args = $"--{nameof( CommandsEnumerable.EnumerableComplexTypeProperty )} [(1 2) (3 4) (5 6)]";            
            var parsed = CommandLine.Instance.Parse<CommandsEnumerable>( args );

            Assert.IsTrue( parsed.EnumerableComplexTypeProperty
                .SequenceEqual( new[] { new Complex( 1, 2 ), new Complex( 3, 4 ), new Complex( 5, 6 ) } ) );
        }
    }

    [TestClass]
    [TestCategory( "Methods" )]
    [TestCategory( "Collections" )]
    public class CollectionTestMethods
    {
        public class Complex
        {
            public int A { get; set; }
            public int B { get; set; }

            public Complex() { }

            public Complex( int a, int b )
            {
                this.A = a;
                this.B = b;
            }

            public override int GetHashCode()
            {
                return this.A ^ this.B;
            }

            public override bool Equals( object obj )
            {
                if( obj is Complex other )
                    return this.A == other.A && this.B == other.B;

                return false;
            }
        }

        private class CommandsArray
        {
            [Option( IsIgnored = true )] public string[] ArrayNoConversionProperty { get; set; }
            [Option( IsIgnored = true )] public int[] ArrayConversionProperty { get; set; }
            [Option( IsIgnored = true )] public Complex[] ArrayComplexTypeProperty { get; set; }

            public void ArrayNoConversionMethod( string[] strings )
            {
                this.ArrayNoConversionProperty = strings;
            }

            public void ArrayConversionMethod( int[] ints )
            {
                this.ArrayConversionProperty = ints;
            }

            public void ArrayComplexMethod( Complex[] inners )
            {
                this.ArrayComplexTypeProperty = inners;
            }
        }

        private class CommandsList
        {
            [Option( IsIgnored = true )] public List<string> ListNoConversionProperty { get; set; }
            [Option( IsIgnored = true )] public List<int> ListConversionProperty { get; set; }
            [Option( IsIgnored = true )] public List<Complex> ListComplexProperty { get; set; }

            public void ListNoConversionMethod( List<string> strings )
            {
                this.ListNoConversionProperty = strings;
            }

            public void ListConversionMethod( List<int> ints )
            {
                this.ListConversionProperty = ints;
            }

            public void ListComplexMethod( List<Complex> complex )
            {
                this.ListComplexProperty = complex;
            }
        }

        private class CommandsEnumerable
        {
            [Option( IsIgnored = true )] public IEnumerable<string> EnumerableNoConversionProperty { get; set; }
            [Option( IsIgnored = true )] public IEnumerable<int> EnumerableConversionProperty { get; set; }
            [Option( IsIgnored = true )] public IEnumerable<Complex> EnumerableComplexTypeProperty { get; set; }

            public void EnumerableNoConversionMethod( IEnumerable<string> strings )
            {
                this.EnumerableNoConversionProperty = strings;
            }

            public void EnumerableConversionMethod( IEnumerable<int> ints )
            {
                this.EnumerableConversionProperty = ints;
            }

            public void EnumerableComplexMethod( IEnumerable<Complex> complex )
            {
                this.EnumerableComplexTypeProperty = complex;
            }
        }

        [TestMethod]
        public void ListNoConversionMethod()
        {
            var args = $"--{nameof( CommandsList.ListNoConversionMethod )} [1 2 3 4 5]";
            var parsed = CommandLine.Instance.Parse<CommandsList>( args );

            Assert.IsTrue( parsed.ListNoConversionProperty
                .SequenceEqual( new[] { "1", "2", "3", "4", "5" } ) );
        }

        [TestMethod]
        public void ListConversionMethod()
        {
            var args = $"--{nameof( CommandsList.ListConversionMethod )} [1 2 3 4 5]";
            var parsed = CommandLine.Instance.Parse<CommandsList>( args );

            Assert.IsTrue( parsed.ListConversionProperty
                .SequenceEqual( new[] { 1, 2, 3, 4, 5 } ) );
        }

        [TestMethod]
        public void ListComplexMethod()
        {
            var args = $"--{nameof( CommandsList.ListComplexMethod )} [(1 2) (3 4) (5 6)]";
            var parsed = CommandLine.Instance.Parse<CommandsList>( args );

            Assert.IsTrue( parsed.ListComplexProperty
                .SequenceEqual( new[] { new Complex( 1, 2 ), new Complex( 3, 4 ), new Complex( 5, 6 ) } ) );
        }

        [TestMethod]
        public void EnumerableNoConversionMethod()
        {
            var args = $"--{nameof( CommandsEnumerable.EnumerableNoConversionMethod )} [1 2 3 4 5]";
            var parsed = CommandLine.Instance.Parse<CommandsEnumerable>( args );

            Assert.IsTrue( parsed.EnumerableNoConversionProperty
                .SequenceEqual( new[] { "1", "2", "3", "4", "5" } ) );
        }

        [TestMethod]
        public void EnumerableConversionMethod()
        {
            var args = $"--{nameof( CommandsEnumerable.EnumerableConversionMethod )} [1 2 3 4 5]";
            var parsed = CommandLine.Instance.Parse<CommandsEnumerable>( args );

            Assert.IsTrue( parsed.EnumerableConversionProperty
                .SequenceEqual( new[] { 1, 2, 3, 4, 5 } ) );
        }

        [TestMethod]
        public void EnumerableComplexMethod()
        {
            var args = $"--{nameof( CommandsEnumerable.EnumerableComplexMethod )} [(1 2) (3 4) (5 6)]";
            var parsed = CommandLine.Instance.Parse<CommandsEnumerable>( args );

            Assert.IsTrue( parsed.EnumerableComplexTypeProperty
                .SequenceEqual( new[] { new Complex( 1, 2 ), new Complex( 3, 4 ), new Complex( 5, 6 ) } ) );
        }

        [TestMethod]
        public void ArrayNoConversionMethod()
        {
            var args = $"--{nameof( CommandsArray.ArrayNoConversionMethod )} [1 2 3 4 5]";
            var parsed = CommandLine.Instance.Parse<CommandsArray>( args );

            Assert.IsTrue( parsed.ArrayNoConversionProperty
                .SequenceEqual( new[] { "1", "2", "3", "4", "5" } ) );
        }

        [TestMethod]
        public void ArrayConversionMethod()
        {
            var args = $"--{nameof( CommandsArray.ArrayConversionMethod )} [1 2 3 4 5]";
            var parsed = CommandLine.Instance.Parse<CommandsArray>( args );

            Assert.IsTrue( parsed.ArrayConversionProperty
                .SequenceEqual( new[] { 1, 2, 3, 4, 5 } ) );
        }

        [TestMethod]
        public void ArrayComplex()
        {
            var args = $"--{nameof( CommandsArray.ArrayComplexMethod )} [(1 2) (3 4) (5 6)]";
            var parsed = CommandLine.Instance.Parse<CommandsArray>( args );

            Assert.IsTrue( parsed.ArrayComplexTypeProperty
                .SequenceEqual( new[] { new Complex( 1, 2 ), new Complex( 3, 4 ), new Complex( 5, 6 ) } ) );
        }
    }
}
