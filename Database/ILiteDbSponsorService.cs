using GithubSponsorsWebhook.Database;
using GithubSponsorsWebhook.Database.Models;
using GithubSponsorsWebhook.Models;

namespace github_sponsors_webhook.Database;
public interface ILiteDbSponsorService
{
    public ILogger<ILiteDbSponsorService> Logger { get; }
    public ILiteDbContext LiteDbContext { get; }

    public int AddSponsor(Sponsor sponsor);
    public bool UpdateSponsor(ref Sponsor sponsor);
    public Sponsor FindSponsor(string loginName);
    public Sponsor FindSponsor(string loginName, GithubType type);
    public Sponsor FindSponsor(int databaseId);
    public Sponsor FindSponsor(int databaseId, GithubType type);
    public Sponsor FindSponsorById(int id);
    public List<Sponsor> FindAll();
    public bool DeleteSponsor(Sponsor sponsor);

    public int UpdateSponsors(List<Sponsor> sponsors);

}
