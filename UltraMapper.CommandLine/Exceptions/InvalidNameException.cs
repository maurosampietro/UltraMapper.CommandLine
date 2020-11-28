using System;

namespace UltraMapper.CommandLine
{
    [Serializable]
    public class InvalidNameException : UltraMapperCommandLineException
    {
        private const string _errorMsg = "Invalid name '{0}'. Names must be non-empty, alphanumerical and containing no special characters";

        public InvalidNameException( string invalidName )
            : base( String.Format( _errorMsg, invalidName ) ) { }
    }
}
