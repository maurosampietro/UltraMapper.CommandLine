using System.Collections.Generic;
using System.Linq;

namespace UltraMapper.CommandLine.Parsers
{
    public class ArrayParam : IParsedParam
    {
        public string Name { get; set; }
        public int Index { get; set; }

        public ArrayParam() { }

        public ArrayParam( IEnumerable<IParsedParam> items )
        {
            _items = items.ToList();
        }

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
