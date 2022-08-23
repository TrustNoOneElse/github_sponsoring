using GithubSponsorsWebhook.Database.Models;
using GithubSponsorsWebhook.Dtos;

namespace GithubSponsorsWebhook.Services;

public interface IGitHubPaymentService
{
    public SponsorDto FillSponsorDtoWithDatabase(SponsorDto sponsorDto);
    public List<SponsorDto> FillSponsorDtoWithDatabase(List<SponsorDto> sponsorDto);
    public bool HasPayedMinimum(Sponsor sponsor);
    public bool HasPayedLifetime(Sponsor sponsor);
}