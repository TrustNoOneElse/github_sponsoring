namespace GithubSponsorsWebhook.Database;

using GithubSponsorsWebhook.Database.Models;
using Microsoft.EntityFrameworkCore;

/**
* Database context for the application
*/
public class DatabaseContext : DbContext
{
    public DbSet<Sponsor> Sponsors { get; set; }
    public DbSet<Tier> Tiers { get; set; }
    public DbSet<OneTimePayment> OneTimePayments { get; set; }
#pragma warning disable CS8618 // DbSet will be handled by Entity Framework
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
#pragma warning restore CS8618

}