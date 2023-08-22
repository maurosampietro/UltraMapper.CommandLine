using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UltraMapper.CommandLine.Parsers;
using UltraMapper.Conventions;
using UltraMapper.Internals;
using UltraMapper.MappingExpressionBuilders;
using UltraMapper.Parsing;

namespace UltraMapper.CommandLine.Extensions
{
    public class ParsedCommandExpressionBuilder : ReferenceMapper
    {
        private readonly IHelpProvider _helpProvider;
        private readonly Expression<Action<Type, IParsedParam>> _helperCall;
        private readonly TargetMemberProvider _targetMemberProvider = new()
        {
            IgnoreFields = true,
            IgnoreNonPublicMembers = true,
            AllowGetterOrSetterMethodsOnly = false,
            DeclaredOnly = true,
            FlattenHierarchy = false
        };

        public ParsedCommandExpressionBuilder( IHelpProvider helpProvider )
        {
            _helpProvider = helpProvider;
            _helperCall = ( type, param ) => _helpProvider.GetHelp( type, param );
        }

        public override bool CanHandle( Mapping mapping )
        {
            var source = mapping.Source.EntryType;
            var target = mapping.Target.EntryType;

            return source == typeof( ParsedCommand ) &&
                target != typeof( ParsedCommand ); //allow cloning
        }

        public override LambdaExpression GetMappingExpression( Mapping mapping )
        {
            var source = mapping.Source.EntryType;
            var target = mapping.Target.EntryType;

            var context = this.GetMapperContext( mapping );
            var targetMembers = this.SelectTargetMembers( target ).ToArray();

            var subParam = Expression.Parameter( typeof( IParsedParam ), "param" );

            var memberAssign = this.GetMemberAssignments( context,
                targetMembers, subParam, mapping.GlobalConfig ).ToList();

            var help = Expression.IfThen
            (
                Expression.Equal( Expression.Constant( "help" ),
                    Expression.Call( Expression.Property( context.SourceInstance,
                    nameof( ParsedCommand.Name ) ), nameof( String.ToLower ), null, null ) ),

                Expression.Invoke( _helperCall, Expression.Constant( context.TargetInstance.Type ),
                    Expression.Property( context.SourceInstance, nameof( ParsedCommand.Param ) ) )
            );

            var expression = Expression.Block
            (
                new[] { context.Mapper, subParam },

                Expression.Assign( context.Mapper, Expression.Constant( context.MapperInstance ) ),

                Expression.Assign( subParam, Expression.Property( context.SourceInstance, nameof( ParsedCommand.Param ) ) ),
                Expression.Block( memberAssign ),
                help,

                Expression.Constant( null, context.TargetInstance.Type )
            );


            var delegateType = typeof( UltraMapperDelegate<,> ).MakeGenericType(
               context.SourceInstance.Type, context.TargetInstance.Type );

            return Expression.Lambda( delegateType, expression,
                context.ReferenceTracker, context.SourceInstance, context.TargetInstance );
        }

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
            var typeMapping = MapperConfiguration[ context.SourceInstance.Type, context.TargetInstance.Type ];

