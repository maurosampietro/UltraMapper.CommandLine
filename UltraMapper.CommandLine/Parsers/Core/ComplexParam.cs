namespace CommandLine.AutoParser.Parsers
{
    public class ComplexParam : IParsedParam
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public IParsedParam[] SubParams { get; set; }

        public IParsedParam this[ int index ]
        {
            get => this.SubParams[ index ];
        }
    }
}
