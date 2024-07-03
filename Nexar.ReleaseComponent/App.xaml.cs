using Nexar.Client;
using Nexar.Client.Login;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Nexar.ReleaseComponent
{
    public partial class App : Application
    {
        public static NexarClient Client { get; private set; }
        public static HttpClient FilesClient { get; private set; }
        public static IReadOnlyList<IMyWorkspace> Workspaces { get; private set; }

        public static string NexarToken { get; private set; }

        /// <summary>
        /// Run this as a task after the window is shown.
        /// </summary>
        /// <remarks>
        /// Why not before as it used to be. If a user does not login and maybe closes the login page,
        /// then the application is running waiting for the login. The user may not know about it.
        /// So, with the window shown the user is aware of it and may close.
        /// </remarks>
        public static async Task LoginAsync()
        {
            try
            {
                // login and get the token
                if (Config.Authority.StartsWith("http"))
                {
                    var clientId = Environment.GetEnvironmentVariable("NEXAR_CLIENT_ID") ?? throw new InvalidOperationException("Please set environment 'NEXAR_CLIENT_ID'");
                    var clientSecret = Environment.GetEnvironmentVariable("NEXAR_CLIENT_SECRET") ?? throw new InvalidOperationException("Please set environment 'NEXAR_CLIENT_SECRET'");
                    var login = await LoginHelper.LoginAsync(
                        clientId,
                        clientSecret,
                        new string[] { "user.access", "design.domain" },
                        Config.Authority);
                    NexarToken = login.AccessToken;
                }
                else
                {
                    NexarToken = Config.Authority;
                }

                // configure files client
                FilesClient = new HttpClient { BaseAddress = new Uri(Config.FilesEndpoint) };
                FilesClient.DefaultRequestHeaders.Add("token", NexarToken);
            }
            catch (Exception ex)
            {
                ShowException(ex);
                Environment.Exit(1);
            }
        }

        public static async Task LoadWorkspacesAsync()
        {
            try
            {
                Client = NexarClientFactory.CreateClient(Config.ApiEndpoint, NexarToken);
                Workspaces = await Client.GetWorkspacesAsync();
            }
            catch (Exception ex)
            {
                ShowException(ex);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Resolves and tests the directory path.
        /// </summary>
        public static string ResolveDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new Exception("Data folder path must not be empty.");

            var result = Path.IsPathRooted(path) ? path : Path.Combine(Path.GetDirectoryName(typeof(App).Assembly.Location), path);
            if (!Directory.Exists(result))
                throw new Exception($"Directory not found: {result}");

            return result;
        }

        public static void ShowException(Exception ex)
        {
            if (ex is AggregateException aex && aex.InnerExceptions.Count == 1)
                ex = aex.InnerExceptions[0];

            var message = $"{ex.Message}\n\n{ex}";
            MessageBox.Show(message, Config.MyTitle, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowException(e.Exception);
            e.Handled = true;
        }
    }
}
