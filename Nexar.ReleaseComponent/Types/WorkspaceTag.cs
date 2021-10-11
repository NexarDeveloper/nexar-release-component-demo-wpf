using Nexar.Client;

namespace Nexar.ReleaseComponent.Types
{
    sealed class WorkspaceTag : TagType<IMyWorkspace>
    {
        public WorkspaceTag(IMyWorkspace tag) : base(tag)
        {
        }

        public override string ToString()
        {
            return Tag.Name;
        }
    }
}
