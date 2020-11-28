using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;

namespace UltraMapper.CommandLine.Extensions
{
    public class ComplexParamExpressionBuilder : ReferenceMapper
    {
        public ComplexParamExpressionBuilder( Configuration configuration )
            : base( configuration ) { }

        public override bool CanHandle( Type source, Type target )
        {
            return source == typeof( ComplexParam ) &&
                target != typeof( ComplexParam ); //allow cloning
        }

        public override LambdaExpression GetMappingExpression( Type source, Type target, IMappingOptions options )
        {
            //IGNORING FIELDS ON PURPOSE SINCE SUPPORTING FIELDS WILL FORCE 
            //THE USER TO ALWAYS SET OPTIONATTRIBUTE.ORDER IN ORDER TO MAP BY INDEX RELIABLY
            //(REFLECTION DO NOT GUARANTEE PROPERTIES AND FIELDS TO BE RETURNED IN DECLARED ORDER
            //IN PARTICULAR IF FIELDS AND PROPERTIES DECLARATION ORDER IS MIXED 
            //(eg: property, field, property, field...)

            //if( target.IsValueType && !target.IsNullable() )
            //    throw new ArgumentException( $"Value types are not supported. {target.GetPrettifiedName()} is a value type." );

            var context = this.GetMapperContext( source, target, options );

            var targetMembers = target.GetProperties() //methods only supported at top level (in ParsedCommand)
                .Where( m => m.GetSetMethod() != null ) //must be assignable
                .Where( m => m.GetIndexParameters().Length == 0 )//indexer not supported
                .Select( m => new
                {
                    Member = m,
                    Options = m.GetCustomAttribute<OptionAttribute>() ?? new OptionAttribute()
                } )
                .Where( m => !m.Options.IsIgnored )
                .OrderByDescending( info => info.Options.IsRequired )
                .ThenBy( info => info.Options.Order ) //once sorted, we will actually use the index
                .Select( m => m.Member )
                .ToArray();

            var subParam = Expression.Parameter( typeof( IParsedParam ), "paramLoopVar" );
            var subParamsAccess = Expression.Property( context.SourceInstance, nameof( ComplexParam.SubParams ) );

            var propertiesAssigns = MemberExpressionBuilder.GetMemberAssignments( context,
                targetMembers, subParam, MapperConfiguration );

            var expression = !propertiesAssigns.Any() ? (Expression)Expression.Empty() : Expression.Block
            (
                new[] { subParam },
                
                ExpressionLoops.ForEach( subParamsAccess, subParam, 
                    Expression.Block( propertiesAssigns ) )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                 context.ReferenceTracker.Type, context.SourceInstance.Type,
                 context.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }
    }
}
