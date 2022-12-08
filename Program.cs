using GithubSponsorsWebhook.Database;
using GithubSponsorsWebhook.Services;
using GithubSponsorsWebhook.Jobs;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Quartz;
using Microsoft.AspNetCore.HttpOverrides;
using github_sponsors_webhook.Database;
using github_sponsors_webhook;

var builder = WebApplication.CreateBuilder(args);

#region Build Configuration
// Add services to the container.
// add LiteDb context to the container
builder.Services.Configure<LiteDbOptions>(builder.Configuration.GetSection(LiteDbOptions.LiteDb));
builder.Services.AddSingleton<ILiteDbContext, LiteDbContext>();
builder.Services.AddTransient<ILiteDbSponsorService, LiteDbSponsorService>();
builder.Services.AddScoped<ISponsorshipService, SponsorshipService>();
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<IGitHubPaymentService, GitHubPaymentService>();
builder.Services.AddHttpClient<HttpClientGitHubGraphQLService>();
builder.Services.AddControllers();
#region CronJob
builder.Services.AddQuartz(q =>
{
    q.SchedulerId = "Scheduler-Core";
    q.SchedulerName = "Quartz Scheduler";
    q.UseMicrosoftDependencyInjectionJobFactory();
    q.UseSimpleTypeLoader();
    q.UseDefaultThreadPool(tp =>
    {
        tp.MaxConcurrency = 3;
    });
    q.UseInMemoryStore();
    q.UseTimeZoneConverter();
    if(builder.Environment.IsProduction())
    q.ScheduleJob<SponsoringJob>((trigger) => trigger
        .WithIdentity("GithubSponsorsWebhook")
        .WithCronSchedule("0 0 0/4 * * ?")
        .StartNow());
    else
        q.ScheduleJob<SponsoringJob>((trigger) => trigger
        .WithIdentity("GithubSponsorsWebhook")
        .WithCronSchedule("0 0/1 * * * ?")
        .StartNow());
});
builder.Services.AddScoped<SponsoringJob>();
// if you are using persistent job store, you might want to alter some options
builder.Services.Configure<QuartzOptions>(options =>
{
    options.Scheduling.IgnoreDuplicates = true; // default: false
    options.Scheduling.OverWriteExistingData = true; // default: true
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
#endregion CronJob
if (builder.Environment.IsDevelopment())
{
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

#endregion Build Configuration

#region App Configuration
var app = builder.Build();
// Add Swagger if in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
}
else
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
}

app.UseCors(CorsPolicyBuilder =>
{
    // we will handle that in nginx proxy
    CorsPolicyBuilder.AllowAnyOrigin();
    CorsPolicyBuilder.WithHeaders("Content-Type", "X-Hub-Signature-256", "X-GitHub-Event", "X-GitHub-Delivery");
    CorsPolicyBuilder.WithMethods("POST", "OPTIONS", "GET");
});

app.UseHttpLogging();

app.MapControllers();
#endregion App Configuration

var migration = new MigrationMySqlToLiteDb();
var service = app.Services.GetService(typeof(ILiteDbSponsorService));
if (service != null && app.Configuration.GetSection("Migration").Value == "true") {
    app.Logger.LogDebug("Migration starting");
    migration.DoMigratation((ILiteDbSponsorService) service, app.Configuration.GetConnectionString("DefaultConnection"), app.Logger) ;
}

app.Run();
