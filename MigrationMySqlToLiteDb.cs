namespace github_sponsors_webhook;

using MySql.Data.MySqlClient;
using github_sponsors_webhook.Database;
using GithubSponsorsWebhook.Database.Models;
using GithubSponsorsWebhook.Models;
using System.Data;

public class MigrationMySqlToLiteDb
{
    public void DoMigratation(ILiteDbSponsorService sponsorService, string mysqlConnectionString, ILogger logger)
    {
        var conn = new MySqlConnection(mysqlConnectionString);
        conn.Open();
        var query = "SELECT * FROM github_sponsors.sponsors AS s " +
            "LEFT JOIN tiers AS t ON t.SponsorId = s.Id " +
            "LEFT JOIN onetimepayments AS o ON o.SponsorId = s.Id;";
        var cmd = new MySqlCommand(query, conn);

        var reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            logger.LogInformation("Start Migration...");
            while (reader.Read())
            {
                Sponsor sponsor = new();
                sponsor.LoginName = reader.GetString("LoginName");
                sponsor.FirstSponsoredAt = reader.GetDateTime("firstSponsoredAt");
                sponsor.DatabaseId = reader.GetInt32("DatabaseId");
                sponsor.GithubType = (GithubType)reader.GetByte("GithubType");
                sponsor.TotalSpendInCent = reader.GetInt32("TotalSpendInCent");
                sponsor.TotalSpendInDollar = reader.GetInt32("TotalSpendInDollar");
                // if has tier
                if (!reader.IsDBNull(7))
                {
                    sponsor.CurrentTier = new Tier();
                    var tier = sponsor.CurrentTier;
                    tier.Name = reader.GetString("Name");
                    tier.MonthlyPriceInCent = reader.GetInt32("MonthlyPriceInCent");
                    tier.MonthlyPriceInDollar = reader.GetInt32("MonthlyPriceInDollar");
                    tier.LatestChangeAt = reader.GetDateTime("LatestChangeAt");
                    tier.CurrentCalculatedIntervalInMonth = reader.GetInt32("CurrentCalculatedIntervalInMonth");
                    tier.IsCancelled = reader.GetBoolean("IsCancelled");
                }
                // if has one time payments
                if(!reader.IsDBNull(15))
                {
                    sponsor.Payments = new List<OneTimePayment>();
                    var oneTimePayment = new OneTimePayment();
                    oneTimePayment.TotalInDollar = reader.GetInt32("TotalInDollar");
                    oneTimePayment.TotalInCent = reader.GetInt32("TotalInCent");
                    oneTimePayment.CreatedAt = reader.GetDateTime("CreatedAt");
                    sponsor.Payments.Add(oneTimePayment);
                }
                sponsorService.AddSponsor(sponsor);
            }
        }
        logger.LogInformation("End Migration...");
        reader.Close();
    }
}

