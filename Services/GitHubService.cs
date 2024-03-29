using github_sponsors_webhook.Database;
using GithubSponsorsWebhook.Database;
using GithubSponsorsWebhook.Dtos;
using GithubSponsorsWebhook.GitHubGraphQLModels;

namespace GithubSponsorsWebhook.Services;

public class GitHubService : IGitHubService
{
    private readonly HttpClientGitHubGraphQLService _httpClient;
    private readonly ILogger<GitHubService> _logger;
    private readonly ILiteDbSponsorService _liteDbService;
    public GitHubService(HttpClientGitHubGraphQLService httpClient, ILogger<GitHubService> logger, ILiteDbSponsorService dbSponsorService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _liteDbService = dbSponsorService;
    }

    public async Task<List<SponsorDto>> GetAllSponsors()
    {
        var sponsorDtos = new List<SponsorDto>();
        try
        {
            var responses = await _httpClient.GetAllSponsors();
            foreach (var response in responses)
            {
                if (response.edges.Length == 0)
                {
                    continue;
                }
                var mapped = response.edges.Select(x => x.node).ToList();
                sponsorDtos.AddRange(mapped.Select(m => MapToSponsorDto(m)));
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while getting all sponsors");
        }
        return sponsorDtos;
    }

    public async Task<SponsorDto?> GetSponsorDataByLogin(string login)
    {
        try
        {
            var response = await _httpClient.GetSponsorByLoginForViewer(login);
            return SponsorResponseToDto(response);
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Error while getting sponsor data by login for {login}", login);
            return null;
        }
    }

    public async Task<SponsorDto?> GetSponsorDataByToken(string token)
    {
        try
        {
            var response = await _httpClient.GetUserByToken(token);
            if (response.HasValue)
            {
                var edges = response.Value.data.viewer.organizations?.edges;
                if (edges?.Length > 0)
                {
                    var orgsFromDb = _liteDbService.FindAll().Where(x => x.GithubType == Models.GithubType.ORGANIZATION).ToList();
                    OrganizationEdge? matchOrg = Array.Find(edges, x =>
                    {
                        return orgsFromDb.Find(y => x.node.login == y.LoginName) != null;
                    });
                    if(matchOrg != null)
                    {
                        return await GetSponsorDataByLogin(matchOrg.node.login);
                    }
                }
                return await GetSponsorDataByLogin(response.Value.data.viewer.login);
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Error while getting sponsor data by token for {token}", token);
            return null;
        }
        return null;
    }

    public async Task<bool> IsSponsorFromViewer(string login)
    {
        try
        {
            var response = await _httpClient.GetIsSponsoringViewer(login);
            return response.data.organization.HasValue || response.data.user.HasValue;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Error while getting sponsor data by login for {login}", login);
            return false;
        }
    }

    public async Task<bool> ViewerIsSponsorFrom(string login)
    {
        try
        {
            var response = await _httpClient.GetSponsorByLoginForViewer(login);
            return response.data.organization.HasValue || response.data.user.HasValue;
        }
        catch (HttpRequestException e)
        {
            _logger.LogError(e, "Error while getting sponsor data by login for {login}", login);
            return false;
        }
    }

    #region Mapping

    private static SponsorDto MapToSponsorDto(SponsorshipNode node)
    {
        SponsorEntity sponsor = node.sponsorEntity;
        ClosestTierDto? closestTierDto = null;
        if (node.tier.closestLesserValueTier.HasValue)
        {
            closestTierDto = new ClosestTierDto
            {
                id = node.tier.closestLesserValueTier.Value.id,
                name = node.tier.closestLesserValueTier.Value.name,
            };
        }
        return new SponsorDto
        {
            email = sponsor.email,
            entityType = sponsor.__typename == "User" ? Models.GithubType.USER : Models.GithubType.ORGANIZATION,
            login = sponsor.login,
            name = sponsor.name,
            tier = new TierDto
            {
                id = node.tier.id,
                monthlyPriceInCents = node.tier.monthlyPriceInCents,
                isCustomAmount = node.tier.isCustomAmount,
                isOneTime = node.tier.isOneTime,
                name = node.tier.name,
                tierSelectedAt = node.tierSelectedAt,
                closestLesserValueTier = closestTierDto
            },
        };
    }

    private static SponsorDto? SponsorResponseToDto(SponsorResponse response)
    {
        GitHubEntity? sponsor = response.data.organization ?? (response.data.user ?? null);
        if (sponsor == null)
        {
            return null;
        }
        TierDto tier = new();
        if (sponsor.Value.sponsorshipForViewerAsSponsorable.HasValue)
        {
            var tierResponse = sponsor.Value.sponsorshipForViewerAsSponsorable.Value.tier;
            ClosestTierDto? closestTierDto = null;
            if (tierResponse.closestLesserValueTier.HasValue)
            {
                closestTierDto = new ClosestTierDto
                {
                    id = tierResponse.closestLesserValueTier.Value.id,
                    name = tierResponse.closestLesserValueTier.Value.name,
                };
            }
            tier = new TierDto
            {
                id = tierResponse.id,
                monthlyPriceInCents = tierResponse.monthlyPriceInCents,
                isCustomAmount = tierResponse.isCustomAmount,
                isOneTime = tierResponse.isOneTime,
                name = tierResponse.name,
                tierSelectedAt = sponsor.Value.sponsorshipForViewerAsSponsorable.Value.tierSelectedAt,
                closestLesserValueTier = closestTierDto
            };
        }

        return new SponsorDto
        {
            email = sponsor.Value.email,
            entityType = response.data.organization.HasValue ? Models.GithubType.ORGANIZATION : Models.GithubType.USER,
            login = sponsor.Value.login,
            name = sponsor.Value.name,
            tier = tier,
            databaseId = sponsor.Value.databaseId
        };
    }

    #endregion Mapping
}