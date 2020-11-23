using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace CommandLine.AutoParser.Example
{
    class Program
    {
        static void Main( string[] args )
        {
            //throw new Exception( "se scrivo --help sleeping time si rompe! sourceinstnace vs param access null" );
            ConsoleLoop.Start<Commands>( args );
        }

        public class Commands
        {
            [Option( Name = "sleep", HelpText = "The amount of time to wait before exiting this application in milliseconds" )]
            public int SleepingTime { get; set; }

            [Option( HelpText = "Sum a list of numers and output the result" )]
            public void Sum( IEnumerable<int> numbers )
            {
                Console.WriteLine( $"The sum is: {numbers.Sum()}" );
            }

            [Option( HelpText = "Clears the screen" )]
            public void Clear()
            {
                Console.Clear();
            }

            [Option( HelpText = "Closes this application" )]
            public void Exit()
            {
                Console.WriteLine( "Application will close in 5 seconds" );
                Thread.Sleep( this.SleepingTime );
                Environment.Exit( 0 );
            }

            [Option( HelpText = "Opens a given path in explorer" )]
            public void Open( string path )
            {
                Process.Start( path );
            }
        }

        public class MyCommands
        {
            public class MoveCommand
            {
                [Option( IsRequired = true, HelpText = "The source location of the file" )]
                public string From { get; set; }

                [Option( IsRequired = true, HelpText = "The destination location of the file" )]
                public string To { get; set; }
            }

            [Option( HelpText = "Moves the file located in a to location b" )]
            public MoveCommand Move { get; set; }

            [Option( Name = "quit", HelpText = "Closes this application" )]
            public void Exit()
            {
                Console.WriteLine( "Application will close in 5 seconds" );
                Thread.Sleep( 5000 );
                Environment.Exit( 0 );
            }

            [Option( HelpText = "Opens a given path in explorer" )]
            public void Open( string path )
            {
                Console.WriteLine( $"opening {path}" );
            }

            [Option( HelpText = "Moves the file located in a to location b" )]
            public void MoveMethod( [Option( Name = "f", HelpText = "The source location of the file" )] string from, string to )
            {
                Console.WriteLine( $"moving file from '{from}' to '{to}'" );
            }

            [Option( HelpText = "Echoes stuff on this console" )]
            public void Print( string line, string[] lines, IEnumerable<int> numbers )
            {
                Console.WriteLine( line );

                foreach( var l in lines )
                    Console.WriteLine( l );

                Console.WriteLine( String.Join( " ", numbers ) );
            }

            [Option( HelpText = "Clears the screen" )]
            public void Clear()
            {
                Console.Clear();
            }

            [Option( HelpText = "Tests complex type input as argument" )]
            public void Complex( MoveCommand command )
            {
                Console.WriteLine( $"You want to move from:{command.From} to:{command.To}" );
            }

            //public (string from, string to) Move2 { get; set; }
        }
    }
}
