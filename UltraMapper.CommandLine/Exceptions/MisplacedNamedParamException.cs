using UltraMapper.Parsing;

namespace UltraMapper.CommandLine
{
    public class MisplacedNamedParamException : UltraMapperCommandLineException
    {
        private const string _errorMsg = "Named parameters must appear after all non-named parameters";

        public MisplacedNamedParamException( IParsedParam param )
            : base( _errorMsg ) { }
    }
}
