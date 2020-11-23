namespace CommandLine.AutoParser.Parsers
{
    public interface IParsedParam
    {
        string Name { get; set; }
        int Index { get; set; }
    }
}
