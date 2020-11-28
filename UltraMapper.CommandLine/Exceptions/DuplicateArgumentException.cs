using System;

namespace UltraMapper.CommandLine
{
    [Serializable]
    public class DuplicateArgumentException : UltraMapperCommandLineException
    {
        private const string errorMsg = "Argument '{0}' is specified multiple times";

        public DuplicateArgumentException( string paramName )
            : base( String.Format( errorMsg, paramName ) ) { }
    }
}
