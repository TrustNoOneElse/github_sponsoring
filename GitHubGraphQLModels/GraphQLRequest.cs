namespace GithubSponsorsWebhook.GitHubGraphQLModels;

public struct GraphQLRequest
{
    public string query { get; set; }
    public Variables? variables { get; set; }
}
