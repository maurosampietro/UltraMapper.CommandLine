using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.MappingExpressionBuilders.MapperContexts;
using UltraMapper.ReferenceTracking;

namespace UltraMapper.CommandLine.Extensions
{
    public class ParsedCommandMemberExpressionBuilder : ReferenceMapper
    {
        public ParsedCommandMemberExpressionBuilder( Configuration configuration )
            : base( configuration ) { }

        public IEnumerable<Expression> GetMemberAssignments( ReferenceMapperContext context,
            MemberInfo[] targetMembers, ParameterExpression subParam, Configuration MapperConfiguration )
        {
            for( int i = 0; i < targetMembers.Length; i++ )
            {
                var memberInfo = targetMembers[ i ];
                var assignment = GetMemberAssignment( context, subParam, memberInfo, MapperConfiguration );

                var optionAttribute = memberInfo.GetCustomAttribute<OptionAttribute>();
                string memberNameLowerCase = String.IsNullOrWhiteSpace( optionAttribute?.Name ) ?
                    memberInfo.Name.ToLower() : optionAttribute.Name.ToLower();

                Expression implicitBoolAssignment = null;

                if( memberInfo is MethodInfo mi )
                {
                    //method calls only available in ParsedCommand where calling by name is mandatory
                    var methodParams = mi.GetParameters();
                    if( methodParams.Length == 1 && methodParams[ 0 ].ParameterType == typeof( bool ) )
                    {
                        implicitBoolAssignment = Expression.Call( context.TargetInstance, mi, Expression.Constant( true ) );
                    }
                }
                else if( memberInfo is PropertyInfo pi )
                {
                    if( pi.PropertyType == typeof( bool ) )
                    {
                        implicitBoolAssignment = Expression.Assign( Expression.Property(
                            context.TargetInstance, memberInfo.Name ), Expression.Constant( true ) );
                    }
                }

                if( implicitBoolAssignment != null )
                {
                    var subParamsAccess = Expression.Property( context.SourceInstance, nameof( ParsedCommand.Param ) );
                    assignment = Expression.IfThenElse
                    (
                        Expression.Equal( subParamsAccess, Expression.Constant( null, typeof( IParsedParam ) ) ),
                        implicitBoolAssignment,
                        assignment
                    );
                }

                var commandNameExp = Expression.Property( context.SourceInstance, nameof( ParsedCommand.Name ) );
                var commandNameToLowerExp = Expression.Call( commandNameExp, nameof( String.ToLower ), null, null );

                yield return Expression.IfThen
                (
                    //we only check command name
                    Expression.Equal( Expression.Constant( memberNameLowerCase ), commandNameToLowerExp ),
                    assignment
                );
            }
        }

