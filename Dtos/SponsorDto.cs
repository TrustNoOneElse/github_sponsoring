using GithubSponsorsWebhook.Models;

namespace GithubSponsorsWebhook.Dtos;
#pragma warning disable IDE1006 // its a dto

public struct SponsorDto
{
    public string login { get; set; }
    public string email { get; set; }
    public string name { get; set; }
    public GithubType entityType { get; set; }
    public TierDto tier { get; set; }
    public bool payedLifetime { get; set; }
    public bool payedMinimum { get; set; }
    public DateTime firstSponsoredAt { get; set; }
    public int totalSpendInCent { get; set; }
    public int databaseId { get; set; }

}
#pragma warning restore IDE1006 //  its a dto
