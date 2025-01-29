using System;
using System.Collections.Generic;
using System.Linq;
using UltraMapper.Parsing;

namespace UltraMapper.CommandLine.Parsers
{
    public class UnixStyleCommandLineParser : ICommandLineParser
    {
        public IEnumerable<ParsedCommand> Parse( string commandLine )
        {
            if(string.IsNullOrWhiteSpace( commandLine ))
                throw new ArgumentException( "Command line cannot be null or empty.", nameof( commandLine ) );

            return Parse( commandLine.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries ) );
        }

        public IEnumerable<ParsedCommand> Parse( string[] commands )
        {
            var parsedCommands = new List<ParsedCommand>();
            ParsedCommand? currentCommand = null;
            int index = 0;

            foreach(var token in commands)
            {
                if(token.StartsWith( "-" ))
                {
                    // Options (e.g., -a or -b)
                    var param = new SimpleParam
                    {
                        Name = token,
                        Index = index++
                    };
                    AddParamToCommand( currentCommand, param );
                }
                else
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
                    else
                    {
                        // Remaining tokens are positional arguments
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