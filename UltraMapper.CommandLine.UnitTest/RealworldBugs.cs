using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace UltraMapper.CommandLine.UnitTest
{
    [TestClass]
    public class RealworldBugs
    {
        public class Commands2
        {
            public class InnerType
            {
                [Option( IsRequired = true, Order = 1 )]
                public string A { get; set; }

                [Option( IsRequired = true, Order = 2 )]
                public string B { get; set; }

                [Option( IsRequired = false, Order = 0 )]
                public InnerType Inner2 { get; set; }
            }

            public InnerType Move { get; set; }
        }

        [TestMethod]
        public void DuplicateArgumentInnerParamAndCircularReference()
        {
            var args = "--move a=fromhere b=tohere inner2=(a=a a=b)";
            Assert.ThrowsException<DuplicateArgumentException>(
                () => CommandLine.Instance.Parse<Commands2>( args ) );
        }

        [TestMethod]
        public void CircularReference()
        {
            var args = "--move a=a b=b inner2=(a=aa b=bb inner2=(a=aaa b=bbb inner2=(a=aaaa b=bbbb)))";
            var parsed = CommandLine.Instance.Parse<Commands2>( args );

            Assert.IsTrue( parsed.Move.A == "a" );
            Assert.IsTrue( parsed.Move.B == "b" );
            Assert.IsTrue( parsed.Move.Inner2.A == "aa" );
            Assert.IsTrue( parsed.Move.Inner2.B == "bb" );
            Assert.IsTrue( parsed.Move.Inner2.Inner2.A == "aaa" );
            Assert.IsTrue( parsed.Move.Inner2.Inner2.B == "bbb" );
            Assert.IsTrue( parsed.Move.Inner2.Inner2.Inner2.A == "aaaa" );
            Assert.IsTrue( parsed.Move.Inner2.Inner2.Inner2.B == "bbbb" );
        }

        [TestMethod]
        //[Ignore] //this should throw and it's ok, but it's not easy to understand where the problem is
        public void ComplexAnonymousParamOnSimpleParamOrderOvverride()
        {
            //order is set overridden manually so the order should be:
            //a=aa b=bb (a=aaa b=bbb) 
            //when checking for order, also check for param type???

            var args = "--move (a=aaa b=bbb) a=aa b=bb";
            var parsed = CommandLine.Instance.Parse<Commands2>( args );
        }

        public class CustomerInfo
        {
            public class BankAccountInfo
            {
                public class CreditCardInfo
                {
                    public string CardNumber { get; set; }
                    public double MonthlyLimit { get; set; }
                }

                [Option( Name = "number" )]
                public string AccountNumber { get; set; }
                public double Balance { get; set; }

                public List<CreditCardInfo> CreditCards { get; set; }
            }

            public string Name { get; set; }
            public int Age { get; set; }

            public BankAccountInfo Account { get; set; }
        }

        [TestMethod]
        public void GitHubExample()
        {
            var args = "--add (\"John Smith\" 26 account=(number=AC2903X balance=3500.00 creditcards=[(CRD01 1000.00) (CRD02 2000.00)]))";
            var parsed = CommandLine.Instance.Parse<CustomerCommands>( args );
        }

        [TestMethod]
        public void GitHubExample2()
        {
            var args = "--add (\"John Smith\" 26 (number=AC2903X balance=3500.00 creditcards=[(CRD01 1000.00) (CRD02 2000.00)]))";
            var parsed = CommandLine.Instance.Parse<CustomerCommands>( args );
        }

        public class CustomerCommands
        {
            [Option( Name = "add", HelpText = "Adds new customer to db" )]
            public void AddToDatabase( CustomerInfo customer )
            {
                Assert.IsTrue( customer.Name == "John Smith" );
                Assert.IsTrue( customer.Age == 26 );
                Assert.IsTrue( customer.Account.AccountNumber == "AC2903X" );
                Assert.IsTrue( customer.Account.Balance == 3500 );
                Assert.IsTrue( customer.Account.CreditCards[ 0 ].CardNumber == "CRD01" );
                Assert.IsTrue( customer.Account.CreditCards[ 0 ].MonthlyLimit == 1000 );
                Assert.IsTrue( customer.Account.CreditCards[ 1 ].CardNumber == "CRD02" );
                Assert.IsTrue( customer.Account.CreditCards[ 1 ].MonthlyLimit == 2000 );
            }
        }
    }
}
