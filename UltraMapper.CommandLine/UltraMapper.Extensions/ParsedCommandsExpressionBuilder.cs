using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.CommandLine.Extensions
{
    public class ParsedCommandsExpressionBuilder : CollectionMapper
    {
        public ParsedCommandsExpressionBuilder( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return typeof( IEnumerable<ParsedCommand> ).IsAssignableFrom( source ) &&
                !typeof( IEnumerable<ParsedCommand> ).IsAssignableFrom( target ) &&
                !target.IsArray && !target.IsEnumerable();   //allow cloning
        }

        public override LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options )
        {
            var context = (CollectionMapperContext)this.GetMapperContext( source, target, options );
            var mappingExpression = MapperConfiguration[ typeof( ParsedCommand ), target ].MappingExpression;

            var body = ExpressionLoops.ForEach( context.SourceInstance, context.SourceCollectionLoopingVar,
                Expression.Invoke( mappingExpression, context.ReferenceTracker,
                    context.SourceCollectionLoopingVar, context.TargetInstance ) );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                 context.ReferenceTracker.Type, context.SourceInstance.Type,
                 context.TargetInstance.Type );

            return Expression.Lambda( delegateType, body,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }
    }
}
