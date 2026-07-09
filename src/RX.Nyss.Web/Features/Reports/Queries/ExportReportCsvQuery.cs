using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Reports.Queries
{
    public class ExportReportCsvQuery : IExportReportQuery
    {
        public ExportReportCsvQuery(int projectId, ReportListFilterRequestDto filter)
        {
            ProjectId = projectId;
            Filter = filter;
        }

        public int ProjectId { get; }
        public ReportListFilterRequestDto Filter { get; }

        public class Handler : ExportReportBaseHandler<ExportReportCsvQuery>
        {
            private readonly IExcelExportService _excelExportService;

            public Handler(IReportExportService reportExportService, IStringsService stringsService, IExcelExportService excelExportService)
                : base(reportExportService, stringsService)
            {
                _excelExportService = excelExportService;
            }

            public override async Task<FileResultDto> Handle(ExportReportCsvQuery request, CancellationToken cancellationToken)
            {
                var strings = await FetchStrings();
                var reports = await FetchData(request);

                var data = request.Filter.FormatCorrect
                    ? GetCorrectReportsData(reports, strings, request.Filter.DataCollectorType)
                    : GetIncorrectReportsData(reports, strings);

                return new FileResultDto(data, MimeTypes.Csv);
            }

            private byte[] GetIncorrectReportsData(
                IReadOnlyList<ExportReportListResponseDto> reports,
                StringsResourcesVault strings)
            {
                var columnLabels = GetIncorrectReportsColumnLabels(strings);

                var reportData = reports
                    .Select(r => new ExportIncorrectReportListCsvContentDto
                    {
                        Id = r.Id,
                        Date = r.DateTime.ToString("yyyy-MM-dd"),
                        Time = r.DateTime.ToString("HH:mm"),
                        EpiYear = r.EpiYear,
                        EpiWeek = r.EpiWeek,
                        Region = r.Region,
                        District = r.District,
                        Village = r.Village,
                        Zone = r.Zone ?? string.Empty,
                        DataCollectorDisplayName = r.DataCollectorDisplayName,
                        ErrorType = r.ErrorType.HasValue
                            ? GetReportErrorTypeString(strings, r.ErrorType.Value)
                            : string.Empty,
                    })
                    .ToList();

                return _excelExportService.ToCsv(reportData, columnLabels);
            }

            private byte[] GetCorrectReportsData(
                IReadOnlyList<ExportReportListResponseDto> reports,
                StringsResourcesVault strings,
                ReportListDataCollectorType reportListDataCollectorType)
            {
                var columnLabels = GetCorrectReportsColumnLabels(strings, reportListDataCollectorType);

                if (reportListDataCollectorType == ReportListDataCollectorType.CollectionPoint)
                {
                    var dcpRows = reports.Select(r => new ExportCorrectDcpReportListCsvContentDto
                    {
                        Id = r.Id,
                        Date = r.DateTime.ToString("yyyy-MM-dd"),
                        Time = r.DateTime.ToString("HH:mm"),
                        EpiYear = r.EpiYear,
                        EpiWeek = r.EpiWeek,
                        Status = r.Status ?? string.Empty,
                        Region = r.Region,
                        District = r.District,
                        Village = r.Village,
                        Zone = r.Zone ?? string.Empty,
                        HealthRiskName = r.HealthRiskName,
                        CountMalesBelowFive = r.CountMalesBelowFive,
                        CountMalesAtLeastFive = r.CountMalesAtLeastFive,
                        CountFemalesBelowFive = r.CountFemalesBelowFive,
                        CountFemalesAtLeastFive = r.CountFemalesAtLeastFive,
                        ReferredCount = r.ReferredCount,
                        DeathCount = r.DeathCount,
                        FromOtherVillagesCount = r.FromOtherVillagesCount,
                        DataCollectorDisplayName = r.DataCollectorDisplayName
                    }).ToList();

                    return _excelExportService.ToCsv(dcpRows, columnLabels);
                }

                var rows = reports.Select(r => new ExportCorrectReportListCsvContentDto
                {
                    Id = r.Id,
                    Date = r.DateTime.ToString("yyyy-MM-dd"),
                    Time = r.DateTime.ToString("HH:mm"),
                    EpiYear = r.EpiYear,
                    EpiWeek = r.EpiWeek,
                    Status = r.Status ?? string.Empty,
                    Region = r.Region,
                    District = r.District,
                    Village = r.Village,
                    Zone = r.Zone ?? string.Empty,
                    HealthRiskName = r.HealthRiskName,
                    CountMalesBelowFive = r.CountMalesBelowFive,
                    CountMalesAtLeastFive = r.CountMalesAtLeastFive,
                    CountFemalesBelowFive = r.CountFemalesBelowFive,
                    CountFemalesAtLeastFive = r.CountFemalesAtLeastFive,
                    DataCollectorDisplayName = r.DataCollectorDisplayName
                }).ToList();

                return _excelExportService.ToCsv(rows, columnLabels);
            }

            public static string? GetReportErrorTypeString(StringsResourcesVault strings, ReportErrorType? errorType) =>
errorType switch
{
    ReportErrorType.HealthRiskNotFound => strings[ResultKey.Report.ErrorType.HealthRiskNotFound],
    ReportErrorType.GlobalHealthRiskCodeNotFound => strings[ResultKey.Report.ErrorType.GlobalHealthRiskCodeNotFound],
    ReportErrorType.FormatError => strings[ResultKey.Report.ErrorType.FormatError],
    ReportErrorType.EventReportHumanHealthRisk => strings[ResultKey.Report.ErrorType.EventReportHumanHealthRisk],
    ReportErrorType.AggregateReportNonHumanHealthRisk => strings[ResultKey.Report.ErrorType.AggregateReportNonHumanHealthRisk],
    ReportErrorType.CollectionPointNonHumanHealthRisk => strings[ResultKey.Report.ErrorType.CollectionPointNonHumanHealthRisk],
    ReportErrorType.CollectionPointUsedDataCollectorFormat => strings[ResultKey.Report.ErrorType.CollectionPointUsedDataCollectorFormat],
    ReportErrorType.DataCollectorUsedCollectionPointFormat => strings[ResultKey.Report.ErrorType.DataCollectorUsedCollectionPointFormat],
    ReportErrorType.SingleReportNonHumanHealthRisk => strings[ResultKey.Report.ErrorType.SingleReportNonHumanHealthRisk],
    ReportErrorType.GenderAndAgeNonHumanHealthRisk => strings[ResultKey.Report.ErrorType.GenderAndAgeNonHumanHealthRisk],
    ReportErrorType.TooLong => strings[ResultKey.Report.ErrorType.TooLong],
    ReportErrorType.Gateway => strings[ResultKey.Report.ErrorType.Gateway],
    ReportErrorType.Timestamp => strings[ResultKey.Report.ErrorType.Timestamp],
    ReportErrorType.Other => strings[ResultKey.Report.ErrorType.Other],
    _ => null
};
        }
    }
}