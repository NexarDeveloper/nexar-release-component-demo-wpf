using Nexar.Client;
using Nexar.ReleaseComponent.Properties;
using Nexar.ReleaseComponent.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;

namespace Nexar.ReleaseComponent
{
    public partial class MainWindow : Window
    {
        private string _workspaceUrl;
        private TreeViewItem _selectedItem;

        /// <summary>
        /// Old window placement.
        /// </summary>
        WindowPlacement _WindowPlacement;

        public MainWindow()
        {
            InitializeComponent();

            // show the endpoint in the title
            Title = $"Login... {Config.ApiEndpoint}";

            // load as a task after the window is shown
            Task.Run(async () =>
            {
                // login
                await App.LoginAsync();

                // load data
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // begin
                    Title = $"Loading... {Config.ApiEndpoint}";
                    TaskbarItemInfo = new TaskbarItemInfo { ProgressState = TaskbarItemProgressState.Indeterminate };

                    // activate, after browser windows this window may be passive
                    Activate();
                    Topmost = true;
                    Topmost = false;
                    Focus();

                    Task.Run(async () =>
                    {
                        // get data
                        await App.LoadWorkspacesAsync();

                        // show data
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // populate the tree with to be expanded workspaces
                            foreach (var workspace in App.Workspaces)
                                MyTree.Items.Add(Tree.CreateItem(new WorkspaceTag(workspace), true));

                            // end
                            Title = $"Nexar.ReleaseComponent Demo - {Config.ApiEndpoint}";
                            TaskbarItemInfo = new TaskbarItemInfo { ProgressState = TaskbarItemProgressState.None };
                        });
                    });
                });
            });
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // load window placement
            try
            {
                _WindowPlacement = Settings.Default.WindowPlacement;
                WindowPlacement.Set(this, _WindowPlacement);
            }
            catch
            { }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // save window placement
            var newPlacement = WindowPlacement.Get(this);
            if (!newPlacement.Equals(_WindowPlacement))
            {
                Settings.Default.WindowPlacement = newPlacement;
                Settings.Default.Save();
            }
        }

        /// <summary>
        /// Fetches workspace components and populates its tree item with folders.
        /// </summary>
        private void ExpandWorkspaceItem(TreeViewItem item)
        {
            var workspace = (WorkspaceTag)item.Tag;
            item.Items.Clear();

            // fetch components (shallow info) with paging
            var components = Task.Run(async () =>
            {
                var list = new List<IMyComponent>();
                string endCursor = null;
                while (true)
                {
                    var res = await App.Client.Components.ExecuteAsync(workspace.Tag.Url, 1000, endCursor);
                    ClientHelper.EnsureNoErrors(res);
                    var data = res.Data.DesLibrary.Components;
                    list.AddRange(data.Nodes);
                    if (!data.PageInfo.HasNextPage)
                        break;
                    endCursor = data.PageInfo.EndCursor;
                }
                return list;
            }).Result;

            // group components by folder names
            var folders = components
                .GroupBy(x => x.Folder?.Name)
                .OrderBy(x => x.Key);

            // populate workspace with folders
            foreach (var folder in folders)
            {
                var folderTreeItem = Tree.CreateItem(new FolderTag(folder.Key, folder, workspace), true);
                item.Items.Add(folderTreeItem);
            }
        }

        /// <summary>
        /// Populates the folder tree item with components.
        /// </summary>
        private void PopulateFolderItem(TreeViewItem item)
        {
            var folder = (FolderTag)item.Tag;
            item.Items.Clear();

            foreach (var component in folder.Components.OrderBy(x => x.Name))
                item.Items.Add(Tree.CreateItem(new ComponentTag(component, folder.Workspace), false));
        }

        public void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            // skip once expanded
            var item = e.Source as TreeViewItem;
            if (Tree.IsItemPopulated(item))
                return;

            // populate expanded item
            using (new WaitCursor())
            {
                if (item.Tag is WorkspaceTag)
                {
                    ExpandWorkspaceItem(item);
                    return;
                }

                if (item.Tag is FolderTag)
                {
                    PopulateFolderItem(item);
                    return;
                }
            }
        }

        // Avoid three view auto scroll to the right on long items.
        private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        // If a component is selected then update the info.
        private void MyTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!(e.NewValue is TreeViewItem item))
                return;

            _selectedItem = item;
            ButtonRelease.IsEnabled = false;
            PanelRevision.Visibility = Visibility.Hidden;

            if (item.Tag is WorkspaceTag workspace)
            {
                _workspaceUrl = workspace.Tag.Url;
                ButtonRelease.IsEnabled = true;
                return;
            }

            if (item.Tag is FolderTag folder)
            {
                _workspaceUrl = folder.Workspace.Tag.Url;
                ButtonRelease.IsEnabled = true;
                return;
            }

            if (item.Tag is ComponentTag component)
            {
                _workspaceUrl = component.Workspace.Tag.Url;
                ButtonRelease.IsEnabled = true;
                using (new WaitCursor())
                {
                    var revisionId = component.Tag.Revision.Id;
                    var revision = Task.Run(async () =>
                    {
                        var res = await App.Client.RevisionDetailsById.ExecuteAsync(revisionId);
                        ClientHelper.EnsureNoErrors(res);
                        return res.Data.DesRevisionDetailsById;
                    }).Result;

                    var sb = new StringBuilder();
                    sb.AppendLine();
                    if (revision.Comment != null)
                        sb.AppendLine($"Comment: {revision.Comment}");
                    if (revision.Description != null)
                        sb.AppendLine($"Description: {revision.Description}");
                    sb.AppendLine($"{component.Tag.Name} is in the {revision.LifeCycleState.Name} state.");
                    if (revision.ChildCount > 0)
                        sb.AppendLine($"{component.Tag.Name} has {revision.ChildCount} dependencies.");
                    if (revision.ParentCount > 0)
                        sb.AppendLine($"{component.Tag.Name} is used by {revision.ParentCount} components.");

                    TextRevision.Text = sb.ToString();
                    PanelRevision.Visibility = Visibility.Visible;
                }
                return;
            }
        }

        // `F2` - open the selected workspace.
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //! F1 is bad, started browser may see it pressed and open help
            // open selected workspace/project
            if (e.Key == Key.F2)
            {
                var item = Tree.FindAncestorItem((TreeViewItem)MyTree.SelectedItem, x => x.Tag is WorkspaceTag);
                if (item != null && item.Tag is WorkspaceTag workspaceTag)
                {
                    Process.Start(workspaceTag.Tag.Url);
                    return;
                }
            }
        }

        // Show the release dialog.
        private void ClickRelease(object sender, RoutedEventArgs e)
        {
            if (_workspaceUrl == null)
                return;

            // show the release dialog, exit on cancel
            var form = new ReleaseWindow(_workspaceUrl);
            if (form.ShowDialog() != true)
                return;

            // skip not yet populated workspace
            var workspaceItem = Tree.FindAncestorItem(_selectedItem, x => x.Tag is WorkspaceTag);
            if (!Tree.IsItemPopulated(workspaceItem))
                return;

            // populate the workspace item
            using (new WaitCursor())
                ExpandWorkspaceItem(workspaceItem);
        }
    }
}
