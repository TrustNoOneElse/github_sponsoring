using GithubSponsorsWebhook.Database;
using GithubSponsorsWebhook.Services;
using GithubSponsorsWebhook.Jobs;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

#region Build Configuration
// Add services to the container.
// add database context to the container
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<ISponsorshipService, SponsorshipService>();
builder.Services.AddControllers();
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
if (builder.Environment.IsDevelopment())
{
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

#endregion Build Configuration

#region App Configuration
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicyBuilder =>
{
    CorsPolicyBuilder.AllowAnyOrigin();
    CorsPolicyBuilder.WithHeaders("Content-Type", "X-Hub-Signature-256", "X-GitHub-Event", "X-GitHub-Delivery");
    CorsPolicyBuilder.WithMethods("POST", "OPTIONS");
});

// make sure database is filled
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    context.Database.EnsureCreated();
}

app.UseHttpLogging();

app.MapControllers();
#endregion App Configuration

app.Run();
