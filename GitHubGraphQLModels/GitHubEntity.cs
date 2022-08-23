namespace GithubSponsorsWebhook.GitHubGraphQLModels;

public struct GitHubEntity
{
    public bool? isSponsoringViewer { get; set; }
    public bool? viewerIsSponsoring { get; set; }

    public string login { get; set; }

    public string name { get; set; }
    public int databaseId { get; set; }

    public string email { get; set; }

    public Sponsorship? sponsorshipForViewerAsSponsorable { get; set; }
}