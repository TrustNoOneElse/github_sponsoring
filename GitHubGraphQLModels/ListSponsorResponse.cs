namespace GithubSponsorsWebhook.GitHubGraphQLModels;

public struct ListSponsorDataResponse
{
    public ListSponsorViewerDataResponse data { get; set; }
}

public struct ListSponsorViewerDataResponse
{
    public ListSponsorViewerAsMaintainerResponse viewer { get; set; }
}

public struct ListSponsorViewerAsMaintainerResponse
{
    public ListSponsorResponse sponsorshipsAsMaintainer { get; set; }
}

public struct ListSponsorResponse
{
    public SponsorshipEdge[] edges { get; set; }
    public PageInfo pageInfo { get; set; }
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