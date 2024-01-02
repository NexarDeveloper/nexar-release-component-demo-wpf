using Microsoft.Extensions.DependencyInjection;
using StrawberryShake;
using StrawberryShake.Serialization;
using System;
using System.Text;

namespace Nexar.Client
{
    public static class NexarClientFactory
    {
        /// <summary>
        /// Creates the Nexar GraphQL client.
        /// </summary>
        public static NexarClient CreateClient(string nexarApiUrl, string nexarAccessToken)
        {
            var nexarApiUri = new Uri(nexarApiUrl);

            var serviceCollection = new ServiceCollection();
            serviceCollection
                // Add serializer for operations using Map.
                .AddSerializer(new JsonSerializer("Map"))
                // Add the Nexar GraphQL client.
                .AddNexarClient()
                // Configure HTTP requests.
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = nexarApiUri;
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {nexarAccessToken}");
                });

            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider.GetRequiredService<NexarClient>();
        }

        /// <summary>
        /// Checks the operation result for no errors.
        /// </summary>
        public static void AssertNoErrors(this IOperationResult result)
        {
            if (result.Errors.Count == 0)
                return;

            var sb = new StringBuilder();
            foreach (var error in result.Errors)
            {
                sb.AppendLine($"ERROR: Code: {error.Code}; Message: {error.Message}");
                if (error.Extensions != null)
                {
                    foreach (var kv in error.Extensions)
                        sb.AppendLine($"{kv.Key}: {kv.Value}");
                }
            }

            var message = sb.ToString();
            throw new InvalidOperationException(message);
        }
    }
}
