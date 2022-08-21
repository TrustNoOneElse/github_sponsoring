namespace GithubSponsorsWebhook.GitHubGraphQLModels;

public struct ViewerResponse
{
    public ViewerData data { get; set; }
}

public struct ViewerData
{
    public Viewer viewer { get; set; }
}