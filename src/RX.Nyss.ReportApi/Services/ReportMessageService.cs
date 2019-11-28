using System.Text.RegularExpressions;
using RX.Nyss.Data.Concepts;
using RX.Nyss.ReportApi.Exceptions;
using RX.Nyss.ReportApi.Models;

namespace RX.Nyss.ReportApi.Services
{
    public interface IReportMessageService
    {
        ParsedReport ParseReport(string reportMessage);
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

        public ParsedReport ParseReport(string reportMessage)
        {
            if (SingleReportRegex.IsMatch(reportMessage))
            {
                return ParseSingleReport(reportMessage);
            }

            if (AggregatedReportRegex.IsMatch(reportMessage))
            {
                return ParseAggregatedReport(reportMessage);
            }

            if (EventReportRegex.IsMatch(reportMessage))
            {
                return ParseEventReport(reportMessage);
            }

            throw new ReportValidationException("A report format was not recognized.");
        }

        internal static ParsedReport ParseSingleReport(string reportMessage)
        {
            var singleReportMatch = SingleReportRegex.Match(reportMessage);
            var healthRiskCodeMatch = singleReportMatch.Groups["healthRiskCode"].Value;
            var sexMatch = singleReportMatch.Groups["sex"].Value;
            var ageGroupMatch = singleReportMatch.Groups["ageGroup"].Value;

            var healthRiskCodeParsedSuccessfully = int.TryParse(healthRiskCodeMatch, out var healthRiskCode);
            var sexParsedSuccessfully = int.TryParse(sexMatch, out var sex);
            var ageGroupParsedSuccessfully = int.TryParse(ageGroupMatch, out var ageGroup);

            if (!healthRiskCodeParsedSuccessfully)
            {
                throw new ReportValidationException($"Cannot parse health risk code '{healthRiskCodeMatch}'.");
            }

            if (!sexParsedSuccessfully)
            {
                throw new ReportValidationException($"Cannot parse sex '{sexMatch}'.");
            }

            if (!ageGroupParsedSuccessfully)
            {
                throw new ReportValidationException($"Cannot parse age group '{ageGroup}'.");
            }

            var parsedReport = new ParsedReport
            {
                HealthRiskCode = healthRiskCode,
                ReportType = ReportType.Single,
                DataCollectorType = DataCollectorType.Human,
                ReportedCase =
                {
                    CountMalesBelowFive = sex == Male && ageGroup == BelowFive ? 1 : 0,
                    CountMalesAtLeastFive = sex == Male && ageGroup == AtLeastFive ? 1 : 0,
                    CountFemalesBelowFive = sex == Female && ageGroup == BelowFive ? 1 : 0,
                    CountFemalesAtLeastFive = sex == Female && ageGroup == AtLeastFive ? 1 : 0
                }
            };

            return parsedReport;
        }

        internal static ParsedReport ParseAggregatedReport(string reportMessage)
        {
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

            if (!healthRiskCodeParsedSuccessfully)
            {
                throw new ReportValidationException($"Cannot parse health risk code '{healthRiskCodeMatch}'.");
            }

            if (!malesBelowFiveParsedSuccessfully)
            {
                throw new ReportValidationException($"Cannot parse number of males below five '{malesBelowFiveMatch}'.");
            }

            if (!malesAtLeastFiveParsedSuccessfully)
            {
                throw new ReportValidationException($"Cannot parse number of males at least five '{malesAtLeastFiveMatch}'.");
            }

            if (!femalesBelowFiveParsedSuccessfully)
            {
                throw new ReportValidationException($"Cannot parse number of females below five '{femalesBelowFiveMatch}'.");
            }

            if (!femalesAtLeastFiveParsedSuccessfully)
            {
                throw new ReportValidationException($"Cannot parse number of females at least five '{femalesAtLeastFiveMatch}'.");
            }

            var parsedReport = new ParsedReport
            {
                HealthRiskCode = healthRiskCode,
                ReportType = ReportType.Aggregate,
                DataCollectorType = DataCollectorType.Human,
                ReportedCase =
                {
                    CountMalesBelowFive = malesBelowFive,
                    CountMalesAtLeastFive = malesAtLeastFive,
                    CountFemalesBelowFive = femalesBelowFive,
                    CountFemalesAtLeastFive = femalesAtLeastFive
                }
            };

            return parsedReport;
        }

        private static ParsedReport ParseEventReport(string reportMessage)
        {
            var eventReportMatch = EventReportRegex.Match(reportMessage);
            var eventCodeMatch = eventReportMatch.Groups["eventCode"].Value;

            var eventCodeParsedSuccessfully = int.TryParse(eventCodeMatch, out var eventCode);

            if (!eventCodeParsedSuccessfully)
            {
                throw new ReportValidationException($"Cannot parse event code '{eventCodeMatch}'.");
            }

            var parsedReport = new ParsedReport
            {
                HealthRiskCode = eventCode,
                ReportType = eventCode == ActivityCode ? ReportType.Activity : ReportType.NonHuman,
                DataCollectorType = DataCollectorType.Human
            };

            return parsedReport;
        }
    }
}
