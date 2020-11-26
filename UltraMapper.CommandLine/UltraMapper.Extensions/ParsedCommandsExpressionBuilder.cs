using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.CommandLine.Extensions
{
    public class ParsedCommandsExpressionBuilder : CollectionToNonCollectionExpressionBuilder
    {
        public ParsedCommandsExpressionBuilder( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return typeof( IEnumerable<ParsedCommand> ).IsAssignableFrom( source ) &&
                !typeof( IEnumerable<ParsedCommand> ).IsAssignableFrom( target ) &&
                !target.IsArray && !target.IsEnumerable();   //allow cloning
        }

        public override Type GetCollectionElementType()
        {
            return typeof( ParsedCommand );
        }

        public override LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options )
        {
            var context = (CollectionMapperContext)this.GetMapperContext( source, target, options );
            var inner = MapperConfiguration[ typeof( ParsedCommand ), target ].MappingExpression;

            var body = Expression.Block
            (
                ExpressionLoops.ForEach( context.SourceInstance, context.SourceCollectionLoopingVar,
                    Expression.Invoke( inner, context.ReferenceTracker, 
                        context.SourceCollectionLoopingVar, context.TargetInstance ), 
                        context.Break, context.Continue )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                 context.ReferenceTracker.Type, context.SourceInstance.Type,
                 context.TargetInstance.Type );

            return Expression.Lambda( delegateType, body,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }

        protected override ReferenceMapperContext GetMapperContext( Type source, Type target, IMappingOptions options )
        {
            return new CollectionMapperContext( source, target, options );
        }
    }
}
