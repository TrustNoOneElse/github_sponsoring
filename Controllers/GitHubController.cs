using GithubSponsorsWebhook.Dtos;
using GithubSponsorsWebhook.Services;
using Microsoft.AspNetCore.Mvc;

namespace GithubSponsorsWebhook.Controllers;

[ApiController]
[Route("[controller]")]
public class GitHubController : ControllerBase
{
    private readonly IGitHubService _gitHubService;
    private readonly ILogger<GitHubController> _logger;
    private readonly IGitHubPaymentService _gitHubPaymentService;
    private readonly ISponsorshipService _sponsorshipService;
    public GitHubController(IGitHubService gitHubService, ILogger<GitHubController> logger, IGitHubPaymentService gitHubPaymentService, ISponsorshipService sponsorshipService)
    {
        _gitHubService = gitHubService;
        _logger = logger;
        _gitHubPaymentService = gitHubPaymentService;
        _sponsorshipService = sponsorshipService;
    }

    [HttpGet("sponsor/by/login")]
    public async Task<IActionResult> GetSponsorByLogin([FromQuery] string login)
    {
        var sponsor = await _gitHubService.GetSponsorDataByLogin(login);
        if (!sponsor.HasValue)
            return NotFound();
        return Ok(_gitHubPaymentService.FillSponsorDtoWithDatabase(sponsor.Value));
    }

    [HttpGet("sponsor/by/token")]
    public async Task<IActionResult> GetSponsorByToken([FromQuery] string token)
    {
        var sponsor = await _gitHubService.GetSponsorDataByToken(token);
        if (!sponsor.HasValue)
            return NotFound();
        return Ok(_gitHubPaymentService.FillSponsorDtoWithDatabase(sponsor.Value));
    }

    [HttpGet("sponsor/list")]
    public async Task<IActionResult> GetSponsorList()
    {
        var sponsors = await _gitHubService.GetAllSponsors();
        return Ok(_gitHubPaymentService.FillSponsorDtoWithDatabase(sponsors));
    }

    [HttpGet("sponsor/is/from/viewer")]
    public async Task<IActionResult> IsSponsorFromViewer([FromQuery] string login)
    {
        var isSponsor = await _gitHubService.IsSponsorFromViewer(login);
        return Ok(isSponsor);
    }

    [HttpGet("sponsor/viewer/is/from")]
    public async Task<IActionResult> ViewerIsSponsorFrom([FromQuery] string login)
    {
        var isSponsor = await _gitHubService.ViewerIsSponsorFrom(login);
        return Ok(isSponsor);
    }

    [HttpPost("sponsor/switch")]
    [Consumes("application/json")]
    public async Task<IActionResult> SponsorFromPatreonSwitch([FromBody] SponsorSwitchDto sponsorSwitchDto)
    {
        var sponsor = await _gitHubService.GetSponsorDataByToken(sponsorSwitchDto.token);
        if (!sponsor.HasValue) return NoContent();
        _sponsorshipService.ProcessSpnsorSwitchEvent(new Models.SponsorSwitchEvent
        {
            GithubType = sponsor.Value.entityType,
            LoginName = sponsor.Value.login,
            TotalSpendInCentInOtherInstance = sponsorSwitchDto.lifetimeAmountinCent,
            DatabaseId = sponsor.Value.databaseId,
        });
        return Ok();
    }
}
