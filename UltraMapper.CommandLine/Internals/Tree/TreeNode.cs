using System.Collections.Generic;

namespace CommandLine.AutoParser.Tree
{
    public sealed class TreeNode<T>
    {
        public T Item { get; private set; }
        public TreeNode<T> Parent { get; set; }
        public List<TreeNode<T>> Children { get; set; }
            = new List<TreeNode<T>>();

        public TreeNode() { }

        public TreeNode( T item, TreeNode<T> parent )
        {
            this.Item = item;
            this.Parent = parent;
        }

        public TreeNode<T> Add( T element )
        {
            var newNode = new TreeNode<T>( element, this );
            this.Children.Add( newNode );
            return newNode;
        }
    }
}
