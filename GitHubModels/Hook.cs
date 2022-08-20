namespace GithubSponsorsWebhook.GitHubModels;

public struct Hook
{
    public string type { get; set; }
    public long id { get; set; }
    public string name { get; set; }
    public bool active { get; set; }
    public List<string> events { get; set; }
    public Config config { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime created_at { get; set; }
    public string? url { get; set; }
    public string? test_url { get; set; }
    public string? ping_url { get; set; }
    public LastResponse? last_response { get; set; }
}