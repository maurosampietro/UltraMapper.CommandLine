using System;

namespace UltraMapper.CommandLine
{
    public class ConsoleLoop
    {
        public static void Start<T>( string[] args, 
            Action<T> onParsed = null ) where T : class, new()
        {
            Start( args, new T(), onParsed );
        }

        public static void Start<T>( string[] args, T instance, 
            Action<T> onParsed = null ) where T : class
        {
            var autoparser = CommandLine.Instance;
            var helpCommandName = autoparser.HelpProvider.HelpCommandDefinition.Name;

            while( true )
            {
                try
                {
                    Console.Write( "> " );

                    string commandLine = (args == null || args.Length == 0) ?
                        Console.ReadLine() : String.Join( " ", args );

                    autoparser.Parse( commandLine, instance );
                    onParsed?.Invoke( instance );

                    args = null;
                }
                catch( UndefinedCommandException argumentEx )
                {
                    Console.WriteLine( argumentEx.Message );
                    Console.WriteLine();

                    if( autoparser.HelpProvider.ShowHelpOnError )
                        autoparser.Parse( $"--{helpCommandName}", instance );
                }
                catch( Exception ex )
                {
                    Console.WriteLine( ex.Message );
                }
            }
        }
    }
}