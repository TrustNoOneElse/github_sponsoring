namespace GithubSponsorsWebhook.GitHubGraphQLModels;

public struct SponsorEntity
{
    public string __typename;
    public string login { get; set; }

    public string name { get; set; }
    public int databaseId { get; set; }

    public string email { get; set; }
}