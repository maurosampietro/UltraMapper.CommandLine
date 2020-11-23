using CommandLine.AutoParser.Parsers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;

namespace CommandLine.AutoParser.UltraMapper.Extensions
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

    public class ArrayParamExpressionBuilder : ReferenceMapper
    {
        public ArrayParamExpressionBuilder( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return source == typeof( ArrayParam ) &&
                target != typeof( ArrayParam );
        }

        protected override ReferenceMapperContext GetMapperContext( Type source, Type target, IMappingOptions options )
        {
            return new ArrayParamCollectionMapperContext( source, target, options );
        }

        public override LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options )
        {
            var context = (ArrayParamCollectionMapperContext)this.GetMapperContext( source, target, options );
            var items = Expression.Property( context.SourceInstance, nameof( ArrayParam.Items ) );

            Type targetType = target;
            if( target.IsInterface || target.IsAbstract )
                targetType = typeof( List<> ).MakeGenericType( context.TargetCollectionElementType );
            else if( target.IsGenericType )
                targetType = target.GetGenericTypeDefinition().MakeGenericType( context.TargetCollectionElementType );

            var inner = MapperConfiguration[ typeof( IEnumerable<IParsedParam> ), targetType ].MappingExpression;

            var body = Expression.Block
            (
               Expression.Invoke( inner, context.ReferenceTracker, items,
                    Expression.Convert( context.TargetInstance, targetType ) )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                 context.ReferenceTracker.Type, context.SourceInstance.Type,
                 context.TargetInstance.Type );

            return Expression.Lambda( delegateType, body,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }
    }
}
