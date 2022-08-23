
using GithubSponsorsWebhook.Database;
using GithubSponsorsWebhook.Database.Models;
using GithubSponsorsWebhook.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NodaTime;
using NodaTime.Extensions;

namespace GithubSponsorsWebhook.Services;

public class SponsorshipService : ISponsorshipService
{
    private readonly DatabaseContext _databaseContext;
    private readonly ILogger<SponsorshipService> _logger;
    public SponsorshipService(DatabaseContext context, ILogger<SponsorshipService> logger)
    {
        _databaseContext = context;
        _logger = logger;
    }
    /**
    * <inheritdoc/>
    */
    public void ExecuteCronJob()
    {
        var tiers = _databaseContext.Tiers.Where(t => !t.IsCancelled).Include(t => t.Sponsor).ToList();
        bool hasChanges = false;
        foreach (var tier in tiers)
        {
            if (CanRecalculate(tier))
            {
                var sponsor = tier.Sponsor;
                sponsor.TotalSpendInCent += tier.MonthlyPriceInCent;
                sponsor.TotalSpendInDollar += tier.MonthlyPriceInDollar;
                _databaseContext.Sponsors.Update(sponsor);
                tier.CurrentCalculatedIntervalInMonth += 1; // indicate that this month has been calculated
                _databaseContext.Tiers.Update(tier);
                hasChanges = true;
            }
        }
        if (hasChanges)
        {
            _databaseContext.SaveChanges();
        }
    }

    /**
    * Checks if the last calculated interval is older than the tier's interval.
    * If so, the tier will be recalculated.
    */
    private bool CanRecalculate(Tier t)
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
        var sponsor = _databaseContext.Sponsors.Where(s => s.DatabaseId == sponsorEvent.DatabaseId && s.GithubType == sponsorEvent.GithubType).Include(s => s.CurrentTier).First();
        if (sponsor != null)
        {
            var tier = sponsor.CurrentTier;
            if (tier != null)
            {
                tier.IsCancelled = true;
                _databaseContext.Tiers.Update(tier);
                _databaseContext.SaveChanges();
            }
        }
    }

    /**
    * Updates the Sponsor by DatabaseId, because login can be changed from the user
    * @param sponsorEvent contains the new tier information
    */
    private void UpdateSponsor(SponsorEvent sponsorEvent)
    {
        var sponsor = _databaseContext.Sponsors.Where(s => s.DatabaseId == sponsorEvent.DatabaseId && s.GithubType == sponsorEvent.GithubType).Include(s => s.CurrentTier).First();
        if (sponsor != null)
        {
            // Login Name can change
            if (sponsor.LoginName != sponsorEvent.LoginName)
            {
                sponsor.LoginName = sponsorEvent.LoginName;
                _databaseContext.Sponsors.Update(sponsor);
            }
            var tier = sponsor.CurrentTier;
            if (tier != null)
            {
                tier.MonthlyPriceInCent = sponsorEvent.Tier.monthly_price_in_cents;
                tier.Name = sponsorEvent.Tier.name;
                tier.MonthlyPriceInDollar = sponsorEvent.Tier.monthly_price_in_dollars;
                tier.LatestChangeAt = sponsorEvent.Tier.created_at;
                tier.CurrentCalculatedIntervalInMonth = -1;
                _databaseContext.Tiers.Update(tier);
            }
            // API doesnt clearly say it, but they could change from one time payment to an actual tier
            else
            {
                tier = new Tier
                {
                    SponsorId = sponsor.Id,
                    MonthlyPriceInCent = sponsorEvent.Tier.monthly_price_in_cents,
                    Name = sponsorEvent.Tier.name,
                    MonthlyPriceInDollar = sponsorEvent.Tier.monthly_price_in_dollars,
                    LatestChangeAt = sponsorEvent.Tier.created_at,
                    CurrentCalculatedIntervalInMonth = -1,
                };
                _databaseContext.Tiers.Add(tier);
            }
            _databaseContext.SaveChanges();
        }
    }

    /**
    * Process a new sponsor event and create the Sponsor Entity as well as the Tier Entity
    * @param sponsorEvent the event to process
*/
    private void CreateSponsor(SponsorEvent sponsorEvent)
    {
        Sponsor? sponsor;
        try
        {
            sponsor = _databaseContext.Sponsors.Where(s => s.DatabaseId == sponsorEvent.DatabaseId && (int)s.GithubType == (int)sponsorEvent.GithubType).First();
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Sponsor does not exist yet");
            sponsor = null;
        }
        EntityEntry<Sponsor> trackedSponsorEntity;
        if (sponsor == null)
        {
            sponsor = new Sponsor
            {
                DatabaseId = sponsorEvent.DatabaseId,
                LoginName = sponsorEvent.LoginName,
                GithubType = sponsorEvent.GithubType,
                TotalSpendInCent = 0,
                TotalSpendInDollar = 0,
                firstSponsoredAt = DateTime.Now
            };
            trackedSponsorEntity = _databaseContext.Sponsors.Add(sponsor);
        }
        else
        {
            sponsor.LoginName = sponsorEvent.LoginName;
            trackedSponsorEntity = _databaseContext.Sponsors.Update(sponsor);
        }
        // if its a one time payment, we calculate the total spend here
        if (sponsorEvent.Tier.is_one_time)
        {
            sponsor.TotalSpendInCent += sponsorEvent.Tier.monthly_price_in_cents;
            sponsor.TotalSpendInDollar += sponsorEvent.Tier.monthly_price_in_dollars;
        }
        _databaseContext.SaveChanges();
        sponsor = trackedSponsorEntity.Entity;
        // if its a one time payment, no reason to create a tier for it
        if (sponsorEvent.Tier.is_one_time)
        {
            var oneTimePayment = new OneTimePayment
            {
                SponsorId = trackedSponsorEntity.Entity.Id,
                TotalInCent = sponsorEvent.Tier.monthly_price_in_cents,
                TotalInDollar = sponsorEvent.Tier.monthly_price_in_dollars,
                CreatedAt = sponsorEvent.Tier.created_at
            };
            _databaseContext.OneTimePayments.Add(oneTimePayment);
        }
        else
        {
            var tier = new Tier
            {
                Name = sponsorEvent.Tier.name,
                MonthlyPriceInCent = sponsorEvent.Tier.monthly_price_in_cents,
                MonthlyPriceInDollar = sponsorEvent.Tier.monthly_price_in_dollars,
                LatestChangeAt = sponsorEvent.CreatedAt,
                SponsorId = sponsor.Id,
                CurrentCalculatedIntervalInMonth = -1,
            };
            _databaseContext.Tiers.Add(tier);
        }
        _databaseContext.SaveChanges();

    }
}