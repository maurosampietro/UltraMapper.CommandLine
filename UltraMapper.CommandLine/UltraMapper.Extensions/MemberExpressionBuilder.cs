using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.MappingExpressionBuilders.MapperContexts;

namespace UltraMapper.CommandLine.Extensions
{
    public class MemberExpressionBuilder : ReferenceMapper
    {
        public MemberExpressionBuilder( Configuration configuration )
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

                if( context.SourceInstance.Type == typeof( ParsedCommand ) )
                { 
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
                else
                {
                    var nameToLowerCase = Expression.Call( Expression.Property( subParam,
                        nameof( IParsedParam.Name ) ), nameof( String.ToLower ), null, null );

                    yield return Expression.IfThen
                    (
                        Expression.OrElse
                        (    
                            //we check param name and index
                            Expression.Equal( Expression.Constant( memberNameLowerCase ), nameToLowerCase ),

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
                    var sourcemappingtype = typeof( ParsedCommand ).GetProperty( nameof( ParsedCommand.Param ) );
                    if( propertyInfo.PropertyType.IsArray )
                    {
                        sourcemappingtype = typeof( ArrayParam ).GetProperty( nameof( ArrayParam.Items ) );

                        var itemsProperty = Expression.Property( Expression.Convert( subParam,
                            typeof( ArrayParam ) ), nameof( ArrayParam.Items ) );
                        sourceToReplace = itemsProperty;
                    }

                    var targetsetprop = context.TargetInstance.Type.GetProperty( memberInfo.Name );
                    var mappingSource = new MappingSource( sourcemappingtype );
                    var mappingTarget = new MappingTarget( targetsetprop );

                    var typeMapping = new TypeMapping( MapperConfiguration, new TypePair( propertyInfo.PropertyType, targetsetprop.PropertyType ) );
                    var membermapping = new MemberMapping( typeMapping, mappingSource, mappingTarget );
                    var membermappingcontext = new MemberMappingContext( membermapping );

                    var targetProperty = Expression.Property( context.TargetInstance, memberInfo.Name );
                    var t = new tempTrack();

                    var targetType = targetProperty.Type;
                    if( targetProperty.Type.IsInterface || targetProperty.Type.IsAbstract )
                        targetType = typeof( List<> ).MakeGenericType( targetType.GetGenericArguments() );

                    var mapping = MapperConfiguration[ targetType, targetsetprop.PropertyType ];

                    var memberAssignment = ((ReferenceMapper)mapping.Mapper).GetMemberAssignment( membermappingcontext )
                        .ReplaceParameter( sourceToReplace, "sourceValue" )
                        .ReplaceParameter( targetProperty, "targetValue" )
                        .ReplaceParameter( context.TargetInstance, "instance" );

                    var mainExp = t.mainexpression(
                        context.ReferenceTracker,
                        subParam, targetProperty,
                        memberAssignment, context.Mapper, _mapper,
                        Expression.Constant( null, typeof( IMapping ) ) );

                    return mainExp;

                    //version better integrated in ultramapper:
                    //return Expression.Block
                    //(
                    //    new[] { context.Mapper },
                    //    Expression.Assign( context.Mapper, Expression.Constant( _mapper ) ),
                    //    t.GetComplexMemberExpression( membermapping, memberAssignment )
                    //)
                    //.ReplaceParameter( context.SourceInstance, "instance" )
                    //.ReplaceParameter( context.TargetInstance, "instance" )
                    //.ReplaceParameter( context.ReferenceTracker, "referenceTracker" )
                    //.ReplaceParameter( context.Mapper, "mapper" );
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

    public class tempTrack
    {
        public static Func<ReferenceTracker, object, Type, object> referenceLookup =
            ( referenceTracker, sourceInstance, targetType ) =>
        {
            referenceTracker.TryGetValue( sourceInstance,
                targetType, out object targetInstance );

            return targetInstance;
        };

        public static Action<ReferenceTracker, object, Type, object> addReferenceToTracker =
            ( referenceTracker, sourceInstance, targetType, targetInstance ) =>
        {
            referenceTracker.Add( sourceInstance,
                targetType, targetInstance );
        };

        public Expression mainexpression(
            ParameterExpression referenceTracker,
            ParameterExpression sourceMember,
            Expression targetMember,
            Expression memberAssignment,
            ParameterExpression mapperParam,
            Mapper mapper,
            Expression mapping )
        {
            var refLookupExp = Expression.Call
            (
                Expression.Constant( referenceLookup.Target ),
                referenceLookup.Method,
                referenceTracker,
                sourceMember,
                Expression.Constant( targetMember.Type )
            );

            var addRefToTrackerExp = Expression.Call
            (
                Expression.Constant( addReferenceToTracker.Target ),
                addReferenceToTracker.Method,
                referenceTracker,
                sourceMember,
                Expression.Constant( targetMember.Type ),
                targetMember
            );

            var mapMethod = ReferenceMapperContext.RecursiveMapMethodInfo
                .MakeGenericMethod( sourceMember.Type, targetMember.Type );

            var trackedReference = Expression.Parameter( targetMember.Type, "trackedReference" );

            var sourceNullConstant = Expression.Constant( null, sourceMember.Type );
            var targetNullConstant = Expression.Constant( null, targetMember.Type );

            return Expression.Block
            (
                new[] { mapperParam, trackedReference },
                Expression.Assign( mapperParam, Expression.Constant( mapper ) ),
                //Expression.Assign( sourceMember, memberContext.SourceMemberValueGetter ),

                Expression.IfThenElse
                (
                     Expression.Equal( sourceMember, sourceNullConstant ),
                     Expression.Assign( targetMember, targetNullConstant ),
                     Expression.Block
                     (
                        Expression.Assign( trackedReference,
                            Expression.Convert( refLookupExp, targetMember.Type ) ),

                        Expression.IfThenElse
                        (
                            Expression.NotEqual( trackedReference, targetNullConstant ),
                            Expression.Assign( targetMember, trackedReference ),
                            Expression.Block
                            (
                                memberAssignment,
                                addRefToTrackerExp,
                                Expression.Call( mapperParam, mapMethod, sourceMember,
                                    targetMember, referenceTracker, mapping )
                            )
                        )
                    )
                )
            );
        }

        public Expression GetComplexMemberExpression( MemberMapping mapping, Expression memberAssignment )
        {
            /* SOURCE (NULL) -> TARGET = NULL
             * 
             * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NULL) = ASSIGN TRACKED OBJECT
             * SOURCE (NOT NULL / VALUE ALREADY TRACKED) -> TARGET (NOT NULL) = ASSIGN TRACKED OBJECT (the priority is to map identically the source to the target)
             * 
             * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET (NULL) = ASSIGN NEW OBJECT 
             * SOURCE (NOT NULL / VALUE UNTRACKED) -> TARGET (NOT NULL) = KEEP USING INSTANCE OR CREATE NEW INSTANCE
             */

            var memberContext = new MemberMappingContext( mapping );

            if( mapping.CustomConverter != null )
            {
                var targetSetterInstanceParamName = mapping.TargetMember.ValueSetter.Parameters[ 0 ].Name;
                var targetSetterValueParamName = mapping.TargetMember.ValueSetter.Parameters[ 1 ].Name;

                var valueReaderExp = Expression.Invoke( mapping.CustomConverter, memberContext.SourceMemberValueGetter );

                return mapping.TargetMember.ValueSetter.Body
                    .ReplaceParameter( memberContext.TargetInstance, targetSetterInstanceParamName )
                    .ReplaceParameter( valueReaderExp, targetSetterValueParamName )
                    .ReplaceParameter( valueReaderExp, mapping.CustomConverter.Parameters[ 0 ].Name );
            }

            var mapMethod = ReferenceMapperContext.RecursiveMapMethodInfo.MakeGenericMethod(
                memberContext.SourceMember.Type, memberContext.TargetMember.Type );

            Expression itemLookupCall = Expression.Call
            (
                Expression.Constant( referenceLookup.Target ),
                referenceLookup.Method,
                memberContext.ReferenceTracker,
                memberContext.SourceMember,
                Expression.Constant( memberContext.TargetMember.Type )
            );

            Expression itemCacheCall = Expression.Call
            (
                Expression.Constant( addReferenceToTracker.Target ),
                addReferenceToTracker.Method,
                memberContext.ReferenceTracker,
                memberContext.SourceMember,
                Expression.Constant( memberContext.TargetMember.Type ),
                memberContext.TargetMember
            );

            return Expression.Block
            (
                new[] { memberContext.TrackedReference, memberContext.SourceMember, memberContext.TargetMember },

                Expression.Assign( memberContext.SourceMember, memberContext.SourceMemberValueGetter ),

                Expression.IfThenElse
                (
                     Expression.Equal( memberContext.SourceMember, memberContext.SourceMemberNullValue ),

                     Expression.Assign( memberContext.TargetMember, memberContext.TargetMemberNullValue ),

                     Expression.Block
                     (
                        //object lookup. An intermediate variable (TrackedReference) is needed in order to deal with ReferenceMappingStrategies
                        Expression.Assign( memberContext.TrackedReference,
                            Expression.Convert( itemLookupCall, memberContext.TargetMember.Type ) ),

                        Expression.IfThenElse
                        (
                            Expression.NotEqual( memberContext.TrackedReference, memberContext.TargetMemberNullValue ),
                            Expression.Assign( memberContext.TargetMember, memberContext.TrackedReference ),
                            Expression.Block
                            (
                                memberAssignment,

                                //cache reference
                                itemCacheCall,

                                Expression.Call( memberContext.Mapper, mapMethod,
                                    memberContext.SourceMember, memberContext.TargetMember,
                                    memberContext.ReferenceTracker, Expression.Constant( mapping ) )
                            )
                        )
                    )
                ),

                memberContext.TargetMemberValueSetter
            );
        }
    }
}
