using System;
using UltraMapper.CommandLine.Parsers;

namespace UltraMapper.CommandLine
{
    [Serializable]
    public class UltraMapperCommandLineException : Exception
    {
        public UltraMapperCommandLineException( string message )
            : base( message ) { }

        public UltraMapperCommandLineException( string message, Exception innerException )
            : base( message, innerException ) { }
    }


    [Serializable]
    public class SyntaxErrorException : UltraMapperCommandLineException
    {
        public SyntaxErrorException( string message = "Syntax error" )
            : base( message ) { }
    }

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

    [Serializable]
    public class UndefinedCommandException : UltraMapperCommandLineException
    {
        private const string errorMsg = "Type '{0}' does not define a command named '{1}'";

        public UndefinedCommandException( Type target, string commandName )
            : base( String.Format( errorMsg, target, commandName ) ) { }
    }

    [Serializable]
    public class UndefinedParameterException : UltraMapperCommandLineException
    {
        private const string errorMsg = "Type '{0}' does not define a parameter named '{1}'";

        public UndefinedParameterException( Type target, string paramName )
            : base( String.Format( errorMsg, target, paramName ) ) { }
    }

    [Serializable]
    public class DuplicateParameterException : UltraMapperCommandLineException
    {
        private const string errorMsg = "Parameter '{0}' is defined multiple times";

        public DuplicateParameterException( string paramName )
            : base( String.Format( errorMsg, paramName ) ) { }
    }

    [Serializable]
    public class DuplicateArgumentException : UltraMapperCommandLineException
    {
        private const string errorMsg = "Argument '{0}' is specified multiple times";

        public DuplicateArgumentException( string paramName )
            : base( String.Format( errorMsg, paramName ) ) { }
    }

    [Serializable]
    public class DuplicateCommandException : UltraMapperCommandLineException
    {
        private const string errorMsg = "Type '{0}' defines multiple commands named '{1}'. " +
          "Please use the Name property of the Option attribute to assign a distinct names.";

        public DuplicateCommandException( Type target, ParsedCommand command )
            : base( String.Format( errorMsg, target, command.Name ) ) { }
    }

    [Serializable]
    public class InvalidNameException : UltraMapperCommandLineException
    {
        private const string _errorMsg = "Invalid name '{0}'. Names must be non-empty, alphanumerical and containing no special characters";

        public InvalidNameException( string invalidName )
            : base( String.Format( _errorMsg, invalidName ) ) { }
    }
}
