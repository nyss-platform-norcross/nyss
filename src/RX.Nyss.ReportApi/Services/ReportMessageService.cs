using System.Text.RegularExpressions;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.ReportApi.Services
{
    public interface IReportMessageService
    {
        (bool reportParsedSuccessfully, ReportMessageService.ParsedReport parsedReport) ParseReport(string reportMessage);
    }

    public class ReportMessageService : IReportMessageService
    {
        private const int Male = 1;
        private const int Female = 2;
        private const int BelowFive = 1;
        private const int AtLeastFive = 2;
        private const int ActivityCode = 99;

        private const string SingleReportPattern = @"^(?<healthRiskCode>[1-9][0-9]*)(?<separator>[#*])(?<sex>[1-2])\k<separator>(?<ageGroup>[1-2])$";
        private static readonly Regex SingleReportRegex = new Regex(SingleReportPattern, RegexOptions.Compiled);

        private const string AggregatedReportPattern = @"^(?<healthRiskCode>[1-9][0-9]*)(?<separator>[#*])(?<malesBelowFive>[0-9]+)\k<separator>(?<malesAtLeastFive>[0-9]+)\k<separator>(?<femalesBelowFive>[0-9]+)\k<separator>(?<femalesAtLeastFive>[0-9]+)$";
        private static readonly Regex AggregatedReportRegex = new Regex(AggregatedReportPattern, RegexOptions.Compiled);

        private const string EventReportPattern = @"^(?<eventCode>[1-9][0-9]*)$";
        private static readonly Regex EventReportRegex = new Regex(EventReportPattern, RegexOptions.Compiled);

        public (bool reportParsedSuccessfully, ParsedReport parsedReport) ParseReport(string reportMessage)
        {
            bool isValid;
            var parsedReportMessage = new ParsedReport();
            
            if (SingleReportRegex.IsMatch(reportMessage))
            {
                isValid = ParseSingleReport(reportMessage, parsedReportMessage);
            }
            else if (AggregatedReportRegex.IsMatch(reportMessage))
            {
                isValid = ParseAggregatedReport(reportMessage, parsedReportMessage);
            }
            else if (EventReportRegex.IsMatch(reportMessage))
            {
                isValid = ParseEventReport(reportMessage, parsedReportMessage);
            }
            else
            {
                isValid = false;
            }

            return (isValid, parsedReportMessage);
        }

        internal static bool ParseSingleReport(string reportMessage, ParsedReport report)
        {
            var isValid = true;

            var singleReportMatch = SingleReportRegex.Match(reportMessage);
            var healthRiskCodeMatch = singleReportMatch.Groups["healthRiskCode"].Value;
            var sexMatch = singleReportMatch.Groups["sex"].Value;
            var ageGroupMatch = singleReportMatch.Groups["ageGroup"].Value;

            var healthRiskCodeParsedSuccessfully = int.TryParse(healthRiskCodeMatch, out var healthRiskCode);
            var sexParsedSuccessfully = int.TryParse(sexMatch, out var sex);
            var ageGroupParsedSuccessfully = int.TryParse(ageGroupMatch, out var ageGroup);

            isValid &= healthRiskCodeParsedSuccessfully;
            isValid &= sexParsedSuccessfully;
            isValid &= ageGroupParsedSuccessfully;

            if (sexParsedSuccessfully && ageGroupParsedSuccessfully)
            {
                report.ReportedCase.CountMalesBelowFive = sex == Male && ageGroup == BelowFive ? 1 : 0;
                report.ReportedCase.CountMalesAtLeastFive = sex == Male && ageGroup == AtLeastFive ? 1 : 0;
                report.ReportedCase.CountFemalesBelowFive = sex == Female && ageGroup == BelowFive ? 1 : 0;
                report.ReportedCase.CountFemalesAtLeastFive = sex == Female && ageGroup == AtLeastFive ? 1 : 0;
            }

            report.HealthRiskCode = healthRiskCodeParsedSuccessfully ? (int?) healthRiskCode : null;
            report.ReportType = ReportType.Single;
            report.DataCollectorType = DataCollectorType.Human;

            return isValid;
        }

        internal static bool ParseAggregatedReport(string reportMessage, ParsedReport report)
        {
            var isValid = true;

            var aggregatedReportMatch = AggregatedReportRegex.Match(reportMessage);
            var healthRiskCodeMatch = aggregatedReportMatch.Groups["healthRiskCode"].Value;
            var malesBelowFiveMatch = aggregatedReportMatch.Groups["malesBelowFive"].Value;
            var malesAtLeastFiveMatch = aggregatedReportMatch.Groups["malesAtLeastFive"].Value;
            var femalesBelowFiveMatch = aggregatedReportMatch.Groups["femalesBelowFive"].Value;
            var femalesAtLeastFiveMatch = aggregatedReportMatch.Groups["femalesAtLeastFive"].Value;

            var healthRiskCodeParsedSuccessfully = int.TryParse(healthRiskCodeMatch, out var healthRiskCode);
            var malesBelowFiveParsedSuccessfully = int.TryParse(malesBelowFiveMatch, out var malesBelowFive);
            var malesAtLeastFiveParsedSuccessfully = int.TryParse(malesAtLeastFiveMatch, out var malesAtLeastFive);
            var femalesBelowFiveParsedSuccessfully = int.TryParse(femalesBelowFiveMatch, out var femalesBelowFive);
            var femalesAtLeastFiveParsedSuccessfully = int.TryParse(femalesAtLeastFiveMatch, out var femalesAtLeastFive);

            isValid &= healthRiskCodeParsedSuccessfully;
            isValid &= malesBelowFiveParsedSuccessfully;
            isValid &= malesAtLeastFiveParsedSuccessfully;
            isValid &= femalesBelowFiveParsedSuccessfully;
            isValid &= femalesAtLeastFiveParsedSuccessfully;

            report.ReportedCase.CountMalesBelowFive = malesBelowFiveParsedSuccessfully ? (int?) malesBelowFive : null;
            report.ReportedCase.CountMalesAtLeastFive = malesAtLeastFiveParsedSuccessfully ? (int?) malesAtLeastFive : null;
            report.ReportedCase.CountFemalesBelowFive = femalesBelowFiveParsedSuccessfully ? (int?) femalesBelowFive : null;
            report.ReportedCase.CountFemalesAtLeastFive = femalesAtLeastFiveParsedSuccessfully ? (int?) femalesAtLeastFive : null;

            report.HealthRiskCode = healthRiskCodeParsedSuccessfully ? (int?) healthRiskCode : null;
            report.ReportType = ReportType.Aggregate;
            report.DataCollectorType = DataCollectorType.Human;

            return isValid;
        }

        private static bool ParseEventReport(string reportMessage, ParsedReport report)
        {
            var isValid = true;

            var eventReportMatch = EventReportRegex.Match(reportMessage);
            var eventCodeMatch = eventReportMatch.Groups["eventCode"].Value;

            var eventCodeParsedSuccessfully = int.TryParse(eventCodeMatch, out var eventCode);

            isValid &= eventCodeParsedSuccessfully;

            report.HealthRiskCode = eventCodeParsedSuccessfully ? (int?) eventCode : null;
            report.ReportType = eventCode == ActivityCode ? ReportType.Activity : ReportType.NonHuman;
            report.DataCollectorType = DataCollectorType.Human;

            return isValid;
        }

        public class ParsedReport
        {
            public int? HealthRiskCode { get; set; }

            public ReportType ReportType { get; set; }

            public ReportCase ReportedCase { get; set; } = new ReportCase();

            public DataCollectorType DataCollectorType { get; set; }

            public DataCollectionPointCase DataCollectionPointCase { get; set; } = new DataCollectionPointCase();
        }
    }
}
