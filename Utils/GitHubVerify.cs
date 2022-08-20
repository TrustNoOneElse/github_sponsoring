using System.Security.Cryptography;
using System.Text;

namespace GithubSponsorsWebhook.Utils;

public static class GitHubVerify
{
    public static bool VerifySignature(string sha256Header, string payload, ILogger _logger)
    {
        if (sha256Header.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
        {
            var signature = sha256Header.Substring("sha256=".Length);
            var secretFromEnv = Environment.GetEnvironmentVariable("GITHUB_WEBHOOK_SECRET");
            if (secretFromEnv == null)
            {
                _logger.LogCritical("GITHUB_WEBHOOK_SECRET is not set, please set it in the environment variables");
                return false;
            }
            var secret = Encoding.ASCII.GetBytes(secretFromEnv);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using (var sha = new HMACSHA256(secret))
            {
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

    private static string ToHexString(byte[] bytes)
    {
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            builder.AppendFormat("{0:x2}", b);
        }

        return builder.ToString();
    }
}