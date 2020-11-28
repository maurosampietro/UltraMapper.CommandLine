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
        private static readonly Dictionary<Type, List<ParameterDefinition>> _cache
            = new Dictionary<Type, List<ParameterDefinition>>();

        public static IEnumerable<ParameterDefinition> GetCommandDefinitions<T>()
        {
            return GetCommandDefinitions( typeof( T ) );
        }

        public static IEnumerable<ParameterDefinition> GetCommandDefinitions( Type type )
        {
            if( !_cache.TryGetValue( type, out List<ParameterDefinition> definition ) )
            {
                var tree = TypeStructure.GetStructure( type );
                definition = GetDefInternal( tree.Root ).ToList();
                _cache.Add( type, definition );
            }

            return definition;
        }

        private static readonly Dictionary<MemberInfo, IEnumerable<ParameterDefinition>> _memberDefinitions =
            new Dictionary<MemberInfo, IEnumerable<ParameterDefinition>>();

        private static IEnumerable<ParameterDefinition> GetDefInternal( TreeNode<MemberInfo> root )
        {
            //if( root?.Item != null && _memberDefinitions.TryGetValue( root.Item, out IEnumerable<ParameterDefinition> subparamdefs ) )
            //    foreach( var item in subparamdefs )
            //        yield return item;

            //the roots are the commands, the leaves are the parameters
            foreach( var command in root.Children )
            {
                var optionAttribute = command.Item.GetCustomAttribute<OptionAttribute>();
                if( optionAttribute?.IsIgnored == true )
                    continue;

                if( optionAttribute == null )
                    optionAttribute = new OptionAttribute();

                string name = String.IsNullOrWhiteSpace( optionAttribute?.Name ) ?
                    command.Item.Name : optionAttribute.Name;

                if( !Regex.IsMatch( name, @"^\w+$" ) )
                    throw new InvalidNameException( name );

                ParameterDefinition[] subparameters = null;

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
                    subparameters = GetDefInternal( command ).ToArray();
                }

                var paramDefinition = new ParameterDefinition()
                {
                    Options = optionAttribute,
                    Name = name,
                    SubParams = subparameters,
                    Type = type,
                    MemberType = memberType
                };
                
                yield return paramDefinition;
            }
        }

        private static IEnumerable<ParameterDefinition> GetMethodParams( TreeNode<MemberInfo> node, MethodInfo methodInfo )
        {
            var methodParams = methodInfo.GetParameters();
            for( int i = 0; i < methodParams.Length; i++ )
            {
                var methodParam = methodParams[ i ];

                var optionAttribute = methodParam.GetCustomAttribute<OptionAttribute>();
                if( optionAttribute == null )
                    optionAttribute = new OptionAttribute();

                optionAttribute.IsRequired = !methodParam.IsOptional;

                string name = String.IsNullOrWhiteSpace( optionAttribute.Name ) ?
                    methodParam.Name : optionAttribute.Name;

                if( !Regex.IsMatch( name, @"^\w+$" ) )
                    throw new InvalidNameException( name );

                var subparameters = GetDefInternal( node.Children[ i ] ).ToArray();

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
    }
}
