using GithubSponsorsWebhook.GitHubModels;

namespace GithubSponsorsWebhook.Models;

public struct SponsorEvent
{
    public GitHubAction Action { get; set; }
    public int DatabaseId { get; set; }
    public string LoginName { get; set; }
    public DateTime CreatedAt { get; set; }
    public GithubType GithubType { get; set; }
    public GitHubTier Tier { get; set; }
    public GitHubTier? ChangedFromTier { get; set; }

    public bool IsCancelled => Action == GitHubAction.CANCELLED;
}
