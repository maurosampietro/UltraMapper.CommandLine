using System.Collections.Generic;
using System.Text;

namespace UltraMapper.CommandLine.Tree
{
    internal class Tree<T>
    {
        public TreeNode<T> Root { get; private set; }

        public Tree()
        {
            this.Root = new TreeNode<T>();
        }

        public IEnumerable<TreeNode<T>> GetLeaves()
        {
            return GetLeaves( this.Root );
        }

        public IReadOnlyCollection<TreeNode<T>> Flatten()
        {
            return Flatten( this.Root );
        }

        public static IEnumerable<TreeNode<T>> GetLeaves( TreeNode<T> node )
        {
            if( node.Children.Count == 0 )
                yield return node;

            foreach( var child in node.Children )
            {
                foreach( var leaf in GetLeaves( child ) )
                    yield return leaf;
            }
        }

        public static IReadOnlyCollection<TreeNode<T>> Flatten( TreeNode<T> node )
        {
            return RootToLeafTraversal2.Traverse( node );
        }

        public override string ToString()
        {
            var sb = RootToLeafTraversal.Traverse( this );
            return sb.ToString();
        }
    }

    internal class RootToLeafTraversal
    {
        public static string Traverse<T>( Tree<T> tree )
        {
            return Traverse( tree.Root, new StringBuilder() ).ToString();
        }

        public static StringBuilder Traverse<T>( TreeNode<T> treeNode, StringBuilder sb, int tabs = 0 )
        {
            if( treeNode.Item != null )
                sb.AppendLine( new string( '\t', tabs ) + "" + treeNode.Item.ToString() );

            foreach( var child in treeNode.Children )
                Traverse( child, sb, tabs + 1 );

            return sb;
        }
    }

    internal class RootToLeafTraversal2
    {
        public static List<TreeNode<T>> Traverse<T>( TreeNode<T> node )
        {
            var list = new List<TreeNode<T>>();
            return Traverse( node, list );
        }

        public static List<TreeNode<T>> Traverse<T>( TreeNode<T> treeNode, List<TreeNode<T>> list )
        {
            if( treeNode.Item != null )
                list.Add( treeNode );

            foreach( var child in treeNode.Children )
                Traverse( child, list );

            return list;
        }
    }
}
