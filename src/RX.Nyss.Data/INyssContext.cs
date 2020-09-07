using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data
{
    public interface INyssContext
    {
        DbSet<Alert> Alerts { get; set; }
        DbSet<AlertNotificationRecipient> AlertNotificationRecipients { get; set; }
        DbSet<AlertReport> AlertReports { get; set; }
        DbSet<AlertRule> AlertRules { get; set; }
        DbSet<ApplicationLanguage> ApplicationLanguages { get; set; }
        DbSet<ContentLanguage> ContentLanguages { get; set; }
        DbSet<Country> Countries { get; set; }
        DbSet<DataCollector> DataCollectors { get; set; }
        DbSet<District> Districts { get; set; }
        DbSet<GatewaySetting> GatewaySettings { get; set; }
        DbSet<NationalSocietyConsent> NationalSocietyConsents { get; set; }
        DbSet<HealthRisk> HealthRisks { get; set; }
        DbSet<HealthRiskLanguageContent> HealthRiskLanguageContents { get; set; }
        DbSet<Localization> Localizations { get; set; }
        DbSet<LocalizedTemplate> LocalizedTemplates { get; set; }
        DbSet<NationalSociety> NationalSocieties { get; set; }
        DbSet<Notification> Notifications { get; set; }
        DbSet<Project> Projects { get; set; }
        DbSet<SupervisorUserProject> SupervisorUserProjects { get; set; }
        DbSet<SupervisorUserAlertRecipient> SupervisorUserAlertRecipients { get; set; }
        DbSet<ProjectHealthRiskAlertRecipient> ProjectHealthRiskAlertRecipients { get; set; }
        DbSet<ProjectHealthRisk> ProjectHealthRisks { get; set; }
        DbSet<RawReport> RawReports { get; set; }
        DbSet<Region> Regions { get; set; }
        DbSet<Report> Reports { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<UserNationalSociety> UserNationalSocieties { get; set; }
        DbSet<Village> Villages { get; set; }
        DbSet<Zone> Zones { get; set; }
        DatabaseFacade Database { get; }
        DbSet<Organization> Organizations { get; set; }
        DbSet<ProjectOrganization> ProjectOrganizations { get; set; }
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
        Task ExecuteSqlInterpolatedAsync(FormattableString sql);
    }
}
