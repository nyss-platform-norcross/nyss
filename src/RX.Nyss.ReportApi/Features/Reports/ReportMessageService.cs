using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Features.Reports.Models;

namespace RX.Nyss.ReportApi.Features.Reports;

public interface IReportMessageService
{
    Task<ParsedReport> ParseReport(string reportMessage);
}

public class ReportMessageService : IReportMessageService
{
    private const int _male = 1;
    private const int _female = 2;
    private const int _belowFive = 1;
    private const int _atLeastFive = 2;

    private const string _reportPattern = @"^(?<healthRiskCode>[1-9][0-9]*)((?<separator>[#*])(?<sex>[1-2])\k<separator>(?<ageGroup>[1-2]))?$";

    private const string _aggregatedReportPattern =
        @"^(?<healthRiskCode>[1-9][0-9]*)(?<separator>[#*])(?<malesBelowFive>[0-9]+)\k<separator>(?<malesAtLeastFive>[0-9]+)\k<separator>(?<femalesBelowFive>[0-9]+)\k<separator>(?<femalesAtLeastFive>[0-9]+)$";

    private const string _dcpReportPattern =
        @"^(?<healthRiskCode>[1-9][0-9]*)(?<separator>[#*])(?<malesBelowFive>[0-9]+)\k<separator>(?<malesAtLeastFive>[0-9]+)\k<separator>(?<femalesBelowFive>[0-9]+)\k<separator>(?<femalesAtLeastFive>[0-9]+)\k<separator>(?<referredToHealthFacility>[0-9]+)$";

    private static readonly Regex _reportRegex = new Regex(_reportPattern, RegexOptions.Compiled);
    private static readonly Regex _aggregatedReportRegex = new Regex(_aggregatedReportPattern, RegexOptions.Compiled);
    private static readonly Regex _dcpReportRegex = new Regex(_dcpReportPattern, RegexOptions.Compiled);

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

        var normalizedReportMessage = NormalizeReportMessage(reportMessage);
        if (string.IsNullOrWhiteSpace(normalizedReportMessage))
        {
            throw new ReportValidationException("A report cannot be empty.");
        }

        if (_reportRegex.IsMatch(normalizedReportMessage))
        {
            return await ParseSingleOrEventReport(normalizedReportMessage);
        }

        if (_aggregatedReportRegex.IsMatch(normalizedReportMessage))
        {
            return ParseAggregatedReport(normalizedReportMessage);
        }

        if (_dcpReportRegex.IsMatch(normalizedReportMessage))
        {
            return ParseDcpReport(normalizedReportMessage);
        }

        throw new ReportValidationException("A report format was not recognized.", ReportErrorType.FormatError);
    }

    private static string NormalizeReportMessage(string reportMessage) =>
        string.Concat(reportMessage.Where(c => !char.IsWhiteSpace(c)));

    internal async Task<ParsedReport> ParseSingleOrEventReport(string reportMessage)
    {
        var reportMatch = _reportRegex.Match(reportMessage);
        var healthRiskCodeMatch = reportMatch.Groups["healthRiskCode"].Value;
        var sexMatch = reportMatch.Groups["sex"].Value;
        var ageGroupMatch = reportMatch.Groups["ageGroup"].Value;

        if (!int.TryParse(healthRiskCodeMatch, out var healthRiskCode))
        {
            throw new ReportValidationException($"The health risk code: {healthRiskCodeMatch} could not be parsed", ReportErrorType.FormatError);
        }

        var sex = sexMatch.ParseToNullableInt();
        var ageGroup = ageGroupMatch.ParseToNullableInt();

        var healthRiskType = await _nyssContext.HealthRisks
            .Where(hr => hr.HealthRiskCode == healthRiskCode)
            .Select(hr => (HealthRiskType?)hr.HealthRiskType)
            .FirstOrDefaultAsync();

        if (!healthRiskType.HasValue)
        {
            throw new ReportValidationException($"Health risk with code: {healthRiskCode} does not exist in the global list.", ReportErrorType.GlobalHealthRiskCodeNotFound);
        }

        if (healthRiskType.Value != HealthRiskType.Human)
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
                CountMalesBelowFive = sex == _male && ageGroup == _belowFive
                    ? 1
                    : 0,
                CountMalesAtLeastFive = sex == _male && ageGroup == _atLeastFive
                    ? 1
                    : 0,
                CountFemalesBelowFive = sex == _female && ageGroup == _belowFive
                    ? 1
                    : 0,
                CountFemalesAtLeastFive = sex == _female && ageGroup == _atLeastFive
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
        var aggregatedReportMatch = _aggregatedReportRegex.Match(reportMessage);
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
                CountFemalesAtLeastFive = femalesAtLeastFive,
                CountUnspecifiedSexAndAge = 0
            }
        };

        return parsedReport;
    }

    internal static ParsedReport ParseEventReport(int healthRiskCode, int? sex, int? ageGroup)
    {
        if (sex.HasValue || ageGroup.HasValue)
        {
            throw new ReportValidationException($"Sex and/or age can not be reported for event or non-human health risk: {healthRiskCode}.", ReportErrorType.GenderAndAgeNonHumanHealthRisk);
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
        var dcpReportMatch = _dcpReportRegex.Match(reportMessage);
        var healthRiskCodeMatch = dcpReportMatch.Groups["healthRiskCode"].Value;
        var malesBelowFiveMatch = dcpReportMatch.Groups["malesBelowFive"].Value;
        var malesAtLeastFiveMatch = dcpReportMatch.Groups["malesAtLeastFive"].Value;
        var femalesBelowFiveMatch = dcpReportMatch.Groups["femalesBelowFive"].Value;
        var femalesAtLeastFiveMatch = dcpReportMatch.Groups["femalesAtLeastFive"].Value;
        var referredToHealthFacilityMatch = dcpReportMatch.Groups["referredToHealthFacility"].Value;

        var healthRiskCode = int.Parse(healthRiskCodeMatch);
        var malesBelowFive = int.Parse(malesBelowFiveMatch);
        var malesAtLeastFive = int.Parse(malesAtLeastFiveMatch);
        var femalesBelowFive = int.Parse(femalesBelowFiveMatch);
        var femalesAtLeastFive = int.Parse(femalesAtLeastFiveMatch);
        var referredToHealthFacility = int.Parse(referredToHealthFacilityMatch);

        var parsedReport = new ParsedReport
        {
            HealthRiskCode = healthRiskCode,
            ReportType = ReportType.DataCollectionPoint,
            ReportedCase =
            {
                CountMalesBelowFive = malesBelowFive,
                CountMalesAtLeastFive = malesAtLeastFive,
                CountFemalesBelowFive = femalesBelowFive,
                CountFemalesAtLeastFive = femalesAtLeastFive,
                CountUnspecifiedSexAndAge = 0
            },
            DataCollectionPointCase =
            {
                ReferredCount = referredToHealthFacility
            }
        };

        return parsedReport;
    }
}
