
namespace GithubSponsorsWebhook.Database.Models;

/**
* OneTimePayment is a payment that is not recurring.
*/
public class OneTimePayment
{
    public int Id { get; set; }
    
    public int TotalInCent { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
}