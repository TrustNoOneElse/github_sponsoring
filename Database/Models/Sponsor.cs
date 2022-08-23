namespace GithubSponsorsWebhook.Database.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GithubSponsorsWebhook.Models;
using Microsoft.EntityFrameworkCore;

[Table("Sponsors")]
[Index("DatabaseId")]
public class Sponsor
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [StringLength(254)]
    public string? LoginName { get; set; }

    public DateTime firstSponsoredAt { get; set; }

    public int DatabaseId { get; set; }

    public GithubType GithubType { get; set; }

    public int TotalSpendInCent { get; set; }

    public int TotalSpendInDollar { get; set; }

    public virtual Tier? CurrentTier { get; set; }

    public virtual List<OneTimePayment>? Payments { get; set; }

    public bool IsSponsor => CurrentTier != null && !CurrentTier.IsCancelled;

}