using System;
using UltraMapper.CommandLine.Parsers;

namespace UltraMapper.CommandLine
{
    [Serializable]
    public class ArgumentNumberException : Exception
    {
        private const string _errorMsg = "Wrong number of arguments passed to command '{0}'";

        public readonly ParsedCommand Command;
        public readonly IParsedParam Param;

        public ArgumentNumberException( ParsedCommand command )
            : base( String.Format( _errorMsg, command.Name ) )
        {
            this.Command = command;
        }

        public ArgumentNumberException( IParsedParam param )
            : base( String.Format( _errorMsg, param.Name ) )
        {
            this.Param = param;
        }
    }
}
