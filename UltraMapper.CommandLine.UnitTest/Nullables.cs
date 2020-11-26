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
        public class Commands
        {
            public int? Option { get; set; }

            [Option( IsIgnored = true )] public bool IsExecuted { get; set; }
            [Option( IsIgnored = true )] public int? MethodParam { get; private set; } = null;

            public void Method( int? option )
            {
                this.IsExecuted = true;
                this.MethodParam = option;
            }
        }

        [TestMethod]
        public void SetNullableProperty()
        {
            string args = $"--{nameof( Commands.Option )} 11";
            var parsed = CommandLine.Instance.Parse<Commands>( args );
            Assert.IsTrue( parsed.Option == 11 );
        }

        [TestMethod]
        public void SetNullableMethodParameter()
        {
            string args = $"--{nameof( Commands.Method )} 11";
            var parsed = CommandLine.Instance.Parse<Commands>( args );

            Assert.IsTrue( parsed.IsExecuted ); 
            Assert.IsTrue( parsed.MethodParam == 11 );
        }
    }
}
