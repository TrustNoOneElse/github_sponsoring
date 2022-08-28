using GithubSponsorsWebhook.GitHubGraphQLModels;
using GithubSponsorsWebhook.Utils;
using System.Net.Http.Headers;

namespace GithubSponsorsWebhook.Services;

public class HttpClientGitHubGraphQLService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpClientGitHubGraphQLService> _logger;
    private readonly Uri baseUrl = new Uri("https://api.github.com/graphql");

    public HttpClientGitHubGraphQLService(HttpClient httpClient, ILogger<HttpClientGitHubGraphQLService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.BaseAddress = baseUrl;
        // The GitHub GraphQL API requires Authorization header .
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_TOKEN")))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", Environment.GetEnvironmentVariable("GITHUB_TOKEN"));
        }
        else
        {
            _logger.LogCritical("GITHUB_TOKEN is not set, please set it in the environment variables");
        }
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "GithubSponsorsWebhook");
    }

    public async Task<ViewerResponse?> GetUserByToken(string token)
    {
        var requestContent = new GraphQLRequest();
        requestContent.query = GraphQLQueries.GetCurrentViewerQuery;
        var request = new HttpRequestMessage()
        {
            RequestUri = baseUrl,
            Method = HttpMethod.Post,
            Content = JsonContent.Create(requestContent),
        };
        request.Headers.Add("Authorization", $"bearer {token}");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ViewerResponse>();
    }

    public async Task<bool> GetViewerIsSponsoringFor(string loginName)
    {
        var requestContent = new GraphQLRequest();
        requestContent.query = GraphQLQueries.GetViewerIsSponsoringForQuery;
        requestContent.variables = new Variables() { loginName = loginName };
        var request = new HttpRequestMessage()
        {
            RequestUri = baseUrl,
            Method = HttpMethod.Post,
            Content = JsonContent.Create(requestContent),
        };
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<SponsorResponse>();
        if (content.data.user.HasValue && content.data.user.Value.viewerIsSponsoring.HasValue)
        {
            return content.data.user.Value.viewerIsSponsoring.Value;
        }
        else if (content.data.organization.HasValue && content.data.organization.Value.viewerIsSponsoring.HasValue)
        {
            return content.data.organization.Value.viewerIsSponsoring.Value;
        }
        return false;
    }

    public async Task<SponsorResponse> GetIsSponsoringViewer(string loginName)
    {
        var requestContent = new GraphQLRequest();
        requestContent.query = GraphQLQueries.GetIsSponsoringViewerQuery;
        requestContent.variables = new Variables() { loginName = loginName };
        var request = new HttpRequestMessage()
        {
            RequestUri = baseUrl,
            Method = HttpMethod.Post,
            Content = JsonContent.Create(requestContent),
        };
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SponsorResponse>();
    }

    public async Task<SponsorResponse> GetSponsorByLoginForViewer(string login)
    {
        var requestContent = new GraphQLRequest();
        requestContent.query = GraphQLQueries.GetSponsorByLoginForViewerQuery;
        requestContent.variables = new Variables() { loginName = login };
        var request = new HttpRequestMessage()
        {
            RequestUri = baseUrl,
            Method = HttpMethod.Post,
            Content = JsonContent.Create(requestContent),
        };
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SponsorResponse>();
    }

    public async Task<List<ListSponsorResponse>> GetAllSponsors()
    {
        var sponsors = new List<ListSponsorResponse>();
        await foreach (var sponsor in FetchSponsors())
        {
            if (sponsor != null)
            {
                sponsors.Add((ListSponsorResponse)sponsor);
            }
            else
            {
                break;
            }
        }
        return sponsors;
    }

    private async Task<ListSponsorResponse> GetSponsorList(string? cursor = null)
    {
        var requestContent = new GraphQLRequest();
        if (cursor == null)
            requestContent.query = GraphQLQueries.GetSponsorListQuery;
        else
        {
            requestContent.query = GraphQLQueries.GetSponsorListPaginationQuery;
            requestContent.variables = new Variables() { cursor = cursor };
        }
        var request = new HttpRequestMessage()
        {
            RequestUri = baseUrl,
            Method = HttpMethod.Post,
            Content = JsonContent.Create(requestContent),
        };
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        _logger.LogDebug("Response from GitHub GraphQL API: {0}", await response.Content.ReadAsStringAsync());
        var responseData = await response.Content.ReadFromJsonAsync<ListSponsorDataResponse>();
        return responseData.data.viewer.sponsorshipsAsMaintainer;
    }

    private async IAsyncEnumerable<ListSponsorResponse?> FetchSponsors()
    {
        var response = await GetSponsorList();
        yield return response;
        while (response.pageInfo.hasNextPage)
        {
            response = await GetSponsorList(response.pageInfo.endCursor);
            yield return response;
        }
        yield return null;
    }
}
