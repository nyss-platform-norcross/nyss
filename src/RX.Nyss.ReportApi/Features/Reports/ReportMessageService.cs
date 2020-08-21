using System.Text.RegularExpressions;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.ReportApi.Features.Reports.Contracts;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Features.Reports.Models;

namespace RX.Nyss.ReportApi.Features.Reports
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

        private const string SingleReportPattern = @"^(?<healthRiskCode>[1-9][0-9]*)(?<separator>[#*])(?<sex>[1-2])\k<separator>(?<ageGroup>[1-2])$";

        private const string AggregatedReportPattern =
            @"^(?<healthRiskCode>[1-9][0-9]*)(?<separator>[#*])(?<malesBelowFive>[0-9]+)\k<separator>(?<malesAtLeastFive>[0-9]+)\k<separator>(?<femalesBelowFive>[0-9]+)\k<separator>(?<femalesAtLeastFive>[0-9]+)$";

        private const string EventReportPattern = @"^(?<eventCode>[1-9][0-9]*)$";

        private const string DcpReportPattern =
            @"^(?<healthRiskCode>[1-9][0-9]*)(?<separator>[#*])(?<malesBelowFive>[0-9]+)\k<separator>(?<malesAtLeastFive>[0-9]+)\k<separator>(?<femalesBelowFive>[0-9]+)\k<separator>(?<femalesAtLeastFive>[0-9]+)\k<separator>(?<referredToHealthFacility>[0-9]+)\k<separator>(?<diedInOrp>[0-9]+)\k<separator>(?<cameFromOtherVillage>[0-9]+)$";

        private static readonly Regex SingleReportRegex = new Regex(SingleReportPattern, RegexOptions.Compiled);
        private static readonly Regex AggregatedReportRegex = new Regex(AggregatedReportPattern, RegexOptions.Compiled);
        private static readonly Regex EventReportRegex = new Regex(EventReportPattern, RegexOptions.Compiled);
        private static readonly Regex DcpReportRegex = new Regex(DcpReportPattern, RegexOptions.Compiled);

        private readonly INyssContext _nyssContext;

        public ReportMessageService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public ParsedReport ParseReport(string reportMessage)
        {
            if (string.IsNullOrWhiteSpace(reportMessage))
            {
                throw new ReportValidationException("A report cannot be empty.");
            }

            if (reportMessage.Length > 160)
            {
                throw new ReportValidationException("A report cannot be longer than 160 characters.");
            }

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

            if (DcpReportRegex.IsMatch(reportMessage))
            {
                return ParseDcpReport(reportMessage);
            }

            throw new ReportValidationException("A report format was not recognized.", ReportErrorType.FormatError);
        }

        internal static ParsedReport ParseSingleReport(string reportMessage)
        {
            var singleReportMatch = SingleReportRegex.Match(reportMessage);
            var healthRiskCodeMatch = singleReportMatch.Groups["healthRiskCode"].Value;
            var sexMatch = singleReportMatch.Groups["sex"].Value;
            var ageGroupMatch = singleReportMatch.Groups["ageGroup"].Value;

            var healthRiskCode = int.Parse(healthRiskCodeMatch);
            var sex = int.Parse(sexMatch);
            var ageGroup = int.Parse(ageGroupMatch);

            var parsedReport = new ParsedReport
            {
                HealthRiskCode = healthRiskCode,
                ReportType = ReportType.Single,
                ReportedCase =
                {
                    CountMalesBelowFive = sex == Male && ageGroup == BelowFive
                        ? 1
                        : 0,
                    CountMalesAtLeastFive = sex == Male && ageGroup == AtLeastFive
                        ? 1
                        : 0,
                    CountFemalesBelowFive = sex == Female && ageGroup == BelowFive
                        ? 1
                        : 0,
                    CountFemalesAtLeastFive = sex == Female && ageGroup == AtLeastFive
                        ? 1
                        : 0
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

            var healthRiskCode = int.Parse(healthRiskCodeMatch);
            var malesBelowFive = int.Parse(malesBelowFiveMatch);
            var malesAtLeastFive = int.Parse(malesAtLeastFiveMatch);
            var femalesBelowFive = int.Parse(femalesBelowFiveMatch);
            var femalesAtLeastFive = int.Parse(femalesAtLeastFiveMatch);

            var parsedReport = new ParsedReport
            {
                HealthRiskCode = healthRiskCode,
                ReportType = ReportType.Aggregate,
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

        internal ParsedReport ParseEventReport(string reportMessage)
        {
            var eventReportMatch = EventReportRegex.Match(reportMessage);
            var eventCodeMatch = eventReportMatch.Groups["eventCode"].Value;

            var eventCode = int.Parse(eventCodeMatch);

            var parsedReport = new ParsedReport
            {
                HealthRiskCode = eventCode,
                ReportType = ReportType.Statement
            };

            return parsedReport;
        }

        internal static ParsedReport ParseDcpReport(string reportMessage)
        {
            var dcpReportMatch = DcpReportRegex.Match(reportMessage);
            var healthRiskCodeMatch = dcpReportMatch.Groups["healthRiskCode"].Value;
            var malesBelowFiveMatch = dcpReportMatch.Groups["malesBelowFive"].Value;
            var malesAtLeastFiveMatch = dcpReportMatch.Groups["malesAtLeastFive"].Value;
            var femalesBelowFiveMatch = dcpReportMatch.Groups["femalesBelowFive"].Value;
            var femalesAtLeastFiveMatch = dcpReportMatch.Groups["femalesAtLeastFive"].Value;
            var referredToHealthFacilityMatch = dcpReportMatch.Groups["referredToHealthFacility"].Value;
            var diedInOrpMatch = dcpReportMatch.Groups["diedInOrp"].Value;
            var cameFromOtherVillageMatch = dcpReportMatch.Groups["cameFromOtherVillage"].Value;

            var healthRiskCode = int.Parse(healthRiskCodeMatch);
            var malesBelowFive = int.Parse(malesBelowFiveMatch);
            var malesAtLeastFive = int.Parse(malesAtLeastFiveMatch);
            var femalesBelowFive = int.Parse(femalesBelowFiveMatch);
            var femalesAtLeastFive = int.Parse(femalesAtLeastFiveMatch);
            var referredToHealthFacility = int.Parse(referredToHealthFacilityMatch);
            var diedInOrp = int.Parse(diedInOrpMatch);
            var cameFromOtherVillage = int.Parse(cameFromOtherVillageMatch);

            var parsedReport = new ParsedReport
            {
                HealthRiskCode = healthRiskCode,
                ReportType = ReportType.DataCollectionPoint,
                ReportedCase =
                {
                    CountMalesBelowFive = malesBelowFive,
                    CountMalesAtLeastFive = malesAtLeastFive,
                    CountFemalesBelowFive = femalesBelowFive,
                    CountFemalesAtLeastFive = femalesAtLeastFive
                },
                DataCollectionPointCase =
                {
                    ReferredCount = referredToHealthFacility,
                    DeathCount = diedInOrp,
                    FromOtherVillagesCount = cameFromOtherVillage
                }
            };

            return parsedReport;
        }
    }
}