            if( memberInfo is PropertyInfo propertyInfo )
            {
                var mappingSource = this.GetMappingSource( context, propertyInfo );
                var mappingTarget = new MappingTarget( memberInfo );
                var memberMapping = typeMapping.AddMemberToMemberMapping( mappingSource, mappingTarget );

                var memberAssignment = memberMapping.MemberMappingExpression.Body
                    .ReplaceParameter( context.Mapper, "mapper" )
                    .ReplaceParameter( subParam, mappingSource.ValueGetter.Parameters[ 0 ].Name )
                    .ReplaceParameter( context.ReferenceTracker, "referenceTracker" )
                    .ReplaceParameter( context.SourceInstance, "sourceInstance" )
                    .ReplaceParameter( context.TargetInstance, "targetInstance" );

                return memberAssignment;
            }
            else if( memberInfo is MethodInfo methodInfo )
            {
                var parametersExps = new List<Expression>();
                var methodParams = methodInfo.GetParameters();

                for( int i = 0; i < methodParams.Length; i++ )
                {
                    var param = methodParams[ i ];
                    Type paramType = GetParamType( param );

                    static Type GetParamType( ParameterInfo param )
                    {
                        if( param.ParameterType.IsBuiltIn( true ) )
                            return typeof( SimpleParam );

                        if( param.ParameterType.IsEnumerable() )
                            return typeof( ArrayParam );

                        return typeof( ComplexParam );
                    }

                    var itemMapping = MapperConfiguration[ paramType, param.ParameterType ].MappingExpression;

                    var convertExp = (Expression)Expression.NewArrayInit( typeof( IParsedParam ),
                        Expression.Convert(
                            Expression.Property( context.SourceInstance, nameof( ParsedCommand.Param ) ),
                        paramType ) );

                    if( methodParams.Length > 1 )
                    {
                        var mappingSource = new MappingSource<ParsedCommand, IList<IParsedParam>>( p => ((ComplexParam)p.Param).SubParams );
                        convertExp = mappingSource.ValueGetter.Body
                            .ReplaceParameter( context.SourceInstance, mappingSource.ValueGetter.Parameters[ 0 ].Name );
                    }

                    if( paramType == typeof( SimpleParam ) )
                    {
                        var paramExp = Expression.Invoke
                        (
                            itemMapping,
                            Expression.Convert
                            (
                                Expression.Invoke( _selectParamExp, convertExp, Expression.Constant( param ) ),
                                paramType
                            )
                        );

                        parametersExps.Add( paramExp );
                    }
                    else
                    {
                        if(param.ParameterType.IsEnumerable())
                        {
                            var paramTargetType = param.ParameterType;
                            //if( paramTargetType.GetGenericTypeDefinition() == typeof( IEnumerable<> ) )
                            //    paramTargetType = typeof( List<> ).MakeGenericType( param.ParameterType.GetCollectionGenericType() );

                            var temp = MapperConfiguration[ paramType, paramTargetType ].MappingExpression;

                            var tempTarget = Expression.Parameter( paramTargetType, "temptarget" );
                            var selectedParam = Expression.Parameter( paramType, "selectedParam" );

                            var getCount = typeof( System.Linq.Enumerable ).GetMethods(
                                BindingFlags.Static | BindingFlags.Public )
                            .First( m =>
                            {
                                if(m.Name != nameof( System.Linq.Enumerable.Count ))
                                    return false;

                                var parameters = m.GetParameters();
                                if(parameters.Length != 1) return false;

                                return parameters[ 0 ].ParameterType.GetGenericTypeDefinition() == typeof( IEnumerable<> );
                            } )
                            .MakeGenericMethod( typeof( IParsedParam ) );

                            Expression GetNewInstanceWithReservedCapacity()
                            {
                                var constructorWithCapacity = paramTargetType.GetConstructor( new Type[] { typeof( int ) } );
                                if(constructorWithCapacity != null)
                                {
                                    var itemsProperty = Expression.Property( Expression.Convert( selectedParam,
                                        typeof( ArrayParam ) ), nameof( ArrayParam.Items ) );

                                    var getCountMethod = Expression.Call( null, getCount, itemsProperty );
                                    return Expression.New( constructorWithCapacity, getCountMethod );
                                }
                                else {
                                    return Expression.New( typeof( List<> ).MakeGenericType(paramTargetType.GenericTypeArguments) );
                                }
                            }
                            
                            var getNewInstanceExp = GetNewInstanceWithReservedCapacity();

                            var paramExp = Expression.Block
                            (
                                new[] { tempTarget, selectedParam },

                                Expression.Assign( selectedParam, Expression.Convert
                                (
                                    Expression.Invoke( _selectParamExp, convertExp, Expression.Constant( param ) ),
                                    paramType
                                ) ),

                                Expression.Assign( tempTarget, getNewInstanceExp ),

                                Expression.Invoke( itemMapping, context.ReferenceTracker, selectedParam, tempTarget ),

                               tempTarget
                            );

                            parametersExps.Add( paramExp );
                        }
                        else
                        {
                            //var mappingSource = new MappingSource<ParsedCommand, ComplexParam>( p => (ComplexParam)p.Param );
                            //convertExp = mappingSource.ValueGetter.Body
                            //    .ReplaceParameter( context.SourceInstance, mappingSource.ValueGetter.Parameters[ 0 ].Name );

                            //itemMapping = MapperConfiguration[ typeof( IParsedParam ), param.ParameterType ].MappingExpression;

                            var targetParamType = param.ParameterType;
                            if( param.ParameterType.IsInterface || param.ParameterType.IsAbstract )
                                targetParamType = typeof( List<> ).MakeGenericType( targetParamType.GetGenericArguments() );

                            var tempTarget = Expression.Parameter( param.ParameterType, "temptarget" );
                            var selectedParam = Expression.Convert
                            (
                                Expression.Invoke( _selectParamExp, convertExp, Expression.Constant( param ) ),
                                paramType
                            );

                            var paramExp = Expression.Block
                            (
                                new[] { tempTarget },

                                Expression.Assign( tempTarget, Expression.New( targetParamType ) ),

                                Expression.Invoke( itemMapping, context.ReferenceTracker, selectedParam, tempTarget ),

                                tempTarget
                            );

                            parametersExps.Add( paramExp );
                        }
                    }
                }

                //optional parameters are mandatory with Expression.Call: pass the default value
                return Expression.Block
                (
                    Expression.Call( context.TargetInstance, methodInfo, parametersExps.ToArray() )
                );
            }

