using System;

namespace Nexar.ReleaseComponent
{
    /// <summary>
    /// App configuration. Different modes and endpoints are used for internal development.
    /// Clients usually use nexar.com endpoints.
    /// </summary>
    static class Config
    {
        public const string MyTitle = "Nexar.ReleaseComponent";

        public static NexarMode NexarMode { get; }
        public static string Authority { get; }
        public static string ApiEndpoint { get; set; }
        public static string FilesEndpoint { get; set; }

        static Config()
        {
            // default mode
            var mode = Environment.GetEnvironmentVariable("NEXAR_MODE") ?? "Prod";
            NexarMode = (NexarMode)Enum.Parse(typeof(NexarMode), mode, true);

            // init mode related data
            switch (NexarMode)
            {
                case NexarMode.Prod:
                    Authority = "https://identity.nexar.com/";
                    ApiEndpoint = "https://api.nexar.com/graphql/";
                    FilesEndpoint = "https://files.nexar.com/";
                    break;
                case NexarMode.Dev:
                    Authority = "https://identity.nexar.com/";
                    ApiEndpoint = "https://api.nexar.com/graphql/";
                    FilesEndpoint = "https://files.nexar.com/";
                    break;
                default:
                    throw new Exception();
            }
        }
    }

    public enum NexarMode
    {
        Prod,
        Dev
    }
}
