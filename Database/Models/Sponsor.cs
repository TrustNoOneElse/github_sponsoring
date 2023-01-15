namespace GithubSponsorsWebhook.Database.Models;

using GithubSponsorsWebhook.Models;
using LiteDB;

public class Sponsor
{
    public int Id { get; set; }
    public string? LoginName { get; set; }

    public DateTime FirstSponsoredAt { get; set; }

    public int DatabaseId { get; set; }

    public GithubType GithubType { get; set; }

    public int TotalSpendInCent { get; set; }

    public Tier? CurrentTier { get; set; }

    public PatreonMigration? PatreonMigration { get; set; }

    public List<Tier>? Tiers { get; set; }

    public List<OneTimePayment>? Payments { get; set; }
    
    [BsonIgnore]
    public bool IsSponsor => CurrentTier != null && !CurrentTier.IsCancelled;

}