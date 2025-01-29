using System;
using System.Collections.Generic;
using System.Linq;
using UltraMapper.Parsing;

namespace UltraMapper.CommandLine.Parsers
{
    public class POSIXStyleCommandLineParser : ICommandLineParser
    {
        public IEnumerable<ParsedCommand> Parse( string commandLine )
        {
            if(string.IsNullOrWhiteSpace( commandLine ))
                throw new ArgumentException( "Command line cannot be null or empty.", nameof( commandLine ) );

            return Parse( SplitCommandLine( commandLine ) );
        }

        public IEnumerable<ParsedCommand> Parse( string[] commands )
        {
            var parsedCommands = new List<ParsedCommand>();
            ParsedCommand? currentCommand = null;
            int index = 0;
            bool isEndOfOptions = false;

            foreach(var token in commands)
            {
                if(currentCommand == null)
                {
                    // First token is the command name
                    currentCommand = new ParsedCommand
                    {
                        Name = token,
                        Param = new ComplexParam()
                    };
                    parsedCommands.Add( currentCommand );
                }
                else if(!isEndOfOptions && token == "--")
                {
                    // End of options indicator
                    isEndOfOptions = true;
                }
                else if(!isEndOfOptions && token.StartsWith( "--" ))
                {
                    // Long options (e.g., --option=value or --option value)
                    var parts = token.Substring( 2 ).Split( '=', 2 );
                    var param = new SimpleParam
                    {
                        Name = parts[ 0 ],
                        Value = parts.Length > 1 ? parts[ 1 ] : Boolean.TrueString,
                        Index = index++
                    };
                    AddParamToCommand( currentCommand, param );
                }
                else if(!isEndOfOptions && token.StartsWith( "-" ) && token.Length > 1)
                {
                    // Short options (e.g., -a, -b, -abc, -o value, or -ovalue)
                    for(int i = 1; i < token.Length; i++)
                    {
                        var option = token[ i ].ToString();

                        if(i == token.Length - 1 && commands.Length > index + 1 && !commands[ index + 1 ].StartsWith( "-" ))
                        {
                            // Last option in the group might have a value (e.g., -o value)
                            var param = new SimpleParam
                            {
                                Name = option,
                                Value = commands[ ++index ],
                                Index = index++
                            };
                            AddParamToCommand( currentCommand, param );
                            break;
                        }
                        else
                        {
                            // Flag-like short option
                            var param = new SimpleParam
                            {
                                Name = option,
                                Value = Boolean.TrueString,
                                Index = index++
                            };
                            AddParamToCommand( currentCommand, param );
                        }
                    }
                }
                else
                {
                    // Positional arguments
                    var param = new SimpleParam
                    {
                        Name = string.Empty, // Anonymous argument
                        Value = token,
                        Index = index++
                    };
                    AddParamToCommand( currentCommand, param );
                }
            }

            return parsedCommands;
        }

        private string[] SplitCommandLine( string commandLine )
        {
            var inQuotes = false;
            var currentToken = string.Empty;
            var tokens = new List<string>();

            foreach(var c in commandLine)
            {
                if(c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if(char.IsWhiteSpace( c ) && !inQuotes)
                {
                    if(currentToken.Length > 0)
                    {
                        tokens.Add( currentToken );
                        currentToken = string.Empty;
                    }
                }
                else
                {
                    currentToken += c;
                }
            }

            if(currentToken.Length > 0)
                tokens.Add( currentToken );

            return tokens.ToArray();
        }

        private void AddParamToCommand( ParsedCommand? command, IParsedParam param )
        {
            if(command == null)
                throw new InvalidOperationException( "Cannot add parameters to a null command." );

            if(command.Param is ComplexParam complexParam)
            {
                complexParam.SubParams.Add( param );
            }
            else
            {
                throw new InvalidOperationException( "Command parameter must be of type ComplexParam." );
            }
        }
    }
}