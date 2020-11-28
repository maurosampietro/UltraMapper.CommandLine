using System;
using System.Linq.Expressions;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.CommandLine.Extensions
{
    public class ArrayParamCollectionMapperContext : CollectionMapperContext
    {
        public ArrayParamCollectionMapperContext( Type source, Type target, IMappingOptions options )
         : base( source, target, options )
        {
            SourceCollectionElementType = typeof( IParsedParam );
            SourceCollectionLoopingVar = Expression.Parameter( SourceCollectionElementType, "loopVar" );
            IsSourceElementTypeBuiltIn = false;
        }
    }
}
