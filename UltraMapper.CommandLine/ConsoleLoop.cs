using System;

namespace UltraMapper.CommandLine
{
    public class ConsoleLoop
    {
        public static void Start<T>( string[] args, Action<T> onParsed = null ) where T : class, new()
        {
            var autoparser = CommandLine.Instance;

            while( true )
            {
                try
                {
                    Console.Write( "> " );

                    string commandLine = (args == null || args.Length == 0) ?
                        Console.ReadLine() : String.Join( " ", args );

                    args = null;
                    var parsed = autoparser.Parse<T>( commandLine );
                    onParsed?.Invoke( parsed );
                }
                catch( UndefinedParameterException undefParam )
                {
                    Console.WriteLine( undefParam.Message );
                    Console.WriteLine();

                    //if( autoparser.HelpProvider.ShowHelpOnError )
                    //    autoparser.Parse<T>( $"--{autoparser.HelpProvider.HelpCommandDefinition.Name} {undefParam.Command.Name}" );
                }
                catch( ArgumentException argumentEx )
                {
                    Console.WriteLine( argumentEx.Message );
                    Console.WriteLine();

                    //if( autoparser.HelpProvider.ShowHelpOnError )
                    //    autoparser.Parse<T>( $"--{autoparser.HelpProvider.HelpCommandDefinition.Name} {argumentEx.CommandDefinition.Name}" );
                }
                catch( CommandLineException ex )
                {
                    Console.WriteLine( ex.Message );
                    Console.WriteLine();

                    //if( autoparser.HelpProvider.ShowHelpOnError )
                    //    autoparser.Parse<T>( $"--{autoparser.HelpProvider.HelpCommandDefinition.Name}" );
                }
                catch( Exception ex )
                {
                    Console.WriteLine( ex.Message );
                }
            }
        }

        public static void Start<T>( string[] args, T instance, Action<T> onParsed = null ) where T : class
        {
            var autoparser = CommandLine.Instance;

            while( true )
            {
                try
                {
                    Console.Write( "> " );

                    string commandLine = (args == null || args.Length == 0) ?
                        Console.ReadLine() : String.Join( " ", args );

                    args = null;
                    var parsed = autoparser.Parse( commandLine, instance );
                    onParsed?.Invoke( parsed );
                }
                catch( UndefinedParameterException undefParam )
                {
                    Console.WriteLine( undefParam.Message );
                    Console.WriteLine();

                    //if( autoparser.HelpProvider.ShowHelpOnError )
                    //    autoparser.Parse<T>( $"--{autoparser.HelpProvider.HelpCommandDefinition.Name} {undefParam.Command.Name}" );
                }
                catch( ArgumentException argumentEx )
                {
                    Console.WriteLine( argumentEx.Message );
                    Console.WriteLine();

                    //if( autoparser.HelpProvider.ShowHelpOnError )
                    //    autoparser.Parse<T>( $"--{autoparser.HelpProvider.HelpCommandDefinition.Name} {argumentEx.CommandDefinition.Name}" );
                }
                catch( CommandLineException ex )
                {
                    Console.WriteLine( ex.Message );
                    Console.WriteLine();

                    //if( autoparser.HelpProvider.ShowHelpOnError )
                    //    autoparser.Parse<T>( $"--{autoparser.HelpProvider.HelpCommandDefinition.Name}" );
                }
                catch( Exception ex )
                {
                    Console.WriteLine( ex.Message );
                }
            }
        }
    }
}