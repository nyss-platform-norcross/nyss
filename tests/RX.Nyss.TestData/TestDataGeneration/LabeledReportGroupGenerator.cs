using System;

namespace RX.Nyss.TestData.TestDataGeneration
{
    public class LabeledReportGroupGenerator
    {
        private readonly EntityNumerator _reportNumerator;

        public LabeledReportGroupGenerator()
        {
            _reportNumerator = new EntityNumerator();
        }

        public LabeledReportGroup Create(string label) =>
            new LabeledReportGroup(_reportNumerator, Guid.Parse(label));
    }
}
