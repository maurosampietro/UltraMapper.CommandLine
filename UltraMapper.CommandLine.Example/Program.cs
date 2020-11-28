using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace UltraMapper.CommandLine.Example
{
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

        public class Commands
        {
            [Option( Name = "sleep", HelpText = "The amount of time to wait before exiting this application in milliseconds" )]
            public int SleepingTime { get; set; }

            [Option( HelpText = "Closes this application" )]
            public void Exit()
            {
                Console.WriteLine( "Application will close in 5 seconds" );
                Thread.Sleep( this.SleepingTime );
                Environment.Exit( 0 );
            }

            [Option( HelpText = "Opens a given path in explorer" )]
            public void OpenDir( string path )
            {
                Process.Start( path );
            }

            [Option( HelpText = "Opens all the given paths in explorer" )]
            public void OpenDirs( string[] paths )
            {
                foreach( var path in paths )
                    Process.Start( path );
            }

            public class MoveParams
            {
                [Option( IsRequired = true, HelpText = "The source location of the file" )]
                public string From { get; set; }

                [Option( IsRequired = true, HelpText = "The destination location of the file" )]
                public string To { get; set; }
            }

            [Option( HelpText = "Moves the file located in a to location b" )]
            public void MoveFile( MoveParams moveParams )
            {
                Console.WriteLine( $"You want to move from:{moveParams.From} to:{moveParams.To}" );
            }

            [Option( HelpText = "Moves the file located in a to location b" )]
            public void MoveFiles( IEnumerable<MoveParams> moveParams )
            {
                foreach( var item in moveParams )
                    Console.WriteLine( $"moving file from '{item.From}' to '{item.To}'" );
            }
        }

        //TUPLE!
        //public class MyCommands
        //{
        //public (string from, string to) Move2 { get; set; }
        //}
    }
}
