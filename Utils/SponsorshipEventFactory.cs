using System.Text.Json;
using GithubSponsorsWebhook.GitHubModels;
using GithubSponsorsWebhook.Models;

namespace GithubSponsorsWebhook.Utils;

/**
* This class is used to create a SponsorEvent from a GitHub webhook payload. And map it in our SponsorEvent type which is used in the rest of the application.
*/
public static class SponsorshipEventFactory
{
    public static SponsorEvent? CreateSponsorEvent(JsonElement root)
    {
        var action = root.GetProperty("action").GetString();
        if (action == "created")
        {
            CreatedSponsorEvent sponsorEvent = JsonSerializer.Deserialize<CreatedSponsorEvent>(root.GetRawText());
            return new SponsorEvent
            {
                Action = GitHubAction.CREATED,
                LoginName = sponsorEvent.sponsorship.sponsor.login,
                DatabaseId = sponsorEvent.sponsorship.sponsor.id,
                GithubType = sponsorEvent.sponsorship.sponsor.type == "User" ? GithubType.USER : GithubType.ORGANIZATION,
                Tier = sponsorEvent.sponsorship.tier,
                ChangedFromTier = null,
                CreatedAt = sponsorEvent.sponsorship.created_at
            };
        }
        else if (action == "tier_changed_pending")
        {
            TierChangedPendingSponsorEvent tierChangedPendingSponsorEvent = JsonSerializer.Deserialize<TierChangedPendingSponsorEvent>(root.GetRawText());
            // Not interested in this event for now.
            return null;
        }
        else if (action == "tier_changed")
        {
            TierChangedSponsorEvent sponsorEvent = JsonSerializer.Deserialize<TierChangedSponsorEvent>(root.GetRawText());
            return new SponsorEvent
            {
                Action = GitHubAction.TIER_CHANGED,
                LoginName = sponsorEvent.sponsorship.sponsor.login,
                DatabaseId = sponsorEvent.sponsorship.sponsor.id,
                GithubType = sponsorEvent.sponsorship.sponsor.type == "User" ? GithubType.USER : GithubType.ORGANIZATION,
                Tier = sponsorEvent.sponsorship.tier,
                ChangedFromTier = sponsorEvent.changes.tier.from,
                CreatedAt = sponsorEvent.sponsorship.created_at
            };
        }
        else if (action == "edited")
        {
            EditedSponsorEvent editedSponsorEvent = JsonSerializer.Deserialize<EditedSponsorEvent>(root.GetRawText());
            // not interested in this event now. We dont care about privacy level changes.
            return null;
        }
        else if (action == "pending_cancellation")
        {
            PendingCancellationSponsorEvent pendingCancellationSponsorEvent = JsonSerializer.Deserialize<PendingCancellationSponsorEvent>(root.GetRawText());
            // not interested in this event for now.
            return null;
        }
        else if (action == "cancelled")
        {
            CancelledSponsorEvent sponsorEvent = JsonSerializer.Deserialize<CancelledSponsorEvent>(root.GetRawText());
            return new SponsorEvent
            {
                Action = GitHubAction.CANCELLED,
                LoginName = sponsorEvent.sponsorship.sponsor.login,
                DatabaseId = sponsorEvent.sponsorship.sponsor.id,
                GithubType = sponsorEvent.sponsorship.sponsor.type == "User" ? GithubType.USER : GithubType.ORGANIZATION,
                Tier = sponsorEvent.sponsorship.tier,
                ChangedFromTier = null,
                CreatedAt = sponsorEvent.sponsorship.created_at
            };
        }
        return null;
    }
}