using CommandLine.AutoParser.Mappers;
using CommandLine.AutoParser.Parsers;
using System.Collections.Generic;
using UltraMapper;
using UltraMapper.MappingExpressionBuilders;

namespace CommandLine.AutoParser.UltraMapper.Extensions
{
    public class UltraMapperBinding : IMapper
    {
        private readonly Mapper _mapper = new Mapper();

        public void Initialize( IHelpProvider helpProvider )
        {
            int index = _mapper.MappingConfiguration.Mappers.FindIndex( m => m is ReferenceMapper );

            _mapper.MappingConfiguration.Mappers.InsertRange( index, new IMappingExpressionBuilder[]
            {
                new ParsedCommandsExpressionBuilder( _mapper.MappingConfiguration ),
                new ParsedCommandExpressionBuilder( _mapper.MappingConfiguration, helpProvider ),
                new ArrayParamExpressionBuilder( _mapper.MappingConfiguration ),
                new ComplexParamExpressionBuilder( _mapper.MappingConfiguration ),
                new SimpleParamExpressionBuilder( _mapper.MappingConfiguration )
            } );
        }

        public T Map<T>( IEnumerable<ParsedCommand> commands ) where T : class, new()
        {
            return _mapper.Map<T>( commands );
        }
    }
}
