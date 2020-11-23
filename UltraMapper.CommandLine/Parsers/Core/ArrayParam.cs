using System.Collections.Generic;

namespace CommandLine.AutoParser.Parsers
{
    public class ArrayParam : IParsedParam
    {
        public string Name { get; set; }
        public int Index { get; set; }

        private readonly List<IParsedParam> _items = new List<IParsedParam>();
        public IReadOnlyList<IParsedParam> Items => _items;

        public IParsedParam this[ int index ]
        {
            get => this.Items[ index ];
        }

        public void Add( IParsedParam item )
        {
            _items.Add( item );
        }
    }
}
