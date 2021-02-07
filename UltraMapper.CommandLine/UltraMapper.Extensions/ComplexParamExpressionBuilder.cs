using System;
using System.Collections.Generic;
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
        public bool CanMapByIndex { get; set; }

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
                .Select( ( m, index ) => new
                {
                    Member = m,
                    Options = m.GetCustomAttribute<OptionAttribute>() ??
                        new OptionAttribute() {/*Order = index*/ }
                } )
                .Where( m => !m.Options.IsIgnored )
                .OrderByDescending( info => info.Options.IsRequired )
                .ThenBy( info => info.Options.Order )
                .Select( m => m.Member )
                .ToArray();

            var subParam = Expression.Parameter( typeof( IParsedParam ), "paramLoopVar" );
            var subParamsAccess = Expression.Property( context.SourceInstance, nameof( ComplexParam.SubParams ) );

            var paramNameLowerCase = Expression.Parameter( typeof( string ), "paramName" );
            var paramNameExp = Expression.Property( subParam, nameof( IParsedParam.Name ) );
            var paramNameToLower = Expression.Call( paramNameExp, nameof( String.ToLower ), null, null );

            var propertiesAssigns = new ComplexParamMemberExpressionBuilder( _mapper.MappingConfiguration ) { CanMapByIndex = CanMapByIndex }
                .GetMemberAssignments( context, targetMembers, subParam, MapperConfiguration, paramNameLowerCase ).ToArray();

            Expression paramNameDispatch = null;
            if( CanMapByIndex )
            {
                var paramNameIfElseChain = GetIfElseChain( propertiesAssigns );
                paramNameDispatch = paramNameIfElseChain;
            }else
            {
                var paramNameSwitch = GetSwitch( propertiesAssigns, paramNameLowerCase );
                paramNameDispatch = paramNameSwitch;
            }

            var expression = !propertiesAssigns.Any() ? (Expression)Expression.Empty() : Expression.Block
            (
                new[] { subParam, paramNameLowerCase },

                ExpressionLoops.ForEach( subParamsAccess, subParam, Expression.Block
                (
                    Expression.Assign( paramNameLowerCase, paramNameToLower ),
                    paramNameDispatch
                ) )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                 context.ReferenceTracker.Type, context.SourceInstance.Type,
                 context.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }

        private Expression GetSwitch( Expression[] propertiesAssigns, ParameterExpression paramNameLowerCase )
        {
            IEnumerable<SwitchCase> getSwitchCases()
            {
                foreach( ConditionalExpression item in propertiesAssigns )
                    yield return Expression.SwitchCase( item.IfTrue, ((BinaryExpression)item.Test).Left );
            }

            var switches = getSwitchCases().ToArray();
            return Expression.Switch( paramNameLowerCase, switches );
        }

        private ConditionalExpression GetIfElseChain( Expression[] propertiesAssigns, int i = 0 )
        {
            var item = (ConditionalExpression)propertiesAssigns[ i ];

            if( i == propertiesAssigns.Length - 1 )
                return item;

            return Expression.IfThenElse( item.Test, item.IfTrue,
                GetIfElseChain( propertiesAssigns, i + 1 ) );
        }
    }
}
