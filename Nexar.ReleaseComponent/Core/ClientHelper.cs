using StrawberryShake;
using System;
using System.Text;

namespace Nexar.ReleaseComponent
{
    public static class ClientHelper
    {
        /// <summary>
        /// Checks the operation result for no errors.
        /// </summary>
        public static void EnsureNoErrors(IOperationResult result)
        {
            if (result.Errors.Count > 0)
            {
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
}
