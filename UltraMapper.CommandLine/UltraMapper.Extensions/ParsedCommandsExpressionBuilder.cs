using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.CommandLine.Extensions
{
    public class ParsedCommandCollectionExpressionBuilder : CollectionMapper
    {
        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source.EntryType;
            var target = mapping.Target.EntryType;

            return typeof( IEnumerable<ParsedCommand> ).IsAssignableFrom( source ) &&
                !typeof( IEnumerable<ParsedCommand> ).IsAssignableFrom( target ) &&
                !target.IsArray && !target.IsEnumerable();   //allow cloning
        }

        public override LambdaExpression GetMappingExpression( Mapping mapping )
        {
            var source = mapping.Source.EntryType;
            var target = mapping.Target.EntryType;

            var context = (CollectionMapperContext)this.GetMapperContext( mapping );
            var mappingExpression = mapping.GlobalConfig[ typeof( ParsedCommand ), target ].MappingExpression;

            var body = Expression.Block(
                ExpressionLoops.ForEach( context.SourceInstance, context.SourceCollectionLoopingVar,
                    Expression.Invoke( mappingExpression, context.ReferenceTracker,
                    context.SourceCollectionLoopingVar, context.TargetInstance ) ),

                Expression.Constant( null, context.TargetInstance.Type )
            );

            var delegateType = typeof( UltraMapperDelegate<,> ).MakeGenericType(
                 context.SourceInstance.Type, context.TargetInstance.Type );

            return Expression.Lambda( delegateType, body,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }
    }
}
