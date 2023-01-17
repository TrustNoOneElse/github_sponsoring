namespace GithubSponsorsWebhook.Models;

public class SponsorSwitchEvent
{
    public int DatabaseId { get; set; }
    public string LoginName { get; set; }
    public Guid PatreonId { get; set; }
    public GithubType GithubType { get; set; }
    public long TotalSpendInCentInOtherInstance { get; set; }
}

