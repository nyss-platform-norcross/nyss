using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data
{
    public class NyssContext : DbContext, INyssContext, INyssReportContext
    {
        public NyssContext(DbContextOptions<NyssContext> options)
            : base(options)
        {
        }

        public DbSet<Alert> Alerts { get; set; }

        public DbSet<AlertEvent> AlertEvents { get; set; }

        public DbSet<AlertRecipient> AlertRecipients { get; set; }

        public DbSet<AlertReport> AlertReports { get; set; }

        public DbSet<AlertRule> AlertRules { get; set; }

        public DbSet<ApplicationLanguage> ApplicationLanguages { get; set; }

        public DbSet<ContentLanguage> ContentLanguages { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<DataCollector> DataCollectors { get; set; }

        public DbSet<District> Districts { get; set; }

        public DbSet<GatewaySetting> GatewaySettings { get; set; }

        public DbSet<HeadManagerConsent> HeadManagerConsents { get; set; }

        public DbSet<HealthRisk> HealthRisks { get; set; }

        public DbSet<HealthRiskLanguageContent> HealthRiskLanguageContents { get; set; }

        public DbSet<Localization> Localizations { get; set; }

        public DbSet<LocalizedTemplate> LocalizedTemplates { get; set; }

        public DbSet<NationalSociety> NationalSocieties { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Project> Projects { get; set; }
        public DbSet<SupervisorUserProject> SupervisorUserProjects { get; set; }

        public DbSet<ProjectHealthRisk> ProjectHealthRisks { get; set; }

        public DbSet<Region> Regions { get; set; }

        public DbSet<Report> Reports { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<UserNationalSociety> UserNationalSocieties { get; set; }

        public DbSet<Village> Villages { get; set; }

        public DbSet<Zone> Zones { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NyssContext).Assembly);
            modelBuilder.HasDefaultSchema("nyss");
            modelBuilder.Seed();
        }
    }
}
