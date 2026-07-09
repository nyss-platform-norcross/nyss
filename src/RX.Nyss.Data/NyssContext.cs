using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data;

public class NyssContext : DbContext, INyssContext
{
    public NyssContext(DbContextOptions<NyssContext> options)
        : base(options)
    {
    }

    public NyssContext()
    {
    }

    public DbSet<Alert> Alerts { get; set; }

    public DbSet<AlertEventType> AlertEventTypes { get; set; }

    public DbSet<AlertEventSubtype> AlertEventSubtypes { get; set; }

    public DbSet<AlertEventLog> AlertEventLogs { get; set; }

    public DbSet<AlertNotificationRecipient> AlertNotificationRecipients { get; set; }

    public DbSet<AlertReport> AlertReports { get; set; }

    public DbSet<AlertRule> AlertRules { get; set; }

    public DbSet<ApplicationLanguage> ApplicationLanguages { get; set; }

    public DbSet<ContentLanguage> ContentLanguages { get; set; }

    public DbSet<Country> Countries { get; set; }

    public DbSet<DataCollector> DataCollectors { get; set; }

    public DbSet<District> Districts { get; set; }

    public DbSet<GatewaySetting> GatewaySettings { get; set; }

    public DbSet<NationalSocietyConsent> NationalSocietyConsents { get; set; }

    public DbSet<HealthRisk> HealthRisks { get; set; }

    public DbSet<HealthRiskLanguageContent> HealthRiskLanguageContents { get; set; }

    public DbSet<SuspectedDisease> SuspectedDisease { get; set; }

    public DbSet<SuspectedDiseaseLanguageContent> SuspectedDiseaseLanguageContent { get; set; }

    public DbSet<Localization> Localizations { get; set; }

    public DbSet<LocalizedTemplate> LocalizedTemplates { get; set; }

    public DbSet<NationalSociety> NationalSocieties { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<Organization> Organizations { get; set; }

    public DbSet<Project> Projects { get; set; }

    public DbSet<ProjectOrganization> ProjectOrganizations { get; set; }

    public DbSet<SupervisorUserProject> SupervisorUserProjects { get; set; }

    public DbSet<SupervisorUserAlertRecipient> SupervisorUserAlertRecipients { get; set; }

    public DbSet<ProjectHealthRisk> ProjectHealthRisks { get; set; }

    public DbSet<ProjectHealthRiskAlertRecipient> ProjectHealthRiskAlertRecipients { get; set; }

    public DbSet<RawReport> RawReports { get; set; }

    public DbSet<Region> Regions { get; set; }

    public DbSet<Report> Reports { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<UserNationalSociety> UserNationalSocieties { get; set; }

    public DbSet<Village> Villages { get; set; }

    public DbSet<Zone> Zones { get; set; }

    public DbSet<HeadSupervisorUserProject> HeadSupervisorUserProjects { get; set; }

    public DbSet<HeadSupervisorUserAlertRecipient> HeadSupervisorUserAlertRecipients { get; set; }

    public DbSet<GatewayModem> GatewayModems { get; set; }

    public DbSet<TechnicalAdvisorUserGatewayModem> TechnicalAdvisorUserGatewayModems { get; set; }

    public DbSet<DataCollectorLocation> DataCollectorLocations { get; set; }

    public DbSet<AlertNotHandledNotificationRecipient> AlertNotHandledNotificationRecipients { get; set; }

    public DbSet<DataCollectorNotDeployed> DataCollectorNotDeployedDates { get; set; }

    public DbSet<ProjectErrorMessage> ProjectErrorMessages { get; set; }

    public DbSet<EidsrConfiguration> EidsrConfiguration { get; set; }

    public DbSet<EidsrOrganisationUnits> EidsrOrganisationUnits { get; set; }

    public Task ExecuteSqlInterpolatedAsync(FormattableString sql) =>
        Database.ExecuteSqlInterpolatedAsync(sql);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NyssContext).Assembly);
        modelBuilder.HasDefaultSchema("nyss");
        modelBuilder.Seed();
    }
}
