using System;
using System.Linq.Expressions;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.CommandLine.Extensions
{
    public class IParsedParamRelay : ReferenceMapper
    {
        public IParsedParamRelay( Configuration configuration ) 
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return source == typeof( IParsedParam );
        }

        public override LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options )
        {
            var context = GetMapperContext( source, target, options );

            var typeMapping = MapperConfiguration[ source, target ];
            var mapMethod = ReferenceMapperContext.RecursiveMapMethodInfo
                .MakeGenericMethod( source, target );

            var expression = Expression.Block
            (
                new[] { context.Mapper },

                Expression.Assign( context.Mapper, Expression.Constant( _mapper ) ),

                Expression.Call( context.Mapper, mapMethod,
                    context.SourceInstance, context.TargetInstance,
                    context.ReferenceTracker, Expression.Constant( typeMapping ) )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                context.ReferenceTracker.Type, context.SourceInstance.Type,
                context.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }
    }
}