        private Expression GetMemberAssignment( ReferenceMapperContext context, ParameterExpression subParam,
            MemberInfo memberInfo, Configuration MapperConfiguration )
        {
            if( memberInfo is PropertyInfo propertyInfo )
            {
                if( propertyInfo.PropertyType.IsBuiltIn( true ) )
                {
                    var conversion = MapperConfiguration[ typeof( SimpleParam ),
                        propertyInfo.PropertyType ].MappingExpression;

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
                    Expression sourcemappingtype = null;
                    if( context.SourceInstance.Type == typeof( ParsedCommand ) )
                        sourcemappingtype = (Expression<Func<ParsedCommand, IParsedParam>>)(( ParsedCommand p ) => p.Param);
                    else
                        sourcemappingtype = (Expression<Func<ComplexParam, IParsedParam[]>>)(( ComplexParam p ) => p.SubParams);

                    if( propertyInfo.PropertyType.IsArray )
                    {
                        sourcemappingtype = (Expression<Func<ArrayParam, IReadOnlyList<IParsedParam>>>)(( ArrayParam p ) => p.Items);

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
                        .ReplaceParameter( sourceToReplace, "sourceValue" )
                        .ReplaceParameter( targetProperty, "targetValue" )
                        .ReplaceParameter( context.TargetInstance, "instance" );

                    if( MapperConfiguration.IsReferenceTrackingEnabled )
                    {
                        var mainExp = ReferenceTrackingExpression.GetMappingExpression(
                        context.ReferenceTracker,
                        subParam, targetProperty,
                        memberAssignment, context.Mapper, _mapper,
                        Expression.Constant( null, typeof( IMapping ) ) );

                        return mainExp;
                    }
                    else
                    {
                        var mapMethod = ReferenceMapperContext.RecursiveMapMethodInfo
                            .MakeGenericMethod( subParam.Type, targetProperty.Type );

                        return Expression.Block
                        (
                            new[] { context.Mapper },

                            Expression.Assign( context.Mapper, Expression.Constant( _mapper ) ),

                            memberAssignment,

                            Expression.Call( context.Mapper, mapMethod, subParam,
                                targetProperty, context.ReferenceTracker, Expression.Constant( null, typeof( IMapping ) ) )
                        );
                    }
                }
            }
            else if( memberInfo is MethodInfo methodInfo )
            {
                var parametersExps = new List<Expression>();
                var methodParams = methodInfo.GetParameters();

                Expression<Func<IParsedParam[], ParameterInfo, object>> selectParamExp =
                    ( parsedparams, methodparam ) => SelectParam( parsedparams, methodparam );

                for( int i = 0; i < methodParams.Length; i++ )
                {
                    var param = methodParams[ i ];

                    var paramType = param.ParameterType.IsBuiltIn( true ) ?
                        typeof( SimpleParam ) : typeof( ComplexParam );

                    if( !param.ParameterType.IsBuiltIn( false ) &&
                        param.ParameterType.IsEnumerable() )
                    {
                        paramType = typeof( ArrayParam );
                    }

                    var itemMapping = MapperConfiguration[ paramType, param.ParameterType ].MappingExpression;

                    var convertExp = (Expression)Expression.NewArrayInit( typeof( IParsedParam ),
                        Expression.Convert(
                            Expression.Property( context.SourceInstance, nameof( ParsedCommand.Param ) ),
                        paramType ) );

                    if( methodParams.Length > 1 )
                    {
                        convertExp = Expression.Property
                        (
                            Expression.Convert
                            (
                                Expression.Property( context.SourceInstance, nameof( ParsedCommand.Param ) ),
                                typeof( ComplexParam )
                            ),
                            nameof( ComplexParam.SubParams )
                        );
                    }

                    if( paramType == typeof( SimpleParam ) )
                    {
                        var paramExp = Expression.Invoke
                        (
                            itemMapping,
                            Expression.Convert
                            (
                                Expression.Invoke( selectParamExp, convertExp, Expression.Constant( param ) ),
                                paramType
                            )
                        );

                        parametersExps.Add( paramExp );
                    }
                    else
                    {
                        if( param.ParameterType.IsArray )
                        {
                            var tempTarget = Expression.Parameter( param.ParameterType, "temptarget" );
                            var selectedParam = Expression.Parameter( paramType, "selectedParam" );

                            var getCount = typeof( System.Linq.Enumerable ).GetMethods(
                                BindingFlags.Static | BindingFlags.Public )
                            .First( m =>
                            {
                                if( m.Name != nameof( System.Linq.Enumerable.Count ) )
                                    return false;

                                var parameters = m.GetParameters();
                                if( parameters.Length != 1 ) return false;

                                return parameters[ 0 ].ParameterType.GetGenericTypeDefinition() == typeof( IEnumerable<> );
                            } )
                            .MakeGenericMethod( typeof( IParsedParam ) );

                            Expression GetNewInstanceWithReservedCapacity()
                            {
                                var constructorWithCapacity = param.ParameterType.GetConstructor( new Type[] { typeof( int ) } );
                                if( constructorWithCapacity == null ) return null;

                                var itemsProperty = Expression.Property( Expression.Convert( selectedParam,
                                    typeof( ArrayParam ) ), nameof( ArrayParam.Items ) );

                                var getCountMethod = Expression.Call( null, getCount, itemsProperty );
                                return Expression.New( constructorWithCapacity, getCountMethod );
                            }

                            var paramExp = Expression.Block
                            (
                                new[] { tempTarget, selectedParam },

                                Expression.Assign( selectedParam, Expression.Convert
                                (
                                    Expression.Invoke( selectParamExp, convertExp, Expression.Constant( param ) ),
                                    paramType
                                ) ),

                                Expression.Assign( tempTarget, GetNewInstanceWithReservedCapacity() ),

                                Expression.Invoke
                                (
                                    itemMapping,
                                    context.ReferenceTracker,
                                    selectedParam,
                                    tempTarget
                                ),

                                tempTarget
                            );

                            parametersExps.Add( paramExp );
                        }
                        else
                        {
                            var targetParamType = param.ParameterType;
                            if( param.ParameterType.IsInterface || param.ParameterType.IsAbstract )
                                targetParamType = typeof( List<> ).MakeGenericType( targetParamType.GetGenericArguments() );

                            var tempTarget = Expression.Parameter( param.ParameterType, "temptarget" );

                            var paramExp = Expression.Block
                            (
                                new[] { tempTarget },
                                Expression.Assign( tempTarget, Expression.New( targetParamType ) ),

                                Expression.Invoke
                                (
                                    itemMapping,
                                    context.ReferenceTracker,
                                    Expression.Convert
                                    (
                                        Expression.Invoke( selectParamExp, convertExp, Expression.Constant( param ) ),
                                        paramType
                                    ),
                                    tempTarget
                                ),

                                tempTarget
                            );

                            parametersExps.Add( paramExp );
                        }
                    }
                }

                return Expression.Block
                (
                    Expression.Call( context.TargetInstance, methodInfo, parametersExps.ToArray() )
                );
            }

            throw new NotSupportedException();
        }

        private static object SelectParam( IParsedParam[] parsedparams, ParameterInfo methodparam )
        {
            var paramOptions = methodparam.GetCustomAttribute<OptionAttribute>();

            string paramName = paramOptions?.Name?.ToLower();
            if( String.IsNullOrWhiteSpace( paramName ) )
                paramName = methodparam.Name.ToLower();

            var namedParam = parsedparams.FirstOrDefault( p => p.Name.ToLower() == paramName );
            if( namedParam != null ) return namedParam;

            int paramOrder = paramOptions?.Order ?? -1;
            if( paramOrder == -1 )
                paramOrder = methodparam.Position;

            return parsedparams.FirstOrDefault( p => p.Index == paramOrder );
        }
    }
}
