using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace UltraMapper.CommandLine.Example
{
    class Program
    {
        static void Main( string[] args )
        {
            ConsoleLoop.Start<UserInfo>( args, parsed =>
            {
                //do what you want with your strongly-type parsed args
            } );
        }

        public class UserInfo
        {
            //public string Name { get; set; }
            //public int Age { get; set; }
            //public bool IsMarried { get; set; }
            public int? Int { get; set; }
            //public class BankAccountInfo
            //{
            //    public string AccountNumber { get; set; }
            //    public double Balance { get; set; }
            //}

            //public BankAccountInfo BankAccount { get; set; }

            //[Option( IsIgnored = true )]
            //public override string ToString()
            //{
            //    return $"{Name} {Age} {IsMarried} {BankAccount?.AccountNumber}{BankAccount?.Balance}";
            //}
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