            throw new NotSupportedException();
        }

        private IMappingSource GetMappingSource( ReferenceMapperContext context, PropertyInfo propertyInfo )
        {
            if( propertyInfo.PropertyType.IsBuiltIn( true ) )
                return new MappingSource<ParsedCommand, string>( pc => ((SimpleParam)pc.Param).Value );

            if( propertyInfo.PropertyType.IsEnumerable() )
                return new MappingSource<ParsedCommand, ArrayParam>( pc => (ArrayParam)pc.Param );

            return new MappingSource<ParsedCommand, ComplexParam>( pc => (ComplexParam)pc.Param );
        }

        private static readonly Expression<Func<IList<IParsedParam>, ParameterInfo, object>> _selectParamExp =
            ( parsedparams, methodparam ) => SelectParam( parsedparams, methodparam );

        private static IParsedParam SelectParam( IList<IParsedParam> parsedparams, ParameterInfo methodParam )
        {
            var paramOptions = methodParam.GetCustomAttribute<OptionAttribute>();

            string paramName = paramOptions?.Name?.ToLower();
            if( String.IsNullOrWhiteSpace( paramName ) )
                paramName = methodParam.Name.ToLower();

            var namedParam = parsedparams.FirstOrDefault( p => p.Name.ToLower() == paramName );
            if( namedParam != null ) return namedParam;

            int paramOrder = paramOptions?.Order ?? -1;
            if( paramOrder == -1 )
                paramOrder = methodParam.Position;

            return parsedparams.FirstOrDefault( p => p.Index == paramOrder );
        }

        private IEnumerable<MemberInfo> SelectTargetMembers( Type target )
        {
            return _targetMemberProvider.GetMembers( target )
                .Select( ( m, index ) => new
                {
                    Member = m,
                    Options = m.GetCustomAttribute<OptionAttribute>() ??
                        new OptionAttribute() {/*Order = index*/ }
                } )
                .Where( m => !m.Options.IsIgnored )
                .OrderByDescending( info => info.Options.IsRequired )
                .ThenBy( info => info.Options.Order )
                .Select( m => m.Member );
        }
    }
}
