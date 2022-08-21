namespace GithubSponsorsWebhook.Utils;

public static class GraphQLQueries
{
    public readonly static string GetSponsorByLoginForViewerQuery = System.IO.File.ReadAllText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Queries", "SponsorByLoginForViewer.gql"));
    public readonly static string GetCurrentViewerQuery = System.IO.File.ReadAllText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Queries", "CurrentViewer.gql"));
    public readonly static string GetSponsorListPaginationQuery = System.IO.File.ReadAllText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Queries", "SponsorListPagination.gql"));

    public readonly static string GetSponsorListQuery = System.IO.File.ReadAllText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Queries", "SponsorListFirst.gql"));

    public readonly static string GetViewerIsSponsoringForQuery = System.IO.File.ReadAllText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Queries", "ViewerIsSponsoringFor.gql"));

    public readonly static string GetIsSponsoringViewerQuery = System.IO.File.ReadAllText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Queries", "IsSponsoringViewer.gql"));

}