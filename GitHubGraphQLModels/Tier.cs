namespace GithubSponsorsWebhook.GitHubGraphQLModels;

public struct Tier
{
    public string id { get; set; }
    public string name { get; set; }
    public bool isOneTime { get; set; }
    public int monthlyPriceInCents { get; set; }
    public int monthlyPriceInDollars { get; set; }
    public bool isCustomAmount { get; set; }
    public ClosestLesserValueTier? closestLesserValueTier { get; set; }
}

public struct ClosestLesserValueTier
{
    public string id { get; set; }
    public string name { get; set; }
}