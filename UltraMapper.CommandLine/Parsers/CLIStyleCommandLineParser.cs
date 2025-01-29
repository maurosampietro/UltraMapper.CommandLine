using System;
using System.Collections.Generic;
using System.Linq;
using UltraMapper.Parsing;

namespace UltraMapper.CommandLine.Parsers
{
    public class CliStyleCommandLineParser : ICommandLineParser
    {
        public IEnumerable<ParsedCommand> Parse( string commandLine )
        {
            if(string.IsNullOrWhiteSpace( commandLine ))
                throw new ArgumentException( "Command line cannot be null or empty.", nameof( commandLine ) );

            return Parse( commandLine.Split( [' '], StringSplitOptions.RemoveEmptyEntries ) );
        }

        public IEnumerable<ParsedCommand> Parse( string[] commands )
        {
            var parsedCommands = new List<ParsedCommand>();
            ParsedCommand? currentCommand = null;
            int index = 0;

            foreach(var token in commands)
            {
                if(token.StartsWith( "--" ))
                {
                    // Long option with optional value (--key=value or --key)
                    var split = token.Substring( 2 ).Split( '=', 2 );
                    var param = new SimpleParam
                    {
                        Name = split[ 0 ],
                        Value = split.Length > 1 ? split[ 1 ] : null,
                        Index = index++
                    };
                    AddParamToCommand( currentCommand, param );
                }
                else if(token.StartsWith( "-" ))
                {
                    // Short options (-a -b value)
                    foreach(var opt in token.Substring( 1 ))
                    {
                        var param = new SimpleParam
                        {
                            Name = opt.ToString(),
                            Index = index++
                        };
                        AddParamToCommand( currentCommand, param );
                    }
                }
                else
                {
                    if(currentCommand == null)
                    {
                        // First token is the command
                        currentCommand = new ParsedCommand
                        {
                            Name = token,
                            Param = new ComplexParam()
                        };
                        parsedCommands.Add( currentCommand );
                    }
                    else
                    {
                        // Remaining tokens are arguments
                        var param = new SimpleParam
                        {
                            Name = string.Empty, // Anonymous argument
                            Value = token,
                            Index = index++
                        };
                        AddParamToCommand( currentCommand, param );
                    }
                }
            }

            return parsedCommands;
        }

        private void AddParamToCommand( ParsedCommand? command, IParsedParam param )
        {
            if(command == null)
                throw new InvalidOperationException( "Cannot add parameters to a null command." );

            if(command.Param is ComplexParam complexParam)
            {
                complexParam.SubParams = complexParam.SubParams.Append( param ).ToList();
            }
            else
            {
                throw new InvalidOperationException( "Command parameter must be of type ComplexParam." );
            }
        }
    }
}