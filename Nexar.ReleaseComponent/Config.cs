using System;

namespace Nexar.ReleaseComponent
{
    /// <summary>
    /// App configuration.
    /// </summary>
    static class Config
    {
        public const string MyTitle = "Nexar.ReleaseComponent";

        public static string Authority { get; private set; }
        public static string ApiEndpoint { get; private set; }
        public static string FilesEndpoint { get; private set; }

        static Config()
        {
            Authority = Environment.GetEnvironmentVariable("NEXAR_AUTHORITY") ?? "https://identity.nexar.com";
            ApiEndpoint = Environment.GetEnvironmentVariable("NEXAR_API_URL") ?? "https://api.nexar.com/graphql";
            FilesEndpoint = Environment.GetEnvironmentVariable("NEXAR_FILES_URL") ?? "https://files.nexar.com";
        }
    }
}
