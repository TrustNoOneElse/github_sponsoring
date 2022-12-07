
using github_sponsors_webhook.Database;
using GithubSponsorsWebhook.Database.Models;
using GithubSponsorsWebhook.Models;
using NodaTime;
using NodaTime.Extensions;

namespace GithubSponsorsWebhook.Services;

public class SponsorshipService : ISponsorshipService
{
    private readonly ILiteDbSponsorService _sponsorService;
    private readonly ILogger<SponsorshipService> _logger;
    public SponsorshipService(ILiteDbSponsorService context, ILogger<SponsorshipService> logger)
    {
        _sponsorService = context;
        _logger = logger;
    }
    /**
    * <inheritdoc/>
    */
    public void ExecuteCronJob()
    {
        var sponsorsCollection = _sponsorService.FindAll();
        var activeSponsors = sponsorsCollection.FindAll(sponsor => sponsor.IsSponsor);
        var hasChanges = false;
        foreach (var sponsor in activeSponsors)
        {
            if (sponsor != null && sponsor != default && CanRecalculate(sponsor.CurrentTier))
            {
                var tier = sponsor.CurrentTier;
                sponsor.TotalSpendInCent += tier.MonthlyPriceInCent;
                sponsor.TotalSpendInDollar += tier.MonthlyPriceInDollar;
                tier.CurrentCalculatedIntervalInMonth += 1; // indicate that this month has been calculated
                hasChanges = true;
            }
        }
        if (hasChanges)
            _sponsorService.UpdateSponsors(activeSponsors);
    }

    /**
    * Checks if the last calculated interval is older than the tier's interval.
    * If so, the tier will be recalculated.
    */
    private static bool CanRecalculate(Tier t)
    {
        Period period = Period.Between(t.LatestChangeAt.ToLocalDateTime(), DateTime.Now.ToLocalDateTime(), PeriodUnits.Months);
        return t.CurrentCalculatedIntervalInMonth < period.Months;
    }
    /**
    * <inheritdoc/>
    */
    public void ProcessSponsorEvent(SponsorEvent sponsorEvent)
    {
        if (sponsorEvent.Action == GitHubAction.CREATED)
        {
            CreateSponsor(sponsorEvent);
        }
        else if (sponsorEvent.Action == GitHubAction.TIER_CHANGED)
        {
            UpdateSponsor(sponsorEvent);
        }
        else if (sponsorEvent.Action == GitHubAction.CANCELLED)
        {
            DeleteSponsor(sponsorEvent);
        }
    }

    private void DeleteSponsor(SponsorEvent sponsorEvent)
    {
        var sponsor = _sponsorService.FindSponsor(sponsorEvent.DatabaseId, sponsorEvent.GithubType);
        if (sponsor != null && sponsor != default)
        {
            var tier = sponsor.CurrentTier;
            if (tier != null && tier != default)
            {
                tier.IsCancelled = true;
                _sponsorService.UpdateSponsor(ref sponsor);
            }
        }
    }

    /**
    * Updates the Sponsor by DatabaseId, because login can be changed from the user
    * @param sponsorEvent contains the new tier information
    */
    private void UpdateSponsor(SponsorEvent sponsorEvent)
    {
        var sponsor = _sponsorService.FindSponsor(sponsorEvent.DatabaseId, sponsorEvent.GithubType);
        if (sponsor != null && sponsor != default)
        {
            // Login Name can change
            if (sponsor.LoginName != sponsorEvent.LoginName)
            {
                sponsor.LoginName = sponsorEvent.LoginName;
            }
            var tier = sponsor.CurrentTier;
            
            if (tier != null && tier != default)
            {
                sponsor.Tiers ??= new List<Tier>();
                sponsor.Tiers.Add(tier);
                var newTier = new Tier();
                newTier.MonthlyPriceInCent = sponsorEvent.Tier.monthly_price_in_cents;
                newTier.Name = sponsorEvent.Tier.name;
                newTier.MonthlyPriceInDollar = sponsorEvent.Tier.monthly_price_in_dollars;
                newTier.LatestChangeAt = sponsorEvent.Tier.created_at;
                newTier.CurrentCalculatedIntervalInMonth = -1;
                sponsor.CurrentTier = newTier;
            }
            // API doesnt clearly say it, but they could change from one time payment to an actual tier
            else
            {
                tier = new Tier
                {
                    MonthlyPriceInCent = sponsorEvent.Tier.monthly_price_in_cents,
                    Name = sponsorEvent.Tier.name,
                    MonthlyPriceInDollar = sponsorEvent.Tier.monthly_price_in_dollars,
                    LatestChangeAt = sponsorEvent.Tier.created_at,
                    CurrentCalculatedIntervalInMonth = -1,
                };
                sponsor.CurrentTier = tier;
            }
            _sponsorService.UpdateSponsor(ref sponsor);
        }
    }

    /**
    * Process a new sponsor event and create the Sponsor Entity as well as the Tier Entity
    * @param sponsorEvent the event to process
*/
    private void CreateSponsor(SponsorEvent sponsorEvent)
    {
        var sponsor = _sponsorService.FindSponsor(sponsorEvent.DatabaseId, sponsorEvent.GithubType);
        if (sponsor == null || sponsor == default)
        {
            sponsor = new Sponsor
            {
                DatabaseId = sponsorEvent.DatabaseId,
                LoginName = sponsorEvent.LoginName,
                GithubType = sponsorEvent.GithubType,
                TotalSpendInCent = 0,
                TotalSpendInDollar = 0,
                FirstSponsoredAt = DateTime.Now
            };
            _sponsorService.AddSponsor(sponsor);
        }
        else
        {
            sponsor.LoginName = sponsorEvent.LoginName;
        }
        // if its a one time payment, we calculate the total spend here and we create a OneTimePayment object for it
        if (sponsorEvent.Tier.is_one_time)
        {
            sponsor.TotalSpendInCent += sponsorEvent.Tier.monthly_price_in_cents;
            sponsor.TotalSpendInDollar += sponsorEvent.Tier.monthly_price_in_dollars;
            var oneTimePayment = new OneTimePayment
            {
                TotalInCent = sponsorEvent.Tier.monthly_price_in_cents,
                TotalInDollar = sponsorEvent.Tier.monthly_price_in_dollars,
                CreatedAt = sponsorEvent.Tier.created_at
            };
            sponsor.Payments ??= new List<OneTimePayment>();
            sponsor.Payments.Add(oneTimePayment);
        }
        else
        {
            if (sponsor.CurrentTier != null && sponsor.CurrentTier != default)
            {
                sponsor.CurrentTier.IsCancelled = true;
                sponsor.Tiers ??= new List<Tier>();
                sponsor.Tiers.Add(sponsor.CurrentTier);
                _sponsorService.UpdateSponsor(ref sponsor);
            }
            var tier = new Tier
            {
                Name = sponsorEvent.Tier.name,
                MonthlyPriceInCent = sponsorEvent.Tier.monthly_price_in_cents,
                MonthlyPriceInDollar = sponsorEvent.Tier.monthly_price_in_dollars,
                LatestChangeAt = sponsorEvent.CreatedAt,
                CurrentCalculatedIntervalInMonth = -1,
            };
            sponsor.CurrentTier = tier;
        }
        _sponsorService.UpdateSponsor(ref sponsor);
    }
}