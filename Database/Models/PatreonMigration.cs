using LiteDB;

namespace GithubSponsorsWebhook.Database.Models;

public class PatreonMigration
{
    public long LifetimeAmountinCent { get; set; }
    public Guid PatreonId { get; set; }

    public bool HasHigherAmount(long amount)
    {
        return amount > LifetimeAmountinCent;
    }

    public long GetDifference(long amount)
    {
        if (HasHigherAmount(amount))
            return amount - LifetimeAmountinCent;
        else
            return LifetimeAmountinCent - amount;
    }
}

