namespace GithubSponsorsWebhook.Models;

public class SponsorSwitchEvent
{
    public int DatabaseId { get; set; }
    public string LoginName { get; set; }
    public GithubType GithubType { get; set; }
    public int TotalSpendInCentInOtherInstance { get; set; }
}

