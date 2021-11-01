using Nexar.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Nexar.ReleaseComponent
{
    partial class ReleaseWindow : Window
    {
        private static string _workspaceUrl;
        private static string _lastWorkspaceUrl;
        private static IMyRevisionNamingScheme[] _schemes;
        private static IMyLifeCycleDefinition[] _cycles;
        private static IMyRevisionNamingScheme _schemeSymbol;
        private static IMyRevisionNamingScheme _schemeFootprint;
        private static IMyRevisionNamingScheme _schemeComponent;
        private static IMyLifeCycleDefinition _cycleSymbol;
        private static IMyLifeCycleDefinition _cycleFootprint;
        private static IMyLifeCycleDefinition _cycleComponent;

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
                        return res.Data;
                    }).Result;

                    // current combo data
                    _schemes = data.DesRevisionNamingSchemes.OrderBy(x => x.Name).ToArray();
                    if (_schemes.Length == 0)
                        throw new Exception("Found no revision naming schemes.");
                    _cycles = data.DesLifeCycleDefinitions.OrderBy(x => x.Name).ToArray();
                    if (_cycles.Length == 0)
                        throw new Exception("Found no life cycle definitions.");

                    // defaults
                    _schemeSymbol = data.SymbolScheme;
                    _schemeFootprint = data.FootprintScheme;
                    _schemeComponent = data.ComponentScheme;
                    _cycleSymbol = data.SymbolLifeCycle;
                    _cycleFootprint = data.FootprintLifeCycle;
                    _cycleComponent = data.ComponentLifeCycle;

                    // set the current workspace
                    _lastWorkspaceUrl = _workspaceUrl;
                }
            }

            // naming scheme combos
            void PopulateNamingSchemes(ComboBox combo, IMyRevisionNamingScheme select)
            {
                foreach (var scheme in _schemes)
                    combo.Items.Add(scheme.Name);

                combo.SelectedIndex = Array.FindIndex(_schemes, x => x.RevisionNamingSchemeId == select.RevisionNamingSchemeId);
            }
            PopulateNamingSchemes(ComboComponentSchemes, _schemeComponent);
            PopulateNamingSchemes(ComboSymbolSchemes, _schemeSymbol);
            PopulateNamingSchemes(ComboFootprintSchemes, _schemeFootprint);

            // cycle definition combos
            void PopulateLifeCycles(ComboBox combo, IMyLifeCycleDefinition select)
            {
                foreach (var cycle in _cycles)
                    combo.Items.Add(cycle.Name);

                combo.SelectedIndex = Array.FindIndex(_cycles, x => x.LifeCycleDefinitionId == select.LifeCycleDefinitionId);
            }
            PopulateLifeCycles(ComboComponentCycles, _cycleComponent);
            PopulateLifeCycles(ComboSymbolCycles, _cycleSymbol);
            PopulateLifeCycles(ComboFootprintCycles, _cycleFootprint);
        }

        // Called on opening and clicking the "Reset" button.
        private void Reset()
        {
            var stamp = DateTime.UtcNow.ToString("yyMMdd_HHmmss");

            TextComponentReleaseFolder.Text = $"Components {stamp}";
            TextComponentItemName.Text = $"CMP {stamp}";
            TextComponentComment.Text = $"CMP {stamp}";
            TextComponentDescription.Text = $"Description {stamp}";

            TextSymbolReleaseFolder.Text = $"Symbols {stamp}";
            TextSymbolItemName.Text = $"SYM {stamp}";

            TextFootprintReleaseFolder.Text = $"Footprints {stamp}";
            TextFootprintItemName.Text = $"PCC {stamp}";
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
                ComponentReleaseFolder = TextComponentReleaseFolder.Text,
                ComponentItemName = TextComponentItemName.Text,
                ComponentComment = TextComponentComment.Text,
                ComponentDescription = TextComponentDescription.Text,
                ComponentRevisionNamingSchemeId = _schemes[ComboComponentSchemes.SelectedIndex].RevisionNamingSchemeId,
                ComponentLifeCycleDefinitionId = _cycles[ComboComponentCycles.SelectedIndex].LifeCycleDefinitionId,
                Parameters = new DesRevisionParameterInput[]
                {
                        new DesRevisionParameterInput { Name = "Parameter1", Value = TextParameter1.Text },
                        new DesRevisionParameterInput { Name = "Parameter2", Value = TextParameter2.Text },
                },
                SymbolReleaseFolder = TextSymbolReleaseFolder.Text,
                SymbolItemName = TextSymbolItemName.Text,
                SymbolFiles = symbolUploads,
                SymbolRevisionNamingSchemeId = _schemes[ComboSymbolSchemes.SelectedIndex].RevisionNamingSchemeId,
                SymbolLifeCycleDefinitionId = _cycles[ComboSymbolCycles.SelectedIndex].LifeCycleDefinitionId,
                FootprintReleaseFolder = TextFootprintReleaseFolder.Text,
                FootprintItemName = TextFootprintItemName.Text,
                FootprintFiles = footprintUploads,
                FootprintRevisionNamingSchemeId = _schemes[ComboFootprintSchemes.SelectedIndex].RevisionNamingSchemeId,
                FootprintLifeCycleDefinitionId = _cycles[ComboFootprintCycles.SelectedIndex].LifeCycleDefinitionId,
            };

            Task.Run(async () =>
            {
                var res = await App.Client.ReleaseComponent.ExecuteAsync(input);
                ClientHelper.EnsureNoErrors(res);
            }).Wait();
        }
    }
}
