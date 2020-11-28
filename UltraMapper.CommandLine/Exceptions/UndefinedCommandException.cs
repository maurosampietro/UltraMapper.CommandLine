using System;

namespace UltraMapper.CommandLine
{
    [Serializable]
    public class UndefinedCommandException : UltraMapperCommandLineException
    {
        private const string errorMsg = "Type '{0}' does not define a command named '{1}'";

        public UndefinedCommandException( Type target, string commandName )
            : base( String.Format( errorMsg, target, commandName ) ) { }
    }
}
