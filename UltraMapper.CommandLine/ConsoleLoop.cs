using System;

namespace UltraMapper.CommandLine
{
    public class ConsoleLoop
    {
        public static void Start<T>( CommandLine cmdLine, string[] args,
            Action<T> onParsed = null, Action<Exception> onError = null ) where T : class, new()
        {
            Start( cmdLine, args, new T(), onParsed, onError );
        }

        public static void Start<T>( CommandLine cmdLine, string[] args, T instance,
            Action<T> onParsed = null, Action<Exception> onError = null ) where T : class
        {
            var helpCommandName = cmdLine.HelpProvider.HelpCommandDefinition.Name;

            while( true )
            {
                try
                {
                    Console.Write( "> " );

                    if( args == null || args.Length == 0 )
                    {
                        string commandLine = Console.ReadLine();

                        cmdLine.Parse( commandLine, instance );
                        onParsed?.Invoke( instance );
                    }
                    else
                    {
                        try
                        {
                            cmdLine.Parse( args, instance );
                            onParsed?.Invoke( instance );
                        }
                        finally
                        {
                            args = null;
                        }
                    }
                }
                catch( UndefinedCommandException argumentEx )
                {
                    onError?.Invoke( argumentEx );

                    Console.WriteLine( argumentEx.Message );
                    Console.WriteLine();

                    if( cmdLine.HelpProvider.ShowHelpOnError )
                        cmdLine.Parse( $"--{helpCommandName}", instance );
                }
                catch( Exception ex )
                {
                    onError?.Invoke( ex );
                    Console.WriteLine( ex.Message );
                }
            }
        }
    }
}
