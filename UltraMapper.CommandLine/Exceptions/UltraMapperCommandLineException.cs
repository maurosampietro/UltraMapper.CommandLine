using System;

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
}
