namespace GithubSponsorsWebhook.GitHubGraphQLModels;
public struct Viewer
{
    public string login { get; set; }
    public string email { get; set; }
    public string name { get; set; }
    public int databaseId { get; set; }
}
