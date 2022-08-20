namespace GithubSponsorsWebhook.Models;

public enum GitHubAction : byte
{
    /**
    * The user has become a sponsor and the Payment was successful.
    */
    CREATED,
    /**
    * The user has changed his tier and the changes have applied.
    */
    TIER_CHANGED,
    /**
    * The user has cancelled his sponsorship.
    */
    CANCELLED
}