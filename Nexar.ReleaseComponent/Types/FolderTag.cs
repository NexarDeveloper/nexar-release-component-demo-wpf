using Nexar.Client;
using System.Collections.Generic;
using System.Linq;

namespace Nexar.ReleaseComponent
{
    sealed class FolderTag
    {
        readonly WorkspaceTag _workspace;
        readonly FolderTreeNode _folderNode;
        readonly List<FolderTag> _myFolders;
        readonly List<IMyComponent> _myComponents;

        public string Id => _folderNode.Folder.Id;
        public string Name => _folderNode.Folder.Name;
        public WorkspaceTag Workspace => _workspace;

        public IReadOnlyList<FolderTag> Folders => _myFolders;
        public IReadOnlyList<IMyComponent> Components => _myComponents;
        public bool CanExpand => _myFolders.Count + _myComponents.Count > 0;

        public FolderTag(FolderTreeNode folderNode, IEnumerable<IMyComponent> components, WorkspaceTag workspace)
        {
            _workspace = workspace;
            _folderNode = folderNode;

            _myFolders = folderNode.Nodes
                .Select(x => new FolderTag(x, components, workspace))
                .ToList();

            var folderId = folderNode.Folder.Id;
            _myComponents = components
                .Where(x => x.Folder?.Id == folderId)
                .OrderBy(x => x.Name)
                .ToList();
        }

        public override string ToString()
        {
            var name = Name;
            return string.IsNullOrEmpty(name) ? "<empty>" : name;
        }
    }
}
