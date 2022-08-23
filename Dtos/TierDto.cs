namespace GithubSponsorsWebhook.Dtos;

public struct TierDto
{
    public string id { get; set; }
    public DateTime tierSelectedAt { get; set; }
    public string name { get; set; }
    public int monthlyPriceInCents { get; set; }
    public int monthlyPriceInDollar { get; set; }
    public bool isOneTime { get; set; }
    public bool isCustomAmount { get; set; }
    public ClosestTierDto? closestLesserValueTier { get; set; }
}