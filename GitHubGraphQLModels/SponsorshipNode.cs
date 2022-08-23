namespace GithubSponsorsWebhook.GitHubGraphQLModels;

public struct SponsorshipNode
{
    public Tier tier { get; set; }
    public string id { get; set; }
    public bool isOneTimePayment { get; set; }
    public DateTime tierSelectedAt { get; set; }

    public SponsorEntity sponsorEntity { get; set; }
}