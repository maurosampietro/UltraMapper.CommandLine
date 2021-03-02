using System;
using System.Collections.Generic;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.CommandLine.Tree
{
    internal class PropertyProvider : IMemberProvider
    {
        private static readonly Dictionary<Type, TreeNode<MemberInfo>> _typeNodes
            = new Dictionary<Type, TreeNode<MemberInfo>>();

        private const BindingFlags _bindingAttrs =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;

        public void PopulateTree( TreeNode<MemberInfo> node, Type type )
        {
            if( _typeNodes.TryGetValue( type, out TreeNode<MemberInfo> processedNode ) )
            {
                node.Children = processedNode.Children;
                return;
            }

            _typeNodes.Add( type, node );

            var properties = type.GetProperties( _bindingAttrs );
            foreach( var property in properties )
            {
                if( property.GetSetMethod() == null )
                    continue;

                var optionAttribute = property.GetCustomAttribute<OptionAttribute>();
                if( optionAttribute?.IsIgnored == true ) continue;

                var newNode = node.Add( property );

                if( !property.PropertyType.IsArray &&
                    !property.PropertyType.IsEnumerable() &&
                    !property.PropertyType.IsBuiltIn( true ) )
                {
                    PopulateTree( newNode, property.PropertyType );
                }
            }
        }
    }
}
