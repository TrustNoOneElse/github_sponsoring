namespace GithubSponsorsWebhook.GitHubModels;

public struct ChangesTierRoot
{
    public ChangesTier tier { get; set; }
}

public struct ChangesTier
{
    public GitHubTier from { get; set; }
}

public struct ChangesPrivacyLevelRoot
{
    public ChangesPrivacyLevel privacy_level { get; set; }
}

public struct ChangesPrivacyLevel
{
    public string from { get; set; }
}
