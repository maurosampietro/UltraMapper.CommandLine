﻿using UltraMapper.CommandLine.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.MappingExpressionBuilders.MapperContexts;

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

            var propertiesAssigns = TempHelper.GetMemberAssignments( context,
                targetMembers, subParam, MapperConfiguration );

            var expression = Expression.Block
            (
                new[] { subParam },

                ExpressionLoops.ForEach( subParamsAccess, subParam, Expression.Block( propertiesAssigns ) )
            );

            var delegateType = typeof( Action<,,> ).MakeGenericType(
                 context.ReferenceTracker.Type, context.SourceInstance.Type,
                 context.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }
    }

    public class TempHelper
    {
        public static IEnumerable<Expression> GetMemberAssignments( ReferenceMapperContext context,
            MemberInfo[] targetMembers, ParameterExpression subParam, Configuration MapperConfiguration )
        {
            for( int i = 0; i < targetMembers.Length; i++ )
            {
                var memberInfo = targetMembers[ i ];
                var assignment = GetMemberAssignment( context, subParam, memberInfo, MapperConfiguration );

                var nameToLowerCase = Expression.Call( Expression.Property( subParam,
                    nameof( IParsedParam.Name ) ), nameof( String.ToLower ), null, null );

                var optionAttribute = memberInfo.GetCustomAttribute<OptionAttribute>();
                string memberName = String.IsNullOrWhiteSpace( optionAttribute?.Name ) ?
                    memberInfo.Name : optionAttribute.Name;

                if( context.SourceInstance.Type == typeof( ParsedCommand ) )
                {
                    if( memberInfo is MethodInfo mi )
                    {
                        //method calls only available in ParsedCommand where calling by name is mandatory

                        var methodParams = mi.GetParameters();
                        if( methodParams.Length == 1 && methodParams[ 0 ].ParameterType == typeof( bool ) )
                        {
                            var subParamsAccess = Expression.Property( context.SourceInstance, nameof( ParsedCommand.Param ) );
                            yield return Expression.IfThen
                            (
                                Expression.Equal( Expression.Constant( memberName.ToLower() ),
                                    Expression.Call( Expression.Property( context.SourceInstance,
                                    nameof( ParsedCommand.Name ) ), nameof( String.ToLower ), null, null ) ),

                                Expression.IfThenElse
                                (
                                    Expression.Equal( subParamsAccess, Expression.Constant( null, typeof( IParsedParam ) ) ),
                                    Expression.Call( context.TargetInstance, mi, Expression.Constant( true ) ),
                                    assignment
                                )
                            );
                        }
                        else
                        {
                            yield return Expression.IfThen
                            (
                                Expression.Equal( Expression.Constant( memberName.ToLower() ),
                                    Expression.Call( Expression.Property( context.SourceInstance,
                                    nameof( ParsedCommand.Name ) ), nameof( String.ToLower ), null, null ) ),

                                assignment
                            );
                        }
                    }
                    else if( memberInfo is PropertyInfo pi )
                    {
                        if( pi.PropertyType == typeof( bool ) )
                        {
                            var subParamsAccess = Expression.Property( context.SourceInstance, nameof( ParsedCommand.Param ) );
                            yield return Expression.IfThen
                            (
                                Expression.Equal( Expression.Constant( memberName.ToLower() ),
                                    Expression.Call( Expression.Property( context.SourceInstance,
                                    nameof( ParsedCommand.Name ) ), nameof( String.ToLower ), null, null ) ),

                                Expression.IfThenElse
                                (
                                    Expression.Equal( subParamsAccess, Expression.Constant( null, typeof( IParsedParam ) ) ),
                                    Expression.Assign( Expression.Property( context.TargetInstance, memberInfo.Name ), Expression.Constant( true ) ),
                                    assignment
                                )
                            );
                        }
                        else
                        {
                            yield return Expression.IfThen
                            (
                                Expression.Equal( Expression.Constant( memberName.ToLower() ),
                                    Expression.Call( Expression.Property( context.SourceInstance,
                                    nameof( ParsedCommand.Name ) ), nameof( String.ToLower ), null, null ) ),

                                assignment
                            );
                        }
                    }
                }
                else
                {
                    yield return Expression.IfThen
                    (
                        Expression.OrElse
                        (
                            Expression.Equal( Expression.Constant( memberName.ToLower() ), nameToLowerCase ),

                            Expression.AndAlso
                            (
                                Expression.Equal( nameToLowerCase, Expression.Constant( String.Empty ) ),
                                Expression.Equal( Expression.Constant( i ),
                                    Expression.Property( subParam, nameof( IParsedParam.Index ) ) )
                            )
                        ),

                        assignment
                    );

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
        }

        private static Expression GetMemberAssignment( ReferenceMapperContext context, ParameterExpression subParam,
            MemberInfo memberInfo, Configuration MapperConfiguration )
        {
            if( memberInfo is PropertyInfo propertyInfo )
            {
                var setter = memberInfo.GetSetterLambdaExpression();

                if( propertyInfo.PropertyType.IsBuiltIn( false ) )
                {
                    var conversion = MapperConfiguration[ typeof( SimpleParam ), propertyInfo.PropertyType ].MappingExpression;

                    var exceptionParam = Expression.Parameter( typeof( Exception ), "exception" );
                    var ctor = typeof( ArgumentException )
                        .GetConstructor( new Type[] { typeof( string ), typeof( Exception ) } );

                    string error = $"Value not assignable to param '{memberInfo.Name.ToLower()}'";// in command '{paramDef.Name}'";

                    var standardBuiltInTypeExp = Expression.TryCatch
                    (
                        Expression.Invoke( setter, context.TargetInstance,
                            Expression.Invoke( conversion, Expression.Convert( subParam, typeof( SimpleParam ) ) ) ),

                        Expression.Catch( exceptionParam, Expression.Throw
                        (
                            Expression.New( ctor, Expression.Constant( error ), exceptionParam ),
                            typeof( void )
                        ) )
                    );

                    return standardBuiltInTypeExp;
                }
                else if( propertyInfo.PropertyType.IsEnumerable() )
                {
                    var mapping = MapperConfiguration[ typeof( ArrayParam ),
                        propertyInfo.PropertyType ];

                    var mappingExpression = mapping.MappingExpression;

                    var targetProperty = Expression.Property( context.TargetInstance, memberInfo.Name );
                    //var mm = new MemberMapping( mapping, , new MappingTarget( setter ) );

                    //var memberMappingContext = new MemberMappingContext( mm );
                    //var collectionAssignment = new CollectionMapper( MapperConfiguration ).GetMemberAssignment( memberMappingContext );
                    var targetType = targetProperty.Type;
                    if( targetProperty.Type.IsInterface || targetProperty.Type.IsAbstract )
                        targetType = typeof( List<> ).MakeGenericType( targetType.GetGenericArguments() );

                    if( targetProperty.Type.IsArray )
                    {
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
                            var constructorWithCapacity = targetProperty.Type.GetConstructor( new Type[] { typeof( int ) } );
                            if( constructorWithCapacity == null ) return null;

                            var itemsProperty = Expression.Property( Expression.Convert( subParam,
                                typeof( ArrayParam ) ), nameof( ArrayParam.Items ) );

                            var getCountMethod = Expression.Call( null, getCount, itemsProperty );
                            return Expression.New( constructorWithCapacity, getCountMethod );
                        }

                        return Expression.Block
                        (
                            Expression.IfThen( Expression.Equal( targetProperty, Expression.Constant( null, targetProperty.Type ) ),
                                Expression.Assign( targetProperty, GetNewInstanceWithReservedCapacity() ) ),

                            Expression.Invoke( mappingExpression, context.ReferenceTracker,
                                Expression.Convert( subParam, typeof( ArrayParam ) ), targetProperty )
                        );
                    }
                    else
                    {
                        return Expression.Block
                        (
                            Expression.IfThen( Expression.Equal( targetProperty, Expression.Constant( null, targetProperty.Type ) ),
                                Expression.Assign( targetProperty, Expression.New( targetType ) ) ),

                            Expression.Invoke( mappingExpression, context.ReferenceTracker,
                                Expression.Convert( subParam, typeof( ArrayParam ) ), targetProperty )
                        );
                    }
                }
                else
                {
                    var mappingExpression = MapperConfiguration[ typeof( ComplexParam ),
                        propertyInfo.PropertyType ].MappingExpression;

                    var targetProperty = Expression.Property( context.TargetInstance, memberInfo.Name );

                    return Expression.Block
                    (
                        Expression.IfThen( Expression.Equal( targetProperty, Expression.Constant( null, targetProperty.Type ) ),
                            Expression.Assign( targetProperty, Expression.New( targetProperty.Type ) ) ),

                        Expression.Invoke( mappingExpression, context.ReferenceTracker,
                            Expression.Convert( subParam, typeof( ComplexParam ) ), targetProperty )
                    );
                }
            }
            else if( memberInfo is MethodInfo methodInfo )
            {
                var parametersExps = new List<Expression>();

                var methodParams = methodInfo.GetParameters();

                ////temporarily skip methods with array params
                //if( methodParams.Any( p => p.ParameterType.IsArray ) )
                //    return (Expression)Expression.Empty();

                //if( methodParams.Any( p => p.ParameterType == typeof( object ) ) )
                //    return (Expression)Expression.Empty();

                Expression<Func<IParsedParam[], ParameterInfo, object>> selectParamExp =
                    ( parsedparams, methodparam ) => SelectParam( parsedparams, methodparam );

                for( int i = 0; i < methodParams.Length; i++ )
                {
                    var param = methodParams[ i ];

                    var paramType = param.ParameterType.IsBuiltIn( false ) ?
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
                                
                                Expression.Assign(selectedParam, Expression.Convert
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