using Nexar.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nexar.ReleaseComponent
{
    static class AllOperations
    {
        public static async Task<IReadOnlyList<IWorkspaces_DesWorkspaceInfos>> GetWorkspacesAsync(
            this NexarClient client)
        {
            var res = await client.Workspaces.ExecuteAsync();
            res.AssertNoErrors();
            return res.Data.DesWorkspaceInfos;
        }

        public static async Task<IReadOnlyList<IFolders_DesLibrary_Folders>> GetFolders(
            this NexarClient client,
            string workspaceUrl)
        {
            var res = await client.Folders.ExecuteAsync(workspaceUrl);
            res.AssertNoErrors();
            return res.Data.DesLibrary.Folders;
        }

        public static async Task<List<IMyComponent>> GetComponentsAsync(
            this NexarClient client,
            string workspaceUrl)
        {
            var list = new List<IMyComponent>();
            string endCursor = null;
            while (true)
            {
                var res = await client.Components.ExecuteAsync(workspaceUrl, 1000, endCursor);
                res.AssertNoErrors();
                var data = res.Data.DesLibrary.Components;
                list.AddRange(data.Nodes);
                if (!data.PageInfo.HasNextPage)
                    break;
                endCursor = data.PageInfo.EndCursor;
            }
            return list;
        }

        public static async Task<IRevisionDetailsById_DesRevisionDetailsById> GetRevisionDetailsByIdAsync(
            this NexarClient client,
            string revisionId)
        {
            var res = await client.RevisionDetailsById.ExecuteAsync(revisionId);
            res.AssertNoErrors();
            return res.Data.DesRevisionDetailsById;
        }

        public static async Task<IReleaseDefinitionsResult> GetReleaseDefinitionsAsync(
            this NexarClient client,
            string workspaceUrl)
        {
            var res = await client.ReleaseDefinitions.ExecuteAsync(workspaceUrl);
            res.AssertNoErrors();
            return res.Data;
        }

        public static async Task ReleaseComponentAsync(
            this NexarClient client,
            DesReleaseComponentInput input)
        {
            var res = await client.ReleaseComponent.ExecuteAsync(input);
            res.AssertNoErrors();
        }
    }
}
