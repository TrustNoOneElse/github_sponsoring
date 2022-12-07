namespace GithubSponsorsWebhook.Database;

using LiteDB;
using Microsoft.Extensions.Options;

public class LiteDbContext : ILiteDbContext
{
    public LiteDatabase Database { get; }

    public LiteDbContext(IOptions<LiteDbOptions> options)
    {
        Database = new LiteDatabase(options.Value.DatabaseLocation);
    }
}

public interface ILiteDbContext
{
    public LiteDatabase Database { get; }
}