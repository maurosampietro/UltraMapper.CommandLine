# UltraMapper.Commandline
[![Build status](https://ci.appveyor.com/api/projects/status/github/maurosampietro/UltraMapper.Commandline?svg=true)](https://ci.appveyor.com/project/maurosampietro/ultramapper.Commandline/branch/master)
[![NuGet](http://img.shields.io/nuget/v/UltraMapper.svg)](https://www.nuget.org/packages/UltraMapper.Commandline/)

A nicely coded commandline parser for .NET 



What is UltraMapper.CommandLine?
--------------------------------

UltraMapper.CommandLine is a .NET <b>command line parser</b>: a tool that parse and map command line arguments into strongly-typed objects.    
UltraMapper.CommandLine is powered by [UltraMapper](https://github.com/maurosampietro/UltraMapper), a powerful .NET mapper!

Game changing features!
--------------------------------

- <b>Direct method call</b> supporting unlimited input parameters
- <b>Complex types</b> support for both <b>properties</b> and <b>methods</b>
- <b>Nesting</b>. Complex types can be nested in other complex types.
- Complex types support <b>unlimited inheritance</b>.
- <b>Collections</b> (IEnumerable<>, List<>, arrays) of both <b>primitive</b> and <b>complex types</b> support
- A <b>simple JSON like syntax</b> supporting all of this!
- Automatic <b>help generator</b>

UltraMapper.CommandLine drastically simplifies code: 
    
- No more flags to signal you need to execute a method and     
  no additional code to react to the flags and call methods:

  ...just execute methods directly!

- No more unrelated messy properties: organize them logically in classes!


Example
--------------------------------

Ok this will have some complexity to it, but i want to impress you!       
The following example shows how to call a method taking as input a well structured complex type having defined collections and another nested complex type.
    
The following example also shows the basic syntax:    
 - Use _--_ <b>double dashes</b> to call a method or set a property defined in 'CustomerCommands'    
 - Use _()_ <b>round brackets</b> to provide values to a <b>complex type</b>    
 - Use _[]_ <b>square brackets</b> to provide values to a <b>collection</b>    
 - Anonymous and named params!        
    
Check out the wiki to more information about the syntax

    class Program
    {
        static void Main( string[] args )
        {
            //--add ("John Smith" 26 account=( number=AC2903X balance=3500.00 creditcards=[(CRD01 1000.00) (CRD02 2000.00)]))
            ConsoleLoop.Start<CustomerCommands>( args );
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

                Console.WriteLine( "new customer inserted!" );
            }
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
     }

Getting started
--------------------------------

Check out the [wiki](https://github.com/maurosampietro/UltraMapper.CommandLine/wiki) for more information and advanced scenarios
    
    
    
    
    
**ANY FEEDBACK IS WELCOME**
