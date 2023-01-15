using System.ComponentModel.DataAnnotations;

namespace GithubSponsorsWebhook.Dtos;
#pragma warning disable IDE1006 // its a dto
public class SponsorSwitchDto
{
    public string token { get; set; }

    public int lifetimeAmountinCent { get; set; }

    public Guid patreonId { get; set; }
}
#pragma warning restore IDE1006 //  its a dto
