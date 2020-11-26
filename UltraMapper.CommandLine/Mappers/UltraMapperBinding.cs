using UltraMapper.CommandLine.Mappers;
using UltraMapper.CommandLine.Parsers;
using System.Collections.Generic;
using UltraMapper;
using UltraMapper.MappingExpressionBuilders;
using System;

namespace UltraMapper.CommandLine.Extensions
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

            //TODO
            //_mapper.MappingConfiguration.MapTypes<string, bool>( n => IntToBoolConverter( n ) );
        }

        //private bool IntToBoolConverter( string n )
        //{
        //    if( n == "0" ) return false;
        //    if( n == "1" ) return true;

        //    return (bool)(object)n;//will throw exception;
        //}

        public T Map<T>( IEnumerable<ParsedCommand> commands ) where T : class, new()
        {
            return _mapper.Map<T>( commands );
        }

        public T Map<T>( IEnumerable<ParsedCommand> commands, T instance ) where T : class
        {
            _mapper.Map( commands, instance );
            return instance;
        }
    }
}
