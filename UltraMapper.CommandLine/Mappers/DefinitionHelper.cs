using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UltraMapper.CommandLine.Tree;
using UltraMapper.Internals;

namespace UltraMapper.CommandLine.Mappers
{
    public class DefinitionHelper
    {
        private static readonly Dictionary<MemberInfo, ParameterDefinition[]> _cache =
            new Dictionary<MemberInfo, ParameterDefinition[]>();

        public static IEnumerable<ParameterDefinition> GetCommandDefinitions<T>()
        {
            return GetCommandDefinitions( typeof( T ) );
        }

        public static ParameterDefinition[] GetCommandDefinitions( Type type )
        {
            if( !_cache.TryGetValue( type, out ParameterDefinition[] definition ) )
            {
                var tree = TypeStructure.GetStructure( type );
                definition = GetDefInternal( tree.Root );
            }

            return definition;
        }

        private static ParameterDefinition[] GetDefInternal( TreeNode<MemberInfo> root )
        {
            var nodeType = root.Item.GetMemberType();
            if( _cache.TryGetValue( nodeType, out ParameterDefinition[] values ) )
            {
                return values;
            }

            var subs = new ParameterDefinition[ root.Children.Count ];
            _cache.Add( nodeType, subs );

            for( int i = 0; i < root.Children.Count; i++ )
            {
                var command = root.Children[ i ];

                var optionAttribute = command.Item.GetCustomAttribute<OptionAttribute>() ?? new OptionAttribute();
                if( optionAttribute?.IsIgnored == true )
                    continue;

                string name = String.IsNullOrWhiteSpace( optionAttribute?.Name ) ?
                    command.Item.Name : optionAttribute.Name;

                if( !Regex.IsMatch( name, @"^\w+$" ) )
                    throw new InvalidNameException( name );

                var subparameters = Array.Empty<ParameterDefinition>();

                Type type = null;
                var memberType = MemberTypes.UNDEFINED;
                if( command.Item is MethodInfo methodInfo )
                {
                    memberType = MemberTypes.METHOD;
                    if( !methodInfo.TryGetMemberType( out type ) )
                    {

                    }

                    subparameters = GetMethodParams( command, methodInfo ).ToArray();
                }
                else
                {
                    memberType = MemberTypes.PROPERTY;
                    type = command.Item.GetMemberType();

                    if( !type.IsBuiltIn( true ) )
                    {
                        subparameters = GetDefInternal( command );
                    }
                }

                var paramDefinition = new ParameterDefinition()
                {
                    Options = optionAttribute,
                    Name = name,
                    SubParams = subparameters,
                    Type = type,
                    MemberType = memberType
                };

                subs[ i ] = paramDefinition;
            }

            return subs;
        }

        private static IEnumerable<ParameterDefinition> GetMethodParams( TreeNode<MemberInfo> node, MethodInfo methodInfo )
        {
            var methodParams = methodInfo.GetParameters();
            for( int i = 0; i < methodParams.Length; i++ )
            {
                var methodParam = methodParams[ i ];

                var optionAttribute = methodParam.GetCustomAttribute<OptionAttribute>() ?? new OptionAttribute();
                optionAttribute.IsRequired = !methodParam.IsOptional;

                string name = String.IsNullOrWhiteSpace( optionAttribute.Name ) ?
                    methodParam.Name : optionAttribute.Name;

                if( !Regex.IsMatch( name, @"^\w+$" ) )
                    throw new InvalidNameException( name );

                var subparameters = GetDefInternal( node.Children[ i ] );

                yield return new ParameterDefinition()
                {
                    Name = name,
                    Options = optionAttribute,
                    Type = methodParam.ParameterType,
                    SubParams = subparameters,
                    MemberType = MemberTypes.METHOD_PARAM
                };
            }
        }

        internal static void Update( Type type, ParameterDefinition[] parameterDefinitions )
        {
            _cache[ type ] = parameterDefinitions;
        }
    }
}
