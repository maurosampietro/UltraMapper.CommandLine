using System;

namespace UltraMapper.CommandLine
{
    [Serializable]
    public class UndefinedParameterException : UltraMapperCommandLineException
    {
        private const string errorMsg = "Type '{0}' does not define a parameter named '{1}'";

        public UndefinedParameterException( Type target, string paramName )
            : base( String.Format( errorMsg, target, paramName ) ) { }
    }
}
