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
        public T AdditionalData { get; set; }
        private readonly Func<(EntityData, T)> _testCaseDefinition;

        public TestCaseData(INyssContext nyssContextMock, Func<(EntityData, T)> definition)
            : base(nyssContextMock)
        {
            _testCaseDefinition = definition;
        }

        public void GenerateData()
        {
            (EntityData, AdditionalData) = _testCaseDefinition.Invoke();
            ConfiugureNyssContext(EntityData);
        }
    }

    public class TestCaseData : BaseTestCaseData
    {
        private readonly Func<EntityData> _testCaseDefinition;

        public TestCaseData(INyssContext nyssContextMock, Func<EntityData> definition)
            : base(nyssContextMock)
        {
            _testCaseDefinition = definition;
        }

        public void GenerateData()
        {
            EntityData = _testCaseDefinition.Invoke();
            ConfiugureNyssContext(EntityData);
        }
    }

    public abstract class BaseTestCaseData
    {
        private readonly INyssContext _nyssContextMock;
        public virtual EntityData EntityData { get; set; }

        protected BaseTestCaseData(INyssContext nyssContextMock)
        {
            _nyssContextMock = nyssContextMock;
        }

        protected void ConfiugureNyssContext(EntityData data)
        {
            AddToNyssContext(data.HealthRisks);
            AddToNyssContext(data.ProjectHealthRisks);
            AddToNyssContext(data.AlertRules);
            AddToNyssContext(data.Reports);
            AddToNyssContext(data.Alerts);
            AddToNyssContext(data.AlertReports);
        }

        private void AddToNyssContext<T>(List<T> entities) where T : class
        {
            if (entities == null)
            {
                return;
            }

            var dbSet = entities.AsQueryable().BuildMockDbSet();

            var properties = typeof(INyssContext).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var dbSetProperty = properties.Single(p => p.PropertyType == typeof(DbSet<T>));

            dbSetProperty.GetValue(_nyssContextMock, null).Returns(dbSet);
        }
    }
}
