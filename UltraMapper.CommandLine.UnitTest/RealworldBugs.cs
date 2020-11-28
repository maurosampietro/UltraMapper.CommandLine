using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                public InnerType Inner2 { get; set; }
            }

            public InnerType Move { get; set; }
        }

        [TestMethod]
        [Ignore]
        public void BugNestingAndReferringTheSameType()
        {
            var args = "--move a=fromhere b=tohere inner2=(a=a a=b)";
            Assert.ThrowsException<DuplicateArgumentException>(
                () => CommandLine.Instance.Parse<Commands2>( args ) );
        }
    }
}
