using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GithubSponsorsWebhook.Database.Models;

/**
* Database model for a tier.
* A tier is a level of sponsorship.
* A sponsor can have one tier at a time.
* A tier can be cancelled and will be held in the database for one year.
*/
[Table("Tiers")]

public class Tier
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [ForeignKey("Sponsor")]
    public int SponsorId { get; set; }
#pragma warning disable CS8618 // sponsors can not be nullable. Will be handled by Entity Framework
    public Sponsor Sponsor { get; set; }
#pragma warning restore CS8618


    [StringLength(254)]
    public string? Name { get; set; }

    public int MonthlyPriceInCent { get; set; }

    public int MonthlyPriceInDollar { get; set; }
    /**
    * The date when the tier was created or updated. It does not represent our database creation.
    */
    public DateTime LatestChangeAt { get; set; }

    public int CurrentCalculatedIntervalInMonth { get; set; }

    public bool IsCancelled { get; set; }

}