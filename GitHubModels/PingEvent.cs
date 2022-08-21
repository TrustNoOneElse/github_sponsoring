namespace GithubSponsorsWebhook.GitHubModels;

public struct PingEvent
{
    public string zen { get; set; }
    public long hook_id { get; set; }
    public Hook hook { get; set; }
    public Repository repository { get; set; }
    public GitHubEntity sender { get; set; }
}