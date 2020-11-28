using System;
using UltraMapper.CommandLine.Parsers;

namespace UltraMapper.CommandLine
{
    [Serializable]
    public class DuplicateCommandException : UltraMapperCommandLineException
    {
        private const string errorMsg = "Type '{0}' defines multiple commands named '{1}'. " +
          "Please use the Name property of the Option attribute to assign a distinct names.";

        public DuplicateCommandException( Type target, ParsedCommand command )
            : base( String.Format( errorMsg, target, command.Name ) ) { }
    }
}
