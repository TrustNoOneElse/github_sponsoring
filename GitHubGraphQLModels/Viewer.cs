namespace GithubSponsorsWebhook.GitHubGraphQLModels;
public struct Viewer
{
    public string login { get; set; }
    public string email { get; set; }
    public string name { get; set; }
    public int databaseId { get; set; }

    public Organizations organizations { get; set; }
}

public class Organizations
{
    public OrganizationEdge[]? edges { get; set; }
}

public class OrganizationEdge
{
    public OrganizationNode node { get; set; }
}

public class OrganizationNode
{
    public int databaseId { get; set; }
    public string login { get; set; }
}