namespace GithubSponsorsWebhook.GitHubModels;
public struct GitHubTier
{
    public string node_id { get; set; }
    public DateTime created_at { get; set; }
    public string description { get; set; }
    public int monthly_price_in_cents { get; set; }
    public int monthly_price_in_dollars { get; set; }
    public string name { get; set; }
    public bool is_one_time { get; set; }
    public bool is_custom_amount { get; set; }
}