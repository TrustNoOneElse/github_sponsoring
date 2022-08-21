namespace GithubSponsorsWebhook.GitHubGraphQLModels;

public struct Sponsorship
{
    public Tier tier { get; set; }
    public string id { get; set; }
    public bool isOneTimePayment { get; set; }
    public DateTime tierSelectedAt { get; set; }

}