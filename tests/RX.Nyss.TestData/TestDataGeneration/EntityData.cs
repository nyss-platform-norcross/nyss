using System;
using System.Collections.Generic;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;

namespace RX.Nyss.TestData.TestDataGeneration;

public class EntityData
{
    public Action<INyssContext> NyssContextMockedMethods { get; set; }

    public List<Alert> Alerts { get; set; } = new List<Alert>();
    public List<AlertNotificationRecipient> AlertRecipients { get; set; } = new List<AlertNotificationRecipient>();
    public List<AlertReport> AlertReports { get; set; } = new List<AlertReport>();
    public List<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
    public List<ApplicationLanguage> ApplicationLanguages { get; set; } = new List<ApplicationLanguage>();
    public List<ContentLanguage> ContentLanguages { get; set; } = new List<ContentLanguage>();
    public List<Country> Countries { get; set; } = new List<Country>();
    public List<DataCollector> DataCollectors { get; set; } = new List<DataCollector>();
    public List<District> Districts { get; set; } = new List<District>();
    public List<GatewaySetting> GatewaySettings { get; set; } = new List<GatewaySetting>();
    public List<NationalSocietyConsent> NationalSocietyConsents { get; set; } = new List<NationalSocietyConsent>();
    public List<HealthRisk> HealthRisks { get; set; } = new List<HealthRisk>();
    public List<HealthRiskLanguageContent> HealthRiskLanguageContents { get; set; } = new List<HealthRiskLanguageContent>();
    public List<Localization> Localizations { get; set; } = new List<Localization>();
    public List<LocalizedTemplate> LocalizedTemplates { get; set; } = new List<LocalizedTemplate>();
    public List<NationalSociety> NationalSocieties { get; set; } = new List<NationalSociety>();
    public List<Notification> Notifications { get; set; } = new List<Notification>();
    public List<Project> Projects { get; set; } = new List<Project>();
    public List<SupervisorUserProject> SupervisorUserProjects { get; set; } = new List<SupervisorUserProject>();
    public List<ProjectHealthRisk> ProjectHealthRisks { get; set; } = new List<ProjectHealthRisk>();
    public List<RawReport> RawReports { get; set; } = new List<RawReport>();
    public List<Region> Regions { get; set; } = new List<Region>();
    public List<Report> Reports { get; set; } = new List<Report>();
    public List<User> Users { get; set; } = new List<User>();
    public List<UserNationalSociety> UserNationalSocieties { get; set; } = new List<UserNationalSociety>();
    public List<Village> Villages { get; set; } = new List<Village>();
    public List<Zone> Zones { get; set; } = new List<Zone>();
    public List<Organization> Organizations { get; set; } = new List<Organization>();

    public void Include(EntityData otherData)
    {
        Alerts.AddRange(otherData.Alerts);
        AlertRecipients.AddRange(otherData.AlertRecipients);
        AlertReports.AddRange(otherData.AlertReports);
        AlertRules.AddRange(otherData.AlertRules);
        ApplicationLanguages.AddRange(otherData.ApplicationLanguages);
        ContentLanguages.AddRange(otherData.ContentLanguages);
        Countries.AddRange(otherData.Countries);
        DataCollectors.AddRange(otherData.DataCollectors);
        Districts.AddRange(otherData.Districts);
        GatewaySettings.AddRange(otherData.GatewaySettings);
        NationalSocietyConsents.AddRange(otherData.NationalSocietyConsents);
        HealthRisks.AddRange(otherData.HealthRisks);
        HealthRiskLanguageContents.AddRange(otherData.HealthRiskLanguageContents);
        Localizations.AddRange(otherData.Localizations);
        LocalizedTemplates.AddRange(otherData.LocalizedTemplates);
        NationalSocieties.AddRange(otherData.NationalSocieties);
        Notifications.AddRange(otherData.Notifications);
        Projects.AddRange(otherData.Projects);
        SupervisorUserProjects.AddRange(otherData.SupervisorUserProjects);
        ProjectHealthRisks.AddRange(otherData.ProjectHealthRisks);
        RawReports.AddRange(otherData.RawReports);
        Regions.AddRange(otherData.Regions);
        Reports.AddRange(otherData.Reports);
        Users.AddRange(otherData.Users);
        UserNationalSocieties.AddRange(otherData.UserNationalSocieties);
        Villages.AddRange(otherData.Villages);
        Zones.AddRange(otherData.Zones);
        Organizations.AddRange(otherData.Organizations);
    }
}
