using github_sponsors_webhook.Database;
using GithubSponsorsWebhook.Database.Models;
using GithubSponsorsWebhook.Dtos;

namespace GithubSponsorsWebhook.Services;

public class GitHubPaymentService : IGitHubPaymentService
{
    private readonly ILogger<GitHubPaymentService> _logger;
    private readonly ILiteDbSponsorService _dbContext;

    public GitHubPaymentService(ILogger<GitHubPaymentService> logger, ILiteDbSponsorService db)
    {
        _logger = logger;
        _dbContext = db;
    }

    public List<SponsorDto> FillSponsorDtoWithDatabase(List<SponsorDto> sponsorDto)
    {
        return sponsorDto.Select(x => FillSponsorDtoWithDatabase(x)).ToList();
    }

    public SponsorDto FillSponsorDtoWithDatabase(SponsorDto sponsorDto)
    {
        var sponsor = _dbContext.FindSponsor(sponsorDto.login, sponsorDto.entityType);
        if (sponsor != null && sponsor != default)
        {
            sponsorDto.totalSpendInCent = sponsor.TotalSpendInCent;
            sponsorDto.firstSponsoredAt = sponsor.FirstSponsoredAt;
            sponsorDto.payedLifetime = HasPayedLifetime(sponsor);
            sponsorDto.payedMinimum = HasPayedMinimum(sponsor);
        }
        else
        {
            _logger.LogInformation("Sponsor {login} not found in database for {entityType}", sponsorDto.login, sponsorDto.entityType);
        }
        return sponsorDto;
    }

    public bool HasPayedMinimum(Sponsor sponsor)
    {

        var minimumSpendInCent = Environment.GetEnvironmentVariable("MINIMUM_SPEND_IN_CENT");
        if (minimumSpendInCent == null)
        {
            _logger.LogCritical("Environment variable MINIMUM_SPEND_IN_CENT is not set.");
            return false;
        }
        return sponsor.TotalSpendInCent >= int.Parse(minimumSpendInCent);
    }

    public bool HasPayedLifetime(Sponsor sponsor)
    {
        var lifetimeSpendInCent = Environment.GetEnvironmentVariable("LIFETIME_SPEND_IN_CENT");
        if (lifetimeSpendInCent == null)
        {
            _logger.LogCritical("Environment variable LIFETIME_SPEND_IN_CENT is not set.");
            return false;
        }
        return sponsor.TotalSpendInCent >= int.Parse(lifetimeSpendInCent);
    }


}