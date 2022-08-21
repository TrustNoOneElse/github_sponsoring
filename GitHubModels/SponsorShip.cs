namespace GithubSponsorsWebhook.GitHubModels;

public struct Sponsorship
{
    public string node_id { get; set; }
    public DateTime created_at { get; set; }
    public GitHubEntity sponsorable { get; set; }
    public GitHubEntity sponsor { get; set; }
    public string privacy_level { get; set; }
    public GitHubTier tier { get; set; }
}