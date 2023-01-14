namespace GithubSponsorsWebhook.Database.Models;

/**
* Database model for a tier.
* A tier is a level of sponsorship.
* A sponsor can have one tier at a time.
* A tier can be cancelled and will be held in the database for one year.
*/

public class Tier
{
    public int Id { get; set; }
 
    public string Name { get; set; }

    public int MonthlyPriceInCent { get; set; }

    /**
    * The date when the tier was created or updated. It does not represent our database creation.
    */
    public DateTime LatestChangeAt { get; set; }

    public int CurrentCalculatedIntervalInMonth { get; set; }

    public bool IsCancelled { get; set; }

}