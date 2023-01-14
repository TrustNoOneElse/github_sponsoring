namespace GithubSponsorsWebhook.Dtos;
#pragma warning disable IDE1006 // its a dto

public struct TierDto
{
    public string id { get; set; }
    public DateTime tierSelectedAt { get; set; }
    public string name { get; set; }
    public int monthlyPriceInCents { get; set; }
    public bool isOneTime { get; set; }
    public bool isCustomAmount { get; set; }
    public ClosestTierDto? closestLesserValueTier { get; set; }
}
#pragma warning restore IDE1006 //  its a dto
