using System;
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
}
