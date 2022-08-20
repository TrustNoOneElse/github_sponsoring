namespace GithubSponsorsWebhook.GitHubModels;

public struct Config
{
    public string content_type { get; set; }
    public string url { get; set; }
    public string insecure_ssl { get; set; }
}