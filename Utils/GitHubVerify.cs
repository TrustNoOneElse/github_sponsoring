using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GithubSponsorsWebhook.Utils;

public static class GitHubVerify
{
    private const string SHA_IDENTIFIER = "sha256=";

    public static bool VerifySignature(string sha256Header, string payload, ILogger _logger)
    {
        _logger.LogDebug($"Verifying signature for sha header: {sha256Header}");
        if (sha256Header.StartsWith(SHA_IDENTIFIER, StringComparison.OrdinalIgnoreCase))
        {
            var signature = sha256Header.Substring(SHA_IDENTIFIER.Length);
            var secretFromEnv = Environment.GetEnvironmentVariable("GITHUB_WEBHOOK_SECRET");
            if (secretFromEnv == null)
            {
                _logger.LogCritical("GITHUB_WEBHOOK_SECRET is not set, please set it in the environment variables");
                return false;
            }
            var secret = Encoding.UTF8.GetBytes(secretFromEnv);
            // verify github signature using the secret

            using (var sha = new System.Security.Cryptography.HMACSHA256(secret))
            {
                var nonFormattedJsonPayload = JToken.ReadFrom(new JsonTextReader(new StringReader(payload))).ToString(Formatting.None);
                var computedHash = sha.ComputeHash(Encoding.UTF8.GetBytes(nonFormattedJsonPayload));
                _logger.LogDebug($"Verifying signature for payload with hash: {BitConverter.ToString(computedHash).Replace("-", "").ToLower()}");
                _logger.LogDebug($"Verifying signature for payload with signature: {signature}");
                return BitConverter.ToString(computedHash).Replace("-", "").ToLower() == signature;
            }
        }
        return false;
    }
}