
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
            _logger.LogDebug("Checking sponsor {0}", sponsor.LoginName);
            if (sponsor != null && sponsor != default && CanRecalculate(sponsor.CurrentTier))
            {
                _logger.LogDebug("Recalculating sponsor {0}", sponsor.LoginName);
                var tier = sponsor.CurrentTier;
                sponsor.TotalSpendInCent += tier.MonthlyPriceInCent;
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
            _logger.LogDebug("Updating sponsor {0}", sponsor.LoginName);
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
                var newTier = new Tier
                {
                    MonthlyPriceInCent = sponsorEvent.Tier.monthly_price_in_cents,
                    Name = sponsorEvent.Tier.name,
                    LatestChangeAt = sponsorEvent.Tier.created_at,
                    CurrentCalculatedIntervalInMonth = 0
                };
                sponsor.CurrentTier = newTier;
                sponsor.TotalSpendInCent += sponsorEvent.Tier.monthly_price_in_cents;
                _logger.LogDebug("Updated tier for sponsor {0}", sponsor.LoginName);
            }
            // API doesnt clearly say it, but they could change from one time payment to an actual tier
            else
            {
                tier = new Tier
                {
                    MonthlyPriceInCent = sponsorEvent.Tier.monthly_price_in_cents,
                    Name = sponsorEvent.Tier.name,
                    LatestChangeAt = sponsorEvent.Tier.created_at,
                    CurrentCalculatedIntervalInMonth = 0,
                };
                sponsor.CurrentTier = tier;
                sponsor.TotalSpendInCent += sponsorEvent.Tier.monthly_price_in_cents;
                _logger.LogDebug("created tier for sponsor {0}", sponsor.LoginName);
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
            var incentive = Configuration.GetConfiguration().NewAccountIncentiveInCent;
            sponsor = new Sponsor
            {
                DatabaseId = sponsorEvent.DatabaseId,
                LoginName = sponsorEvent.LoginName,
                GithubType = sponsorEvent.GithubType,
                TotalSpendInCent = incentive,
                FirstSponsoredAt = DateTime.Now,
                Payments = new List<OneTimePayment>
                {
                    new OneTimePayment
                    {
                        TotalInCent = (int)incentive,
                        CreatedAt = DateTime.Now,
                        Description = "New Account Incentive"
                    }
                },
            };
        }
        else
        {
            sponsor.LoginName = sponsorEvent.LoginName;
        }
        // if its a one time payment, we calculate the total spend here and we create a OneTimePayment object for it
        if (sponsorEvent.Tier.is_one_time)
        {
            sponsor.TotalSpendInCent += sponsorEvent.Tier.monthly_price_in_cents;
            var oneTimePayment = new OneTimePayment
            {
                TotalInCent = sponsorEvent.Tier.monthly_price_in_cents,
                CreatedAt = sponsorEvent.Tier.created_at,
                Description = sponsorEvent.Tier.name
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
            }
            var tier = new Tier
            {
                Name = sponsorEvent.Tier.name,
                MonthlyPriceInCent = sponsorEvent.Tier.monthly_price_in_cents,
                LatestChangeAt = sponsorEvent.CreatedAt,
                CurrentCalculatedIntervalInMonth = 0,
            };
            sponsor.TotalSpendInCent += sponsorEvent.Tier.monthly_price_in_cents;
            sponsor.CurrentTier = tier;
        }
        _sponsorService.UpdateSponsor(ref sponsor);
    }

    /**
     * Handles a SponsorSwitchEvent which is responsible for Patreon migrations 
     */
    public void ProcessSpnsorSwitchEvent(SponsorSwitchEvent sponsorSwitchEvent)
    {
        var sponsor = _sponsorService.FindSponsor(sponsorSwitchEvent.DatabaseId, sponsorSwitchEvent.GithubType);
        var patreonGuidSponsor = _sponsorService.FindSponsorByPatreonId(sponsorSwitchEvent.PatreonId);
        // check if user try to dupe accounts
        if (IsDuping(sponsor, patreonGuidSponsor))
        {
            _logger.LogInformation("User with patreonId " + sponsorSwitchEvent.PatreonId + " tried to create another github account.");
            return;
        }
        // User did not sponsor us on GitHub and its is first migration
        if (sponsor == null)
        {
            var incentive = Configuration.GetConfiguration().NewAccountIncentiveInCent;
            var totalSpendInCent = sponsorSwitchEvent.TotalSpendInCentInOtherInstance + incentive;
            sponsor = new Sponsor
            {
                DatabaseId = sponsorSwitchEvent.DatabaseId,
                LoginName = sponsorSwitchEvent.LoginName,
                GithubType = sponsorSwitchEvent.GithubType,
                TotalSpendInCent = totalSpendInCent,
                FirstSponsoredAt = DateTime.Now,
                PatreonMigration = new PatreonMigration
                {
                    PatreonId = sponsorSwitchEvent.PatreonId,
                    LifetimeAmountinCent = totalSpendInCent
                },
                Payments = new List<OneTimePayment> { new OneTimePayment
                {
                    CreatedAt = DateTime.Now,
                    Description = "New Account Incentive",
                    TotalInCent = (int)incentive
                    }
                }
            };
        }
        // User migrated again, here we check the difference and apply it
        else if (sponsor.PatreonMigration != null && sponsor.PatreonMigration != default)
        {
            var diff = sponsor.PatreonMigration.GetDifference(sponsorSwitchEvent.TotalSpendInCentInOtherInstance);
            if (diff <= 0) return;
            sponsor.TotalSpendInCent += diff;
            sponsor.PatreonMigration.LifetimeAmountinCent += diff;
        }
        // User already sponsor us on GitHub, but has no migration yet done for patreon
        else if (sponsor.PatreonMigration == null)
        {
            sponsor.TotalSpendInCent += sponsorSwitchEvent.TotalSpendInCentInOtherInstance;
            sponsor.PatreonMigration = new PatreonMigration
            {
                PatreonId = sponsorSwitchEvent.PatreonId,
                LifetimeAmountinCent = sponsorSwitchEvent.TotalSpendInCentInOtherInstance
            };
        }
        if (sponsor != null)
            _sponsorService.UpdateSponsor(ref sponsor);

        static bool IsDuping(Sponsor sponsor, Sponsor? patreonGuidSponsor)
        {
            return patreonGuidSponsor != null && sponsor == null || patreonGuidSponsor != null && sponsor != null && sponsor.DatabaseId != patreonGuidSponsor.DatabaseId;
        }
    }
}