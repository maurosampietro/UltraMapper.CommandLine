using System;

namespace UltraMapper.CommandLine
{
    [Serializable]
    public class DuplicateParameterException : UltraMapperCommandLineException
    {
        private const string errorMsg = "Parameter '{0}' is defined multiple times";

        public DuplicateParameterException( string paramName )
            : base( String.Format( errorMsg, paramName ) ) { }
    }
}
