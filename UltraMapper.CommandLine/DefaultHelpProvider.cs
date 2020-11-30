using System;
using System.Collections.Generic;
using System.Linq;
using UltraMapper.CommandLine.Mappers;
using UltraMapper.CommandLine.Parsers;

namespace UltraMapper.CommandLine
{
    public class DefaultHelpProvider : IHelpProvider
    {
        public bool ShowHelpOnError { get; set; } = true;

        public ParameterDefinition HelpCommandDefinition => new ParameterDefinition()
        {
            Name = "Help",

            Options = new OptionAttribute()
            {
                HelpText = "Shows usage info"
            },

            SubParams = new[]
            {
                new ParameterDefinition()
                {
                    Name = "Command",
                    Type = typeof( string ),
                    Options = new OptionAttribute()
                    {
                        HelpText = "The name of the command you want more info about",
                        IsRequired = false
                    }
                }
            },

            Type = typeof( string )
        };

        public void Initialize( Type type )
        {
            var defs = DefinitionHelper.GetCommandDefinitions( type ).ToList();
            if( !defs.Any( cmd => cmd.Name.ToLower() == this.HelpCommandDefinition.Name.ToLower() ) )
            {
                defs.Add( HelpCommandDefinition );

                DefinitionHelper.Update( type, defs.ToArray() );
            }
        }

        public void GetHelp( Type type, IParsedParam parsedParam )
        {
            this.Initialize( type );

            string helpParam = ((SimpleParam)parsedParam)?.Value;

            var cmdDefs = DefinitionHelper.GetCommandDefinitions( type );
            cmdDefs.OrderBy( cDef => cDef.Name ).ToList();

            if( String.IsNullOrEmpty( helpParam ) )
            {
                string indentation = new string( ' ', 2 );
                Console.WriteLine();

                Console.WriteLine( $"{indentation}AVAILABLE COMMANDS:" );
                Console.WriteLine();

                Console.WriteLine( $"{indentation}{indentation}{"COMMAND",-20}{"DESCRIPTION"}" );
                Console.WriteLine();

                foreach( var cmdDef in cmdDefs )
                    Console.WriteLine( $"{indentation}{indentation}{cmdDef.Name.ToLower(),-20}{cmdDef.Options.HelpText}" );

                Console.WriteLine();
                Console.WriteLine( $"{indentation}To invoke a command, type --<command>" );
                Console.WriteLine( $"{indentation}For more info about a specific command, type --help <command>" );
                //Console.WriteLine( $"{indentation}Collection parameters must be surrounded by []" );
                //Console.WriteLine( $"{indentation}Non primitives types must be surrounded by ()" );
                Console.WriteLine();
            }

            //else if( !(helpCmd.Param is SimpleParam) )
            //{
            //    Console.WriteLine( "--help accepts only one input argument" );

            //    //if( this.ShowHelpOnError )
            //    //    this.Parse<T>( $"--{HelpCommandDefinition.Name.ToLower()} {HelpCommandDefinition.Name.ToLower()}" );
            //    //else
            //    //    Console.WriteLine();
            //}
            else
            {
                var cmdUsage = cmdDefs.FirstOrDefault( cmd => cmd.Name.ToLower() == helpParam.ToLower() );
                if( cmdUsage == null )
                {
                    Console.WriteLine( $"A command named '{helpParam}' does not exist" );

                    //if( this.ShowHelpOnError )
                    //    this.Parse<T>( $"--{HelpCommandDefinition.Name.ToLower()}" );
                    //else
                    //    Console.WriteLine();
                }
                else
                {
                    string indentation = new string( ' ', 2 );

                    Console.WriteLine();
                    Console.WriteLine( $"{indentation}COMMAND '{cmdUsage.Name.ToUpper()}'" );

                    if( !String.IsNullOrWhiteSpace( cmdUsage.Options.HelpText ) )
                        Console.WriteLine( $"{indentation}{indentation}{cmdUsage.Options.HelpText}" );

                    Console.WriteLine();
                    Console.WriteLine( $"{indentation}USAGE:" );

                    var parameters = cmdUsage.SubParams.Select( p => p.Options.IsRequired ? p.Name.ToLower() : $"[{p.Name.ToLower()}]" );
                    Console.WriteLine( $"{indentation}{indentation}--{cmdUsage.Name.ToLower() } {String.Join( " ", parameters )}" );

                    if( cmdUsage.SubParams.Length > 0 )
                    {
                        Console.WriteLine();
                        Console.WriteLine( $"{indentation}PARAMETERS DETAILS:" );
                        Console.WriteLine();

                        int namePadRight = Math.Max( cmdUsage.SubParams.Max( p => p.Name.Length ), "NAME".Length ) + 3;
                        int typePadRight = Math.Max( cmdUsage.SubParams.Max( p => p.Type.GetPrettifiedName().Length ), "TYPE".Length ) + 3;
                        int requiredPadRight = "REQUIRED".Length + 3;

                        Console.WriteLine( $"{indentation}{indentation}{"NAME".PadRight( namePadRight )}{"TYPE".PadRight( typePadRight )}{"REQUIRED".PadRight( requiredPadRight )}DESCRIPTION" );
                        foreach( var param in cmdUsage.SubParams )
                        {
                            string required = param.Options.IsRequired ? "Yes" : "No";
                            string description = param.Options.HelpText ?? "<No description provided>";

                            Console.WriteLine( $"{indentation}{indentation}{param.Name.PadRight( namePadRight )}{param.Type.GetPrettifiedName().PadRight( typePadRight )}{required.PadRight( requiredPadRight )}{description}" );
                        }
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
