using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;

namespace RX.Nyss.TestData.TestDataGeneration
{
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

        public void AddToDbContext() =>
            ConfiugureNyssContext(EntityData);

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

            data.NyssContextMockedMethods?.Invoke(_nyssContextMock);
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
                entities.AddRange(existingDbSet.ToList());
            }

            var newDbSet = entities.AsQueryable().BuildMockDbSet();
            dbSetProperty.GetValue(_nyssContextMock, null).Returns(newDbSet);
        }

        private bool DbSetIsImplemented<T>(PropertyInfo dbSetProperty) where T : class
        {
            var dbSet = dbSetProperty.GetValue(_nyssContextMock, null) as DbSet<T>;
            try
            {
                dbSet.Any();
                return true;
            }
            catch (NotImplementedException)
            {
                return false;
            }
        }
    }
}
