using UltraMapper.CommandLine.Parsers;
using System.Collections.Generic;

namespace UltraMapper.CommandLine.Mappers
{
    public interface IMapper
    {
        void Initialize( IHelpProvider helpProvider );
        T Map<T>( IEnumerable<ParsedCommand> commands ) where T : class, new();
        T Map<T>( IEnumerable<ParsedCommand> commands, T instance ) where T : class;
    }
}
