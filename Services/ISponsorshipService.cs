using GithubSponsorsWebhook.Models;

namespace GithubSponsorsWebhook.Services;

public interface ISponsorshipService
{
    /**
    * This method is called when a new event is received and the database needs to be updated.
    * @param event The event that was received and needs to be processed.
    */
    public void ProcessSponsorEvent(SponsorEvent sponsorEvent);

    /**
    * This method is executed every few hours and checks if we need to calculate the new tiers of a sponsor.
    */
    public void ExecuteCronJob();
}