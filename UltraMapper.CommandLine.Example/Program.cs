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

            public class MoveParams
            {
                [Option( IsRequired = true, HelpText = "The source location of the file" )]
                public string From { get; set; }

                [Option( IsRequired = true, HelpText = "The destination location of the file" )]
                public string To { get; set; }

                public class Test
                {
                    public int A { get; set; }
                    public int B { get; set; }
                }

                public Test TestParam { get; set; }
            }

            [Option( HelpText = "Moves the file located in a to location b" )]
            public void MoveMethod1( MoveParams moveParams )
            {
                Console.WriteLine( $"You want to move from:{moveParams.From} to:{moveParams.To}" );
                Console.WriteLine( $"test params: {moveParams.TestParam.A}, {moveParams.TestParam.B}");
            }

            [Option( HelpText = "Moves the file located in a to location b" )]
            public void MoveMethod2( [Option( Name = "f", HelpText = "The source location of the file" )] string from, string to )
            {
                Console.WriteLine( $"moving file from '{from}' to '{to}'" );
            }
        }

        //TUPLE!
        //public class MyCommands
        //{
        //   //public (string from, string to) Move2 { get; set; }
        //}
    }
}
