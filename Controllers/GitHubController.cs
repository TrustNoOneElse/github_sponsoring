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
    public GitHubController(IGitHubService gitHubService, ILogger<GitHubController> logger, IGitHubPaymentService gitHubPaymentService)
    {
        _gitHubService = gitHubService;
        _logger = logger;
        _gitHubPaymentService = gitHubPaymentService;
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
}
