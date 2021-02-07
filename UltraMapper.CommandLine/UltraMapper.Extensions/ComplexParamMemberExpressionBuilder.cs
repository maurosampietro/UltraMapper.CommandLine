using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.MappingExpressionBuilders.MapperContexts;
using UltraMapper.ReferenceTracking;

namespace UltraMapper.CommandLine.Extensions
{
    public class ComplexParamMemberExpressionBuilder : ReferenceMapper
    {
        public bool CanMapByIndex { get; set; }

        public ComplexParamMemberExpressionBuilder( Configuration configuration )
            : base( configuration ) { }

        public IEnumerable<Expression> GetMemberAssignments( ReferenceMapperContext context,
            MemberInfo[] targetMembers, ParameterExpression subParam,
            Configuration MapperConfiguration, ParameterExpression paramNameLowerCase )
        {
            for( int i = 0; i < targetMembers.Length; i++ )
            {
                var memberInfo = targetMembers[ i ];
                var assignment = GetMemberAssignment( context, subParam, memberInfo, MapperConfiguration );

                var optionAttribute = memberInfo.GetCustomAttribute<OptionAttribute>();
                string memberNameLowerCase = String.IsNullOrWhiteSpace( optionAttribute?.Name ) ?
                    memberInfo.Name.ToLower() : optionAttribute.Name.ToLower();

                if( this.CanMapByIndex )
                {
                    yield return Expression.IfThen
                    (
                        Expression.OrElse
                        (
                            //we check param name and index
                            Expression.Equal( Expression.Constant( memberNameLowerCase ), paramNameLowerCase ),

                            Expression.AndAlso
                            (
                                Expression.Equal( paramNameLowerCase, Expression.Constant( String.Empty ) ),
                                Expression.Equal( Expression.Constant( i ),
                                    Expression.Property( subParam, nameof( IParsedParam.Index ) ) )
                            )
                        ),

                        assignment
                    );
                }
                else //can map only by name
                {
                    yield return Expression.IfThen
                    (
                        //we check param name and index
                        Expression.Equal( Expression.Constant( memberNameLowerCase ), paramNameLowerCase ),
                        assignment
                    );
                }

                //if( memberInfo is PropertyInfo pi && pi.PropertyType == typeof( bool ) && implicitbool )
                //{
                //    var subParamsAccess = Expression.Property( context.SourceInstance, nameof( ParsedCommand.Param ) );
                //    var setter = memberInfo.GetSetterLambdaExpression();

                //    yield return Expression.IfThenElse
                //    (
                //        Expression.Equal( subParamsAccess, Expression.Constant( null, typeof( IParsedParam ) ) ),
                //        Expression.Invoke( setter, context.TargetInstance, Expression.Constant( true ) ),
                //        standardChecks
                //    );
                //}                    
            }
        }

