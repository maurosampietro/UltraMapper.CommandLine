using System;
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

        public static Tree<MemberInfo> GetStructure( Type obj )
        {
            var tree = new Tree<MemberInfo>();

            foreach( var memberProvider in _memberProviders )
                memberProvider.PopulateTree( tree.Root, obj );

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
        private const BindingFlags _bindingAttrs =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;

        public void PopulateTree( TreeNode<MemberInfo> node, Type type )
        {
            var properties = type.GetProperties( _bindingAttrs );
            foreach( var property in properties )
            {
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
