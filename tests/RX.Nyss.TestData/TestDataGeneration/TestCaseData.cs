using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;

namespace RX.Nyss.TestData.TestDataGeneration;

public class TestCaseData<T> : BaseTestCaseData
{
    private readonly Func<EntityData, T> _testCaseDefinition;
    public T AdditionalData { get; set; }

    public TestCaseData(INyssContext nyssContextMock, Func<EntityData, T> definition)
        : base(nyssContextMock)
    {
        _testCaseDefinition = definition;
    }

    public TestCaseData<T> GenerateData()
    {
        AdditionalData = _testCaseDefinition.Invoke(EntityData);
        return this;
    }
}

public class TestCaseData : BaseTestCaseData
{
    private readonly Action<EntityData> _testCaseDefinition;

    public TestCaseData(INyssContext nyssContextMock, Action<EntityData> definition)
        : base(nyssContextMock)
    {
        _testCaseDefinition = definition;
    }

    public TestCaseData GenerateData()
    {
        _testCaseDefinition.Invoke(EntityData);
        return this;
    }
}

public abstract class BaseTestCaseData
{
    private readonly INyssContext _nyssContextMock;
    public virtual EntityData EntityData { get; set; } = new EntityData();

    protected BaseTestCaseData(INyssContext nyssContextMock)
    {
        _nyssContextMock = nyssContextMock;
    }

    public void AddToDbContext(bool useInMemoryDb = false)
    {
        if (useInMemoryDb)
        {
            ConfigureNyssContextInMemory(EntityData);
        }
        else
        {
            ConfiugureNyssContext(EntityData);
        }
    }

    protected void ConfiugureNyssContext(EntityData data)
    {
        AddToNyssContext(data.Alerts);
        AddToNyssContext(data.AlertRecipients);
        AddToNyssContext(data.AlertReports);
        AddToNyssContext(data.AlertRules);
        AddToNyssContext(data.ApplicationLanguages);
        AddToNyssContext(data.ContentLanguages);
        AddToNyssContext(data.Countries);
        AddToNyssContext(data.DataCollectors);
        AddToNyssContext(data.Districts);
        AddToNyssContext(data.GatewaySettings);
        AddToNyssContext(data.NationalSocietyConsents);
        AddToNyssContext(data.HealthRisks);
        AddToNyssContext(data.HealthRiskLanguageContents);
        AddToNyssContext(data.Localizations);
        AddToNyssContext(data.LocalizedTemplates);
        AddToNyssContext(data.NationalSocieties);
        AddToNyssContext(data.Notifications);
        AddToNyssContext(data.Projects);
        AddToNyssContext(data.SupervisorUserProjects);
        AddToNyssContext(data.ProjectHealthRisks);
        AddToNyssContext(data.RawReports);
        AddToNyssContext(data.Regions);
        AddToNyssContext(data.Reports);
        AddToNyssContext(data.Users);
        AddToNyssContext(data.UserNationalSocieties);
        AddToNyssContext(data.Villages);
        AddToNyssContext(data.Zones);
        AddToNyssContext(data.Organizations);
        data.NyssContextMockedMethods?.Invoke(_nyssContextMock);
    }

    protected void ConfigureNyssContextInMemory(EntityData data)
    {
        _nyssContextMock.Database.EnsureDeleted();

        _nyssContextMock.Alerts.AddRange(data.Alerts);
        _nyssContextMock.AlertNotificationRecipients.AddRange(data.AlertRecipients);
        _nyssContextMock.AlertReports.AddRange(data.AlertReports);
        _nyssContextMock.AlertRules.AddRange(data.AlertRules);
        _nyssContextMock.ApplicationLanguages.AddRange(data.ApplicationLanguages);
        _nyssContextMock.ContentLanguages.AddRange(data.ContentLanguages);
        _nyssContextMock.Countries.AddRange(data.Countries);
        _nyssContextMock.DataCollectors.AddRange(data.DataCollectors);
        _nyssContextMock.Districts.AddRange(data.Districts);
        _nyssContextMock.GatewaySettings.AddRange(data.GatewaySettings);
        _nyssContextMock.NationalSocietyConsents.AddRange(data.NationalSocietyConsents);
        _nyssContextMock.HealthRisks.AddRange(data.HealthRisks);
        _nyssContextMock.HealthRiskLanguageContents.AddRange(data.HealthRiskLanguageContents);
        _nyssContextMock.Localizations.AddRange(data.Localizations);
        _nyssContextMock.LocalizedTemplates.AddRange(data.LocalizedTemplates);
        _nyssContextMock.NationalSocieties.AddRange(data.NationalSocieties);
        _nyssContextMock.Notifications.AddRange(data.Notifications);
        _nyssContextMock.Projects.AddRange(data.Projects);
        _nyssContextMock.SupervisorUserProjects.AddRange(data.SupervisorUserProjects);
        _nyssContextMock.ProjectHealthRisks.AddRange(data.ProjectHealthRisks);
        _nyssContextMock.RawReports.AddRange(data.RawReports);
        _nyssContextMock.Regions.AddRange(data.Regions);
        _nyssContextMock.Reports.AddRange(data.Reports);
        _nyssContextMock.Users.AddRange(data.Users);
        _nyssContextMock.UserNationalSocieties.AddRange(data.UserNationalSocieties);
        _nyssContextMock.Villages.AddRange(data.Villages);
        _nyssContextMock.Zones.AddRange(data.Zones);
        _nyssContextMock.Organizations.AddRange(data.Organizations);

        _nyssContextMock.SaveChanges();
        _nyssContextMock.Database.EnsureCreated();
    }

    private void AddToNyssContext<T>(List<T> entities) where T : class
    {
        if (entities == null)
        {
            return;
        }

        var properties = typeof(INyssContext).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var dbSetProperty = properties.Single(p => p.PropertyType == typeof(DbSet<T>));

        if (DbSetIsImplemented<T>(dbSetProperty))
        {
            var existingDbSet = dbSetProperty.GetValue(_nyssContextMock, null) as DbSet<T>;
            existingDbSet.AddRange(entities);
        }

        var newDbSet = entities.AsQueryable().BuildMockDbSet();
        dbSetProperty.GetValue(_nyssContextMock, null).Returns(newDbSet);
    }

    private bool DbSetIsImplemented<T>(PropertyInfo dbSetProperty) where T : class
    {
        var dbSet = dbSetProperty.GetValue(_nyssContextMock, null) as DbSet<T>;
        try
        {
            return dbSet != null;
        }
        catch (NotImplementedException)
        {
            return false;
        }
    }
}