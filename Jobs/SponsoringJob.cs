using GithubSponsorsWebhook.Services;
using Quartz;

namespace GithubSponsorsWebhook.Jobs;

/**
* This job is executed every few hours and checks if we need to calculate the new tiers of a sponsor.
* One time payments are not handled by this job.
*/
public class SponsoringJob : IJob
{

    private readonly ISponsorshipService _sponsorshipService;

    public SponsoringJob(ISponsorshipService service)
    {
        _sponsorshipService = service;
    }
    Task IJob.Execute(IJobExecutionContext context)
    {
        _sponsorshipService.ExecuteCronJob();
        return Task.CompletedTask;
    }
}