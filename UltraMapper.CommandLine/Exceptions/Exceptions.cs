using CommandLine.AutoParser.Parsers;
using System;

namespace CommandLine.AutoParser
{
    [Serializable]
    public class CommandLineException : Exception
    {
        public CommandLineException( string message )
            : base( message ) { }
    }

    [Serializable]
    public class CommandLineParserException : CommandLineException
    {
        public CommandLineParserException( string message )
            : base( message ) { }
    }

    [Serializable]
    public class CommandLineSyntaxErrorException : CommandLineException
    {
        public CommandLineSyntaxErrorException( string message ="Syntax error" )
            : base( message ) { }
    }

    [Serializable]
    public class CommandLineMapperException : CommandLineException
    {
        public IParsedParam Command { get; }

        public CommandLineMapperException( IParsedParam command, string message )
            : base( message )
        {
            this.Command = command;
        }
    }

    [Serializable]
    public class CommandLineParserArgumentException : CommandLineMapperException
    {
        public CommandLineParserArgumentException( IParsedParam command, string message )
            : base( command, message ) { }
    }

    [Serializable]
    public class UndefinedParameterException : Exception
    {
        private const string errorMsg = "Type '{0}' does not define a parameter named '{1}'";

        public UndefinedParameterException( Type target, string providedParam )
            : base( String.Format( errorMsg, target, providedParam ) ) { }
    }

    [Serializable]
    public class DuplicateArgumentException : CommandLineParserException
    {
        private const string errorMsg = "Argument '{0}' is specified multiple times";

        public DuplicateArgumentException( string paramName )
            : base( String.Format( errorMsg, paramName ) ) { }
    }
}
