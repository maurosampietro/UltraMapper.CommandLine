using CommandLine.AutoParser.Parsers;
using System.Collections.Generic;

namespace CommandLine.AutoParser.Mappers
{
    public interface IMapper
    {
        void Initialize( IHelpProvider helpProvider );
        T Map<T>( IEnumerable<ParsedCommand> commands ) where T : class, new();
    }
}
