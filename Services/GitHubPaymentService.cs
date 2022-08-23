using GithubSponsorsWebhook.Database;
using GithubSponsorsWebhook.Database.Models;
using GithubSponsorsWebhook.Dtos;

namespace GithubSponsorsWebhook.Services;

public class GitHubPaymentService : IGitHubPaymentService
{
    private readonly ILogger<GitHubPaymentService> _logger;
    private readonly DatabaseContext _db;

    public GitHubPaymentService(ILogger<GitHubPaymentService> logger, DatabaseContext db)
    {
        _logger = logger;
        _db = db;
    }

    public List<SponsorDto> FillSponsorDtoWithDatabase(List<SponsorDto> sponsorDto)
    {
        return sponsorDto.Select(x => FillSponsorDtoWithDatabase(x)).ToList();
    }

    public SponsorDto FillSponsorDtoWithDatabase(SponsorDto sponsorDto)
    {
        Sponsor sponsor = null;
        try
        {
            sponsor = _db.Sponsors.Where(s => s.LoginName == sponsorDto.login && s.GithubType == sponsorDto.entityType).First();
        }
        catch
        {
            _logger.LogWarning($"Sponsor {sponsorDto.login} not found in database for {sponsorDto.entityType}");
        }
        if (sponsor != null)
        {
            sponsorDto.totalSpendInCent = sponsor.TotalSpendInCent;
            sponsorDto.totalSpendInDollar = sponsor.TotalSpendInDollar;
            sponsorDto.firstSponsoredAt = sponsor.firstSponsoredAt;
            sponsorDto.payedLifetime = HasPayedLifetime(sponsor);
            sponsorDto.payedMinimum = HasPayedMinimum(sponsor);
        }
        else
        {
            _logger.LogInformation($"Sponsor {sponsorDto.login} not found in database for {sponsorDto.entityType}");
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