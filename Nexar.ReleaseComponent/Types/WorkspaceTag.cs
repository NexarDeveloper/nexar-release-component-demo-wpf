using Nexar.Client;

namespace Nexar.ReleaseComponent
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
