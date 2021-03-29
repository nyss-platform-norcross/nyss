using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.ReportApi.Features.Reports.Contracts;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Features.Reports.Models;

namespace RX.Nyss.ReportApi.Features.Reports
{
    public interface IReportMessageService
    {
        Task<ParsedReport> ParseReport(string reportMessage);
    }

    public class ReportMessageService : IReportMessageService
    {
        private const int Male = 1;
        private const int Female = 2;
        private const int BelowFive = 1;
        private const int AtLeastFive = 2;

        private const string ReportPattern = @"^(?<healthRiskCode>[1-9][0-9]*)((?<separator>[#*])(?<sex>[1-2])\k<separator>(?<ageGroup>[1-2]))?$";

        private const string AggregatedReportPattern =
            @"^(?<healthRiskCode>[1-9][0-9]*)(?<separator>[#*])(?<malesBelowFive>[0-9]+)\k<separator>(?<malesAtLeastFive>[0-9]+)\k<separator>(?<femalesBelowFive>[0-9]+)\k<separator>(?<femalesAtLeastFive>[0-9]+)$";

        private const string DcpReportPattern =
            @"^(?<healthRiskCode>[1-9][0-9]*)(?<separator>[#*])(?<malesBelowFive>[0-9]+)\k<separator>(?<malesAtLeastFive>[0-9]+)\k<separator>(?<femalesBelowFive>[0-9]+)\k<separator>(?<femalesAtLeastFive>[0-9]+)\k<separator>(?<referredToHealthFacility>[0-9]+)\k<separator>(?<diedInOrp>[0-9]+)\k<separator>(?<cameFromOtherVillage>[0-9]+)$";

        private static readonly Regex ReportRegex = new Regex(ReportPattern, RegexOptions.Compiled);
        private static readonly Regex AggregatedReportRegex = new Regex(AggregatedReportPattern, RegexOptions.Compiled);
        private static readonly Regex DcpReportRegex = new Regex(DcpReportPattern, RegexOptions.Compiled);

        private readonly INyssContext _nyssContext;

        public ReportMessageService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public async Task<ParsedReport> ParseReport(string reportMessage)
        {
            if (string.IsNullOrWhiteSpace(reportMessage))
            {
                throw new ReportValidationException("A report cannot be empty.");
            }

            if (reportMessage.Length > 160)
            {
                throw new ReportValidationException($"A report cannot be longer than 160 characters (was {reportMessage.Length} characters)", ReportErrorType.TooLong);
            }

            if (ReportRegex.IsMatch(reportMessage))
            {
                return await ParseSingleOrEventReport(reportMessage);
            }

            if (AggregatedReportRegex.IsMatch(reportMessage))
            {
                return ParseAggregatedReport(reportMessage);
            }

            if (DcpReportRegex.IsMatch(reportMessage))
            {
                return ParseDcpReport(reportMessage);
            }

            throw new ReportValidationException("A report format was not recognized.", ReportErrorType.FormatError);
        }

        internal async Task<ParsedReport> ParseSingleOrEventReport(string reportMessage)
        {
            var reportMatch = ReportRegex.Match(reportMessage);
            var healthRiskCodeMatch = reportMatch.Groups["healthRiskCode"].Value;
            var sexMatch = reportMatch.Groups["sex"].Value;
            var ageGroupMatch = reportMatch.Groups["ageGroup"].Value;

            var healthRiskCode = int.Parse(healthRiskCodeMatch);
            var sex = sexMatch.ParseToNullableInt();
            var ageGroup = ageGroupMatch.ParseToNullableInt();

            var healthRisk = await _nyssContext.HealthRisks
                .Where(hr => hr.HealthRiskCode == healthRiskCode)
                .FirstOrDefaultAsync();

            if (healthRisk == null)
            {
                throw new ReportValidationException($"Health risk with code: {healthRiskCode} does not exist in the global list.", ReportErrorType.GlobalHealthRiskCodeNotFound);
            }

            if (healthRisk.HealthRiskType != HealthRiskType.Human)
            {
                return ParseEventReport(healthRiskCode, sex, ageGroup);
            }

            return ParseSingleReport(healthRiskCode, sex, ageGroup);
        }

        internal static ParsedReport ParseSingleReport(int healthRiskCode, int? sex, int? ageGroup)
        {
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
                        : 0,
                    CountUnspecifiedSexAndAge = !(sex.HasValue && ageGroup.HasValue)
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

        internal static ParsedReport ParseEventReport(int healthRiskCode, int? sex, int? ageGroup)
        {
            if (sex.HasValue || ageGroup.HasValue)
            {
                throw new ReportValidationException($"Sex and/or age can not be reported for event or non-human health risk: {healthRiskCode}.", ReportErrorType.FormatError);
            }

            var parsedReport = new ParsedReport
            {
                HealthRiskCode = healthRiskCode,
                ReportType = ReportType.Event
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
