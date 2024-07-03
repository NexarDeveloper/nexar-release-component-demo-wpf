using Nexar.Client;

namespace Nexar.ReleaseComponent
{
    sealed class ComponentTag : TagType<IMyComponent>
    {
        public WorkspaceTag Workspace { get; }

        public ComponentTag(IMyComponent tag, WorkspaceTag workspace) : base(tag)
        {
            Workspace = workspace;
        }

        public override string ToString()
        {
            return Tag.Name;
        }
    }
}
