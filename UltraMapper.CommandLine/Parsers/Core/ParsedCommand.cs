namespace CommandLine.AutoParser.Parsers
{
    public class ParsedCommand
    {
        public string Name { get; set; }
        public IParsedParam Param { get; set; }
    }
}
