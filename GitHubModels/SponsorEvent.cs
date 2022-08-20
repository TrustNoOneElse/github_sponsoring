namespace GithubSponsorsWebhook.GitHubModels;

public struct CreatedSponsorEvent
{
    public Sponsorship sponsorship { get; set; }
    public string action { get; set; }
    public Person sender { get; set; }
}

public struct TierChangedPendingSponsorEvent
{
    public Sponsorship sponsorship { get; set; }
    public string action { get; set; }
    public ChangesTierRoot changes { get; set; }
    public DateTime effective_date { get; set; }
    public Person sender { get; set; }
}

public struct TierChangedSponsorEvent
{
    public Sponsorship sponsorship { get; set; }
    public string action { get; set; }
    public ChangesTierRoot changes { get; set; }
    public Person sender { get; set; }
}

public struct EditedSponsorEvent
{
    public Sponsorship sponsorship { get; set; }
    public string action { get; set; }
    public ChangesPrivacyLevelRoot changes { get; set; }
    public Person sender { get; set; }
}

public struct PendingCancellationSponsorEvent
{
    public Sponsorship sponsorship { get; set; }
    public string action { get; set; }
    public DateTime effective_date { get; set; }
    public Person sender { get; set; }
}

public struct CancelledSponsorEvent
{
    public Sponsorship sponsorship { get; set; }
    public string action { get; set; }
    public Person sender { get; set; }
}