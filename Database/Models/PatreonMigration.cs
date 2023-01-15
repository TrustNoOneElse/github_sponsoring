using LiteDB;

namespace GithubSponsorsWebhook.Database.Models;

public class PatreonMigration
{
    public int LifetimeAmountinCent { get; set; }
    public Guid PatreonId { get; set; }

    public bool HasHigherAmount(int amount)
    {
        return amount > LifetimeAmountinCent;
    }

    public int GetDifference(int amount)
    {
        if (HasHigherAmount(amount))
            return amount - LifetimeAmountinCent;
        else
            return LifetimeAmountinCent - amount;
    }
}

