using GithubSponsorsWebhook.Database;
using GithubSponsorsWebhook.Database.Models;
using GithubSponsorsWebhook.Models;

namespace github_sponsors_webhook.Database;

public class LiteDbSponsorService : ILiteDbSponsorService
{
    private const string CollectionName = "Sponsors";
    public ILogger<ILiteDbSponsorService> Logger { get; }
    public ILiteDbContext LiteDbContext { get; }

    public LiteDbSponsorService(ILiteDbContext dbContext, ILogger<ILiteDbSponsorService> logger)
    {
        Logger = logger;
        LiteDbContext = dbContext;
    }
    
    public int AddSponsor(Sponsor sponsor)
    {
        var collection = LiteDbContext.Database.GetCollection<Sponsor>(CollectionName);
        return collection.Insert(sponsor);
    }

    public bool UpdateSponsor(ref Sponsor sponsor)
    {
        var collection = LiteDbContext.Database.GetCollection<Sponsor>(CollectionName);
       return collection.Update(sponsor);
    }

    public Sponsor FindSponsor(string loginName)
    {
        var collection = LiteDbContext.Database.GetCollection<Sponsor>(CollectionName);
        return collection.FindOne(x => x.LoginName == loginName);
    }

    public Sponsor FindSponsor(int databaseId)
    {
        var collection = LiteDbContext.Database.GetCollection<Sponsor>(CollectionName);
        return collection.FindOne(x => x.DatabaseId == databaseId);
    }

    public Sponsor FindSponsor(int databaseId, GithubType type)
    {
        var collection = LiteDbContext.Database.GetCollection<Sponsor>(CollectionName);
        return collection.FindOne(x => x.DatabaseId == databaseId && x.GithubType == type);
    }

    public Sponsor FindSponsor(string loginName, GithubType githubType)
    {
        var collection = LiteDbContext.Database.GetCollection<Sponsor>(CollectionName);
        return collection.FindOne(x => x.LoginName == loginName && x.GithubType == githubType);
    }

    public Sponsor FindSponsorById(int id)
    {
        var collection = LiteDbContext.Database.GetCollection<Sponsor>(CollectionName);
        return collection.FindById(id);
    }

    public bool DeleteSponsor(Sponsor sponsor)
    {
        var collection = LiteDbContext.Database.GetCollection<Sponsor>(CollectionName);
        return collection.Delete(sponsor.Id);
    }

    public List<Sponsor> FindAll()
    {
        var collection = LiteDbContext.Database.GetCollection<Sponsor>(CollectionName);
        return collection.FindAll().ToList();
    }

    public int UpdateSponsors(List<Sponsor> sponsors)
    {
        var collection = LiteDbContext.Database.GetCollection<Sponsor>(CollectionName);
        return collection.Update(sponsors);
    }
}

