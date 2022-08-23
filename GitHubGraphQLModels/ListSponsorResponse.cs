namespace GithubSponsorsWebhook.GitHubGraphQLModels;

public struct ListSponsorResponse
{
    public SponsorshipEdge[] edges { get; set; }
    public PageInfo pageInfo;
}

public struct PageInfo
{
    public string endCursor { get; set; }
    public bool hasNextPage { get; set; }
}

public struct SponsorshipEdge
{
    public SponsorshipNode node { get; set; }
}