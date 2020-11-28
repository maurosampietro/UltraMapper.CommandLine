using System;

namespace UltraMapper.CommandLine
{
    [Serializable]
    public class SyntaxErrorException : UltraMapperCommandLineException
    {
        public SyntaxErrorException( string message = "Syntax error" )
            : base( message ) { }
    }
}
