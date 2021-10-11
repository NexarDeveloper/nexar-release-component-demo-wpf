using Nexar.Client;
using System.Collections.Generic;

namespace Nexar.ReleaseComponent.Types
{
    sealed class FolderTag
    {
        public string Name { get; }
        public IEnumerable<IMyComponent> Components { get; }
        public WorkspaceTag Workspace { get; }

        public FolderTag(string name, IEnumerable<IMyComponent> components, WorkspaceTag workspace)
        {
            Name = name;
            Components = components;
            Workspace = workspace;
        }

        public override string ToString()
        {
            return Name ?? "<No folder>";
        }
    }
}
