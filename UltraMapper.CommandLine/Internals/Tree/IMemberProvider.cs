using System;
using System.Reflection;

namespace UltraMapper.CommandLine.Tree
{
    internal interface IMemberProvider
    {
        void PopulateTree( TreeNode<MemberInfo> node, Type type );
    }
}
