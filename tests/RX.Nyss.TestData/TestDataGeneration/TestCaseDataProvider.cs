using System;
using System.Collections.Generic;
using RX.Nyss.Data;

namespace RX.Nyss.TestData.TestDataGeneration
{
    public class TestCaseDataProvider
    {
        private readonly Dictionary<string, BaseTestCaseData> _testCases = new Dictionary<string, BaseTestCaseData>();
        private readonly INyssContext _nyssContextMock;

        public TestCaseDataProvider(INyssContext nyssContextMock)
        {
            _nyssContextMock = nyssContextMock;
        }

        public TestCaseData GetOrCreate(string label, Action<EntityData> testCaseDefinition)
        {
            if (_testCases.ContainsKey(label))
            {
                return (TestCaseData)_testCases[label];
            }

            var newTestCaseData = new TestCaseData(_nyssContextMock, testCaseDefinition);
            _testCases.Add(label, newTestCaseData);
            return newTestCaseData;
        }

        public TestCaseData<T> GetOrCreate<T>(string label, Func<EntityData, T> testCaseDefinition)
        {
            if (_testCases.ContainsKey(label))
            {
                return (TestCaseData<T>)_testCases[label];
            }

            var newTestCaseData = new TestCaseData<T>(_nyssContextMock, testCaseDefinition);
            _testCases.Add(label, newTestCaseData);
            return newTestCaseData;
        }
    }
}
