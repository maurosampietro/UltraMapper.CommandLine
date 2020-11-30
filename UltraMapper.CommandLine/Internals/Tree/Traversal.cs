﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UltraMapper.Internals;

namespace UltraMapper.CommandLine.Tree
{
    internal class TypeStructure
    {
        private static readonly IMemberProvider[] _memberProviders = new IMemberProvider[]
        {
            new PropertyProvider(),
            new MethodProvider()
        };

        public static Tree<MemberInfo> GetStructure( Type type )
        {
            var tree = new Tree<MemberInfo>( type );

            foreach( var memberProvider in _memberProviders )
                memberProvider.PopulateTree( tree.Root, type );

            return tree;
        }
    }

    internal interface IMemberProvider
    {
        void PopulateTree( TreeNode<MemberInfo> node, Type type );
    }

    internal class MethodProvider : IMemberProvider
    {
        private static readonly PropertyProvider _propertyProvider = new PropertyProvider();

        private const BindingFlags _bindingAttrs =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;

        public void PopulateTree( TreeNode<MemberInfo> node, Type type )
        {
            var methods = type.GetMethods( _bindingAttrs );
            foreach( var method in methods )
            {
                if( method.ReturnType != typeof( void ) )
                    continue;

                if( method.IsAbstract ||
                    method.IsGenericMethod ||
                    method.IsSpecialName )
                    continue;

                var optionAttribute = method.GetCustomAttribute<OptionAttribute>();
                if( optionAttribute?.IsIgnored == true ) continue;

                var newNode = node.Add( method );

                var parameters = method.GetParameters();
                if( parameters.Length > 0 )
                {
                    foreach( var param in parameters )
                    {
                        var newParamNode = newNode.Add( param.ParameterType );
                        _propertyProvider.PopulateTree( newParamNode, param.ParameterType );
                    }
                }
            }
        }
    }

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
