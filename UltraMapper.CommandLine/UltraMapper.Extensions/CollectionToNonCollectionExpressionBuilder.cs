using System;
using System.Linq.Expressions;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.CommandLine.Extensions
{
//    public abstract class CollectionToNonCollectionExpressionBuilder : ReferenceMapper
//    {
//        public CollectionToNonCollectionExpressionBuilder( Configuration configuration )
//            : base( configuration ) { }

//        public override LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options )
//        {
//            var context = this.GetMapperContext( source, target, options );
//            var sourceCollElementType = this.GetCollectionElementType();

//            var mapMethod = CollectionMapperContext.RecursiveMapMethodInfo
//                .MakeGenericMethod( sourceCollElementType, target );

//            var itemMapping = this.MapperConfiguration[ sourceCollElementType, target ];

//            var loopVar = Expression.Parameter( sourceCollElementType, "loopVar" );
//            var expression = Expression.Block
//            (
//                GetPreLoopExpression( context ),

//                ExpressionLoops.ForEach( context.SourceInstance, loopVar, Expression.Block
//                (
//                    Expression.Call( context.Mapper, mapMethod, loopVar, context.TargetInstance,
//                        context.ReferenceTracker, Expression.Constant( itemMapping ) )
//                ) ),

//                GetPostLoopExpression( context )
//            );

//            var delegateType = typeof( Action<,,> ).MakeGenericType(
//                 context.ReferenceTracker.Type, context.SourceInstance.Type,
//                 context.TargetInstance.Type );

//            return Expression.Lambda( delegateType, expression,
//                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
//        }

//        public virtual Expression GetPreLoopExpression( ReferenceMapperContext context ) => Expression.Empty();
//        public virtual Expression GetPostLoopExpression( ReferenceMapperContext context ) => Expression.Empty();

//        public abstract Type GetCollectionElementType();        
//    }
}
