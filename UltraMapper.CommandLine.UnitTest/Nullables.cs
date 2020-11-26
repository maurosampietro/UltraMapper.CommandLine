using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltraMapper.CommandLine.UnitTest
{
    [TestClass]
    public class Nullables
    {
        public class NullableProperty
        {
            public int? Option { get; set; }
        }

        public class NullableMethod
        {
            [Option( IsIgnored = true )] public bool IsExecuted { get; set; }
            [Option( IsIgnored = true )] public int? MethodParam { get; private set; } = null;

            public void Method( int? option )
            {
                this.IsExecuted = true;
                this.MethodParam = option;
            }
        }

        public class NullableCollectionProperty
        {
            public IEnumerable<int?> Option { get; set; }
        }

        public class NullableCollectionMethod
        {
            [Option( IsIgnored = true )] public bool IsExecuted { get; set; }
            [Option( IsIgnored = true )] public IEnumerable<int?> MethodParam { get; private set; } = null;

            public void Method( IEnumerable<int?> option )
            {
                this.IsExecuted = true;
                this.MethodParam = option;
            }
        }

        [TestMethod]
        public void SetNullableProperty()
        {
            string args = $"--{nameof( NullableProperty.Option )} 11";
            var parsed = CommandLine.Instance.Parse<NullableProperty>( args );
            Assert.IsTrue( parsed.Option == 11 );
        }

        [TestMethod]
        public void SetNullableMethodParameter()
        {
            string args = $"--{nameof( NullableMethod.Method )} 11";
            var parsed = CommandLine.Instance.Parse<NullableMethod>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.MethodParam == 11 );
        }

        [TestMethod]
        public void SetNullableCollectionProperty()
        {
            string args = $"--{nameof( NullableCollectionProperty.Option )} [11 13]";
            var parsed = CommandLine.Instance.Parse<NullableCollectionProperty>( args );
            Assert.IsTrue( parsed.Option.SequenceEqual( new int?[] { 11, 13 } ) );
        }

        [TestMethod]
        public void SetNullableCollectionMethodParameter()
        {
            string args = $"--{nameof( NullableCollectionMethod.Method )} [11 13]";
            var parsed = CommandLine.Instance.Parse<NullableCollectionMethod>( args );

            Assert.IsTrue( parsed.IsExecuted );
            Assert.IsTrue( parsed.MethodParam.SequenceEqual( new int?[] { 11, 13 } ) );
        }
    }
}
