using UltraMapper.Parsing;

namespace UltraMapper.CommandLine.Parsers
{
    public class ParsedCommand
    {
        public string Name { get; set; }
        public IParsedParam Param { get; set; }
    }
}
