using Nexar.Client;
using Nexar.ReleaseComponent.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;

namespace Nexar.ReleaseComponent
{
    partial class ReleaseWindow : Window
    {
        private const string cVideoContentTypeId = "46F5A583-0605-48D5-A7E2-8119124377B7";
        private static string _workspaceUrl;
        private static string _lastWorkspaceUrl;
        private static IMyRevisionNamingScheme[] _schemes;
        private static IMyLifeCycleDefinition[] _cycles;

        internal ReleaseWindow(string workspaceUrl)
        {
            _workspaceUrl = workspaceUrl;

            InitializeComponent();

            using (new WaitCursor())
            {
                Reset();

                // once per workspace
                if (_lastWorkspaceUrl != _workspaceUrl)
                {
                    // query data for combos
                    var data = Task.Run(async () =>
                    {
                        var res = await App.Client.ReleaseDefinitions.ExecuteAsync(_workspaceUrl);
                        ClientHelper.EnsureNoErrors(res);
                        return res.Data.DesLibrary;
                    }).Result;

                    // current combo data
                    _schemes = data.RevisionNamingSchemes.OrderBy(x => x.Name).ToArray();
                    _cycles = data.LifeCycleDefinitions.OrderBy(x => x.Name).ToArray();

                    // set the current workspace
                    _lastWorkspaceUrl = _workspaceUrl;
                }
            }

            // add naming schemes
            {
                if (_schemes.Length == 0)
                    throw new Exception("Found no revision naming schemes.");

                foreach (var scheme in _schemes)
                    ComboSchemes.Items.Add(scheme.Name);

                ComboSchemes.SelectedIndex = 0;
            }

            // add cycle definitions
            {
                if (_cycles.Length == 0)
                    throw new Exception("Found no life cycle definitions.");

                foreach (var cycle in _cycles)
                    ComboCycles.Items.Add(cycle.Name);

                ComboCycles.SelectedIndex = 0;
            }
        }

        // Called on opening and clicking the "Reset" button.
        private void Reset()
        {
            var stamp = DateTime.UtcNow.ToString("yyMMdd_hhmmss");

            TextSymbolReleaseFolder.Text = $"Symbols {stamp}";
            TextFootprintReleaseFolder.Text = $"Footprints {stamp}";
            TextComponentReleaseFolder.Text = $"Components {stamp}";

            TextSymbolItemName.Text = $"SYM {stamp}";
            TextFootprintItemName.Text = $"PCC {stamp}";
            TextComponentItemName.Text = $"CMP {stamp}";
        }

        // Called on clicking the "Reset" button.
        private void ClickReset(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        // Called on clicking the "Release" button.
        private void ClickRelease(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                try
                {
                    ReleaseComponent();
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    App.ShowException(ex);
                }
            }
        }

        /// <summary>
        /// Uploads folder files.
        /// </summary>
        private static async Task<List<DesReleaseComponentFileInput>> UploadFilesAsync(string root)
        {
            var result = new List<DesReleaseComponentFileInput>();
            var uploadUrl = App.FilesClient.BaseAddress.AbsoluteUri + "/File/Upload";
            foreach (var file in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
            {
                using (var form = new MultipartFormDataContent())
                {
                    using (var fileContent = new ByteArrayContent(File.ReadAllBytes(file)))
                    {
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                        form.Add(fileContent, "file", Path.GetFileName(file));

                        // post form
                        var response = await App.FilesClient.PostAsync(uploadUrl, form);
                        var content = await response.Content.ReadAsStringAsync();
                        response.EnsureSuccessStatusCode();

                        var relativePath = file.Substring(root.Length).TrimStart('\\');
                        result.Add(new DesReleaseComponentFileInput { FileId = content, RelativePath = relativePath });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Calls release component with the current data.
        /// </summary>
        void ReleaseComponent()
        {
            // upload files
            var symbolDirectory = App.ResolveDirectory(TextSymbolDataFolder.Text);
            var footprintDirectory = App.ResolveDirectory(TextFootprintDataFolder.Text);
            List<DesReleaseComponentFileInput> symbolUploads = null;
            List<DesReleaseComponentFileInput> footprintUploads = null;
            Task.Run(async () =>
            {
                symbolUploads = await UploadFilesAsync(symbolDirectory);
                footprintUploads = await UploadFilesAsync(footprintDirectory);
            }).Wait();

            var input = new DesReleaseComponentInput
            {
                WorkspaceUrl = _workspaceUrl,
                SymbolReleaseFolder = TextSymbolReleaseFolder.Text,
                SymbolItemName = TextSymbolItemName.Text,
                SymbolFiles = symbolUploads,
                FootprintReleaseFolder = TextFootprintReleaseFolder.Text,
                FootprintItemName = TextFootprintItemName.Text,
                FootprintFiles = footprintUploads,
                ComponentReleaseFolder = TextComponentReleaseFolder.Text,
                ComponentItemName = TextComponentItemName.Text,
                Parameters = new DesRevisionParameterInput[]
                {
                        new DesRevisionParameterInput { Name = "Parameter1", Value = TextParameter1.Text },
                        new DesRevisionParameterInput { Name = "Parameter2", Value = TextParameter2.Text },
                },
                RevisionNamingSchemeId = _schemes[ComboSchemes.SelectedIndex].RevisionNamingSchemeId,
                LifeCycleDefinitionId = _cycles[ComboCycles.SelectedIndex].LifeCycleDefinitionId,
            };

            Task.Run(async () =>
            {
                var res = await App.Client.ReleaseComponent.ExecuteAsync(input);
                ClientHelper.EnsureNoErrors(res);
            }).Wait();
        }
    }
}
