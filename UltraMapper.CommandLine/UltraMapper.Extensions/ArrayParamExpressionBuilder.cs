using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.CommandLine.Extensions
{
    public class ArrayParamExpressionBuilder : ArrayMapper
    {
        public ArrayParamExpressionBuilder( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return source == typeof( ArrayParam ) &&
                target != typeof( ArrayParam );
        }

        public override LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options )
        {
            var context = (CollectionMapperContext)this.GetMapperContext( source, target, options );
            var items = Expression.Property( context.SourceInstance, nameof( ArrayParam.Items ) );

            Type targetType = target;
            if( target.IsInterface || target.IsAbstract )
                targetType = typeof( List<> ).MakeGenericType( context.TargetCollectionElementType );
            else if( target.IsGenericType )
                targetType = target.GetGenericTypeDefinition().MakeGenericType( context.TargetCollectionElementType );

            var mappingExpression = MapperConfiguration[ typeof( IEnumerable<IParsedParam> ), targetType ].MappingExpression;

            var body = Expression.Invoke( mappingExpression, context.ReferenceTracker, items,
                    Expression.Convert( context.TargetInstance, targetType ) );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                 context.ReferenceTracker.Type, context.SourceInstance.Type,
                 context.TargetInstance.Type );

            return Expression.Lambda( delegateType, body,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }
    }
}
