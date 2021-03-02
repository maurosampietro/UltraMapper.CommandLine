using System;
using System.Reflection;

namespace UltraMapper.CommandLine.Tree
{
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
}