        private Expression GetMemberAssignment( ReferenceMapperContext context, ParameterExpression subParam,
            MemberInfo memberInfo, Configuration MapperConfiguration )
        {
            var propertyInfo = (PropertyInfo)memberInfo;

            if( propertyInfo.PropertyType.IsBuiltIn( true ) )
            {
                var conversion = MapperConfiguration[ typeof( SimpleParam ),
                    propertyInfo.PropertyType ].MappingExpression;



                //var targetsetprop = context.TargetInstance.Type.GetProperty( memberInfo.Name );

                //var mappingSource = new MappingSource( typeof( ParsedCommand ).GetProperty( nameof( ParsedCommand.Param ) ) );
                //var mappingTarget = new MappingTarget( targetsetprop );

                //var typePair = new TypePair( propertyInfo.PropertyType, targetsetprop.PropertyType );
                //var typeMapping = new TypeMapping( MapperConfiguration, typePair );
                //var membermapping = new MemberMapping( typeMapping, mappingSource, mappingTarget );
                //var membermappingcontext = new MemberMappingContext( membermapping );

                //var exp = base.GetSimpleMemberExpression( membermapping );
                //return exp;

                //var exceptionParam = Expression.Parameter( typeof( Exception ), "exception" );
                //var ctor = typeof( ArgumentException )
                //    .GetConstructor( new Type[] { typeof( string ), typeof( Exception ) } );

                //string error = $"Value not assignable to param '{memberInfo.Name.ToLower()}'";// in command '{paramDef.Name}'";

                //var standardBuiltInTypeExp = Expression.TryCatch
                //(
                //    Expression.Invoke( setter, context.TargetInstance,
                //        Expression.Invoke( conversion, Expression.Convert( subParam, typeof( SimpleParam ) ) ) ),

                //    Expression.Catch( exceptionParam, Expression.Throw
                //    (
                //        Expression.New( ctor, Expression.Constant( error ), exceptionParam ),
                //        typeof( void )
                //    ) )
                //);

                //return Expression.Invoke( setter, context.TargetInstance,
                //    Expression.Invoke( conversion, Expression.Convert( subParam, typeof( SimpleParam ) ) ) );

                var setter = memberInfo.GetSetterLambdaExpression();

                return base.GetSimpleMemberExpressionInternal( conversion,
                    context.TargetInstance, Expression.Convert( subParam, typeof( SimpleParam ) ), setter );
            }
            else
            {
                Expression sourceToReplace = subParam;
                PropertyInfo sourcemappingtype = null;
                Expression sourceValueExp = null;
                if( context.SourceInstance.Type == typeof( ParsedCommand ) )
                {
                    sourcemappingtype = typeof( ParsedCommand ).GetProperty( nameof( ParsedCommand.Param ) );
                    sourceValueExp = Expression.Property( Expression.Convert( subParam, typeof( ParsedCommand ) ), nameof( ParsedCommand.Param ) );
                }
                else
                {
                    sourcemappingtype = typeof( ComplexParam ).GetProperty( nameof( ComplexParam.SubParams ) );
                    sourceValueExp = Expression.Property( Expression.Convert( subParam, typeof( ComplexParam ) ), nameof( ComplexParam.SubParams ) );
                }

                if( propertyInfo.PropertyType.IsEnumerable() )
                {
                    sourcemappingtype = typeof( ArrayParam ).GetProperty( nameof( ArrayParam.Items ) );
                    sourceValueExp = Expression.Property( Expression.Convert( subParam, typeof( ArrayParam ) ), nameof( ArrayParam.Items ) );

                    var itemsProperty = Expression.Property( Expression.Convert( subParam,
                        typeof( ArrayParam ) ), nameof( ArrayParam.Items ) );
                    sourceToReplace = itemsProperty;
                }

                var targetsetprop = context.TargetInstance.Type.GetProperty( memberInfo.Name );
                var mappingSource = new MappingSource( sourcemappingtype );
                var mappingTarget = new MappingTarget( targetsetprop );

                var typePair = new TypePair( propertyInfo.PropertyType, targetsetprop.PropertyType );
                var typeMapping = new TypeMapping( MapperConfiguration, typePair );
                var membermapping = new MemberMapping( typeMapping, mappingSource, mappingTarget );
                var membermappingcontext = new MemberMappingContext( membermapping );

                var targetProperty = Expression.Property( context.TargetInstance, memberInfo.Name );

                var targetType = targetProperty.Type;
                if( targetProperty.Type.IsInterface || targetProperty.Type.IsAbstract )
                    targetType = typeof( List<> ).MakeGenericType( targetType.GetGenericArguments() );

                var mapping = MapperConfiguration[ targetType, targetsetprop.PropertyType ];

                var memberAssignment = ((ReferenceMapper)mapping.Mapper).GetMemberAssignment( membermappingcontext )
                    //.ReplaceParameter( sourceToReplace, "sourceValue" )
                    .ReplaceParameter( membermappingcontext.SourceMember, "sourceValue" )
                    .ReplaceParameter( targetProperty, "targetValue" )
                    .ReplaceParameter( context.TargetInstance, "instance" );

                if( MapperConfiguration.IsReferenceTrackingEnabled )
                {
                    return ReferenceTrackingExpression.GetMappingExpression(
                        context.ReferenceTracker,
                        subParam, targetProperty,
                        memberAssignment, context.Mapper, _mapper,
                        Expression.Constant( null, typeof( IMapping ) ) );
                }
                else
                {
                    var mapMethod = ReferenceMapperContext.RecursiveMapMethodInfo
                        .MakeGenericMethod( subParam.Type, targetProperty.Type );

                    return Expression.Block
                    (
                        new[] { membermappingcontext.SourceMember, context.Mapper },

                        Expression.Assign( context.Mapper, Expression.Constant( _mapper ) ),
                        Expression.Assign( membermappingcontext.SourceMember, sourceValueExp ),

                        memberAssignment,

                        Expression.Call( context.Mapper, mapMethod, subParam,
                            targetProperty, context.ReferenceTracker, Expression.Constant( null, typeof( IMapping ) ) )
                    );
                }

                ////version better integrated in ultramapper:
                //var main2 = Expression.Block
                //(
                //    new[] { context.Mapper },
                //    Expression.Assign( context.Mapper, Expression.Constant( _mapper ) ),
                //    base.GetComplexMemberExpression( membermapping )
                //        .ReplaceParameter( context.SourceInstance, "instance" )
                //        .ReplaceParameter( context.ReferenceTracker, "referenceTracker" )
                //        .ReplaceParameter( context.Mapper, "mapper" )
                //        .ReplaceParameter( context.TargetInstance, "instance" )
                //        .ReplaceParameter( sourceToReplace, "sourceValue" )//!!!!! il problema è la fonte da cui leggere il parametro
                //);

                //return main2;
            }
        }
    }
}
