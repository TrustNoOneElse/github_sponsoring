using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

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
            /*var secretFromEnv = Environment.GetEnvironmentVariable("GITHUB_WEBHOOK_SECRET");
            if (secretFromEnv == null)
            {
                _logger.LogCritical("GITHUB_WEBHOOK_SECRET is not set, please set it in the environment variables");
                return false;
            }*/
            var secret = Encoding.ASCII.GetBytes("Hackpass09!");
            // verify github signature using the secret
            /**payload = JToken.ReadFrom(new JsonTextReader(new StringReader(payload))).ToString(Formatting.None);
            var hash = new HMACSHA256(secret).ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = SHA_IDENTIFIER + BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            _logger.LogDebug($"Computed signature: {computedSignature}");
            return sha256Header.Equals(computedSignature, StringComparison.OrdinalIgnoreCase);

            using (var sha = new HMACSHA256(secret))
            {
                var nonFormattedJsonPayload = JToken.ReadFrom(new JsonTextReader(new StringReader(payload))).ToString(Formatting.None);
                var computedHash = sha.ComputeHash(Encoding.UTF8.GetBytes(nonFormattedJsonPayload));
                _logger.LogDebug($"Verifying signature for payload with hash: {BitConverter.ToString(computedHash).Replace("-", "").ToLower()}");
                _logger.LogDebug($"Verifying signature for payload with signature: {signature}");
                return BitConverter.ToString(computedHash).Replace("-", "").ToLower() == signature;
            }*/
            using (var sha = new HMACSHA256(secret))
            {
                var payloadBytes = Encoding.ASCII.GetBytes(payload);

                var hash = sha.ComputeHash(payloadBytes);

                var hashString = ToHexString(hash);

                if (hashString.Equals(signature))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static string ToHexString(byte[] bytes)
    {
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            builder.AppendFormat("{0:x2}", b);
        }

        return builder.ToString();
    }
}