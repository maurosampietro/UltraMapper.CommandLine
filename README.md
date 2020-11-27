# UltraMapper.Commandline
[![Build status](https://ci.appveyor.com/api/projects/status/github/maurosampietro/UltraMapper.Commandline?svg=true)](https://ci.appveyor.com/project/maurosampietro/ultramapper.Commandline/branch/master)
[![NuGet](http://img.shields.io/nuget/v/UltraMapper.svg)](https://www.nuget.org/packages/UltraMapper.Commandline/)

A commandline parser supporting direct method calls taking as input an unlimited number of primitive and complex-type parameters



What is UltraMapper.CommandLine?
--------------------------------

UltraMapper.CommandLine is a .NET <b>command line parser</b>: a tool that <b>parse</b> (analyze and interpret) your command line text and <b>map</b> (trasform) it into <b>strongly-typed</b> objects.    

UltraMapper.CommandLine drastically simplifies your code: 
    
- **Methods calls support** allows you to get rid of all of the 'commandline flags' and all of the code needed to handle them
- **Complex-type support** allows you to organize parameters logically in classes

UltraMapper.CommandLine is powered by [UltraMapper](https://github.com/maurosampietro/UltraMapper), a powerful .NET mapper!

Game changing features!
--------------------------------

- **Direct method calls** supporting an **unlimited** number of **input parameters**
- **Complex types** support for both **properties** and **methods**
- **Nesting**. Complex types can internally define other complex-type members recursively.
- **Type inheritance** is fully supported.
- **Collections** (IEnumerable<>, List<>, arrays) of both **primitive** and **complex types** support
- A **simple JSON like syntax** supporting all of this!
- Automatic **help generator**


Example
--------------------------------

Ok this will have some complexity to it, but i want to impress you!          

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


The above example shows how to call the _add_ method from commandline, passing as input a string representation of the well structured complex-type _CustomerInfo_ as parameter.   

The parameter passed as input to the method _add_ is a compex-type of type _CustomerInfo_ which internally refers to another complex-type of type _BankAccountInfo_ 
which internally defines a collection of yet another complex-type of type '_CreditCardInfo_'.    
    
The above example also shows a few features:
    
   - The basic syntax:        
        - Use _--_ <b>double dashes</b> to call a method or set a property defined in 'CustomerCommands'    
        - Use _()_ <b>round brackets</b> to provide values to a <b>complex type</b>    
        - Use _[]_ <b>square brackets</b> to provide values to a <b>collection</b>    
        - Anonymous and named params!    
   
       [Read more about the syntax here](https://github.com/maurosampietro/UltraMapper.CommandLine/wiki/Default-syntax)     
    
   - The Option attribute:        
        Allows you to override a member's name, ignore a member or provide a help description.

        [Read more about the Option attribute here](https://github.com/maurosampietro/UltraMapper.CommandLine/wiki/OptionAttribute)     

Getting started
--------------------------------

Check out the [wiki](https://github.com/maurosampietro/UltraMapper.CommandLine/wiki) for more information and advanced scenarios
    
    
    
    
    
**ANY FEEDBACK IS WELCOME**
