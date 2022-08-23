namespace GithubSponsorsWebhook.GitHubGraphQLModels;

public struct SponsorEntity
{
    public string __typename;
    public GitHubEntity? user;
    public GitHubEntity? organization;
}