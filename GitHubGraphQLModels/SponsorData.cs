namespace GithubSponsorsWebhook.GitHubGraphQLModels;

public struct SponsorData
{
    public GitHubEntity? user { get; set; }
    public GitHubEntity? organization { get; set; }
}