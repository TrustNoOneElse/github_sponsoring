using GithubSponsorsWebhook.Dtos;

namespace GithubSponsorsWebhook.Services;

public interface IGitHubService
{
    public Task<SponsorDto?> GetSponsorDataByToken(string token);
    public Task<SponsorDto?> GetSponsorDataByLogin(string login);
    public Task<List<SponsorDto>> GetAllSponsors();
    public Task<bool> IsSponsorFromViewer(string login);
    public Task<bool> ViewerIsSponsorFrom(string login);
}
