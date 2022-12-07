using System.Text.Json;
using GithubSponsorsWebhook.GitHubModels;
using GithubSponsorsWebhook.Models;
using GithubSponsorsWebhook.Services;
using GithubSponsorsWebhook.Utils;
using Microsoft.AspNetCore.Mvc;

namespace GithubSponsorsWebhook.Controllers;

[ApiController]
[Route("[controller]")]
public class GitHubWebhookController : ControllerBase
{
    private readonly ILogger<GitHubWebhookController> _logger;
    private readonly ISponsorshipService _sponsorshipService;
    private readonly IWebHostEnvironment _env;

    public GitHubWebhookController(ILogger<GitHubWebhookController> logger, ISponsorshipService sponsorshipService, IWebHostEnvironment env)
    {
        _logger = logger;
        _sponsorshipService = sponsorshipService;
        _env = env;
    }


    [HttpPost("sponsorship")]
    [Consumes("application/json")]
    public async Task<IActionResult> Sponsorship([FromHeader(Name = "X-Hub-Signature-256")] string sha256Secret, [FromHeader(Name = "X-Hub-Signature")] string sha1Secret)
    {
        JsonDocument? json = null;
        using (var reader = new StreamReader(Request.Body))
        {
            var jsonPayload = await reader.ReadToEndAsync();
            if (_env.IsProduction() && !GitHubVerify.VerifySignature(sha256Secret, jsonPayload, _logger))
            {
                return NotFound();
            }
            json = JsonDocument.Parse(jsonPayload);
        }
        // Normally you would throw Unauthorized, but for the bad case we dont want to show that he used the wrong secret

        JsonElement root = json.RootElement;
        // Check if its a sponsorship Event
        if (root.TryGetProperty("action", out JsonElement action))
        {
            var sponsorEvent = SponsorshipEventFactory.CreateSponsorEvent(root);
            if (sponsorEvent != null)
            {
                _sponsorshipService.ProcessSponsorEvent((SponsorEvent)sponsorEvent);
            }
        }
        // check if its a ping event
        else if (root.TryGetProperty("zen", out JsonElement zen))
        {
            var pingEvent = JsonSerializer.Deserialize<PingEvent>(root);
            _logger.LogDebug("PingEvent registered with id:" + pingEvent.hook_id);
            _logger.LogDebug("PingEvent registered:" + pingEvent.zen);
        }
        // should never happen or it is a attack
        else
        {
            _logger.LogCritical("Unknown Event happened, please investigate");
            json.Dispose();
            // if its a attack, we dont want to show that we are vulnerable to it
            return NotFound();
        }
        json.Dispose();
        return Ok();
    }

}
