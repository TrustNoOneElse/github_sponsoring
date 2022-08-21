
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GithubSponsorsWebhook.Database.Models;

/**
* OneTimePayment is a payment that is not recurring.
*/
[Table("OneTimePayments")]
public class OneTimePayment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int TotalInCent { get; set; }

    public int TotalInDollar { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("Sponsor")]
    public int SponsorId { get; set; }
#pragma warning disable CS8618 // sponsors can not be nullable. Will be handled by Entity Framework
    public Sponsor Sponsor { get; set; }
#pragma warning restore CS8618
}