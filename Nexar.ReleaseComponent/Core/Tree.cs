using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Nexar.ReleaseComponent
{
    /// <summary>
    /// Tree view helper functions.
    /// </summary>
    static class Tree
    {
        /// <summary>
        /// Creates a tree item with the specified attached tag and expanding state.
        /// </summary>
        public static TreeViewItem CreateItem(object tag, bool toExpand)
        {
            var item = new TreeViewItem
            {
                Header = tag.ToString(),
                Tag = tag
            };

            if (toExpand)
                item.Items.Add(null);

            return item;
        }

        /// <summary>
        /// Recursive search for an ancestor item by the predicate, including the specified item.
        /// </summary>
        public static TreeViewItem FindAncestorItem(TreeViewItem item, Func<TreeViewItem, bool> predicate)
        {
            while (item != null)
            {
                if (predicate(item))
                    return item;

                var obj = VisualTreeHelper.GetParent(item);
                while (!(obj is TreeViewItem || obj is TreeView))
                    obj = VisualTreeHelper.GetParent(obj);

                item = obj as TreeViewItem;
            }
            return null;
        }

        /// <summary>
        /// Gets true if the tree item was expanded.
        /// </summary>
        public static bool IsItemPopulated(TreeViewItem item)
        {
            return item.Items.Count == 0 || item.Items[0] != null;
        }
    }
}
