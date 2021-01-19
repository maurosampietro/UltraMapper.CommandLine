using System.Collections.Generic;
using UltraMapper.CommandLine.Mappers;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.MappingExpressionBuilders;

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
                //new IParsedParamRelay( _mapper.MappingConfiguration ),
                new ParsedCommandsExpressionBuilder( _mapper.MappingConfiguration ),
                new ParsedCommandExpressionBuilder( _mapper.MappingConfiguration, helpProvider ),
                new ArrayParamExpressionBuilder( _mapper.MappingConfiguration ),
                new ComplexParamExpressionBuilder( _mapper.MappingConfiguration ),
                new SimpleParamExpressionBuilder( _mapper.MappingConfiguration )
            } );
            
            //try
            //{

            //    _mapper.MappingConfiguration.MapTypes<SimpleParam, object>()
            //        .MapMember( source => source.Value, target => target );
            //}
            //catch( Exception ex)
            //{

                
            //}

            //TODO
            //_mapper.MappingConfiguration.MapTypes<string, bool>( str => IntToBoolConverter( str ) );
        }

        //private bool IntToBoolConverter( string str )
        //{
        //    str = str.Trim();

        //    if( String.Compare( str.Trim(), Boolean.FalseString, true ) == 0 ) return false;
        //    if( String.Compare( str.Trim(), Boolean.TrueString, true ) == 0 ) return true;

        //    if( str == "0" ) return false;
        //    if( str == "1" ) return true;

        //    throw new FormatException();
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

        //disabling reference tracking

        //public T Map<T>( IEnumerable<ParsedCommand> commands ) where T : class, new()
        //{
        //    return this.Map( commands, new T() );
        //}

        //public T Map<T>( IEnumerable<ParsedCommand> commands, T instance ) where T : class
        //{
        //    _mapper.Map( commands, instance, null,
        //        ReferenceBehaviors.CREATE_NEW_INSTANCE, false );

        //    return instance;
        //}
    }
}
