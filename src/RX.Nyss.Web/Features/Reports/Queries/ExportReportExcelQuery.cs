using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OfficeOpenXml;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Reports.Queries
{
    public class ExportReportExcelQuery : IExportReportQuery
    {

        public ExportReportExcelQuery(int projectId, ReportListFilterRequestDto filter)
        {
            ProjectId = projectId;
            Filter = filter;
        }

        public int ProjectId { get; }
        public ReportListFilterRequestDto Filter { get; }

        public class Handler : ExportReportBaseHandler<ExportReportExcelQuery>
        {
            static Handler() => ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            public Handler(IReportExportService reportExportService, IStringsService stringsService)
                : base(reportExportService, stringsService)
            {
            }

            public override async Task<FileResultDto> Handle(ExportReportExcelQuery request, CancellationToken cancellationToken)
            {
                var strings = await FetchStrings();
                var reports = await FetchData(request);

                var data = request.Filter.FormatCorrect
                    ? GetCorrectReportsData(reports, strings, request.Filter.DataCollectorType)
                    : GetIncorrectReportsData(reports, strings);

                return new FileResultDto(data, MimeTypes.Excel);
            }

            private static ExcelWorksheet CreateSheet(ExcelPackage package, string title, IReadOnlyList<string> columnLabels)
            {
                var worksheet = package.Workbook.Worksheets.Add(title);
                for (var i = 0; i < columnLabels.Count; i++)
                {
                    var cell = worksheet.Cells[1, i + 1];
                    cell.Value = columnLabels[i];
                    cell.Style.Font.Bold = true;
                }
                return worksheet;
            }

            private static byte[] GetIncorrectReportsData(
                IReadOnlyList<ExportReportListResponseDto> reports,
                StringsResourcesVault strings)
            {
                var columnLabels = GetIncorrectReportsColumnLabels(strings);
                var title = strings["reports.export.title"];

                using var package = new ExcelPackage();
                package.Workbook.Properties.Title = title;
                var worksheet = CreateSheet(package, title, columnLabels);

                for (var i = 0; i < reports.Count; i++)
                {
                    var r = reports[i];
                    var row = i + 2;
                    worksheet.SetValue(row, 1, r.Id);
                    worksheet.SetValue(row, 2, r.DateTime);
                    worksheet.SetValue(row, 3, r.DateTime);
                    worksheet.SetValue(row, 4, r.EpiYear);
                    worksheet.SetValue(row, 5, r.EpiWeek);
                    worksheet.SetValue(row, 6, r.Message);
                    worksheet.SetValue(row, 7, r.ErrorType.HasValue ? GetReportErrorTypeString(strings, r.ErrorType.Value) : string.Empty);
                    worksheet.SetValue(row, 8, r.Region);
                    worksheet.SetValue(row, 9, r.District);
                    worksheet.SetValue(row, 10, r.Village);
                    worksheet.SetValue(row, 11, r.Zone);
                    worksheet.SetValue(row, 12, r.DataCollectorDisplayName);
                }

                if (reports.Count > 0)
                {
                    worksheet.Cells[2, 2, reports.Count + 1, 2].Style.Numberformat.Format = "yyyy-MM-dd";
                    worksheet.Cells[2, 3, reports.Count + 1, 3].Style.Numberformat.Format = "HH:mm";
                }

                worksheet.Column(2).Width = 12;
                worksheet.Column(7).Width = 50;
                worksheet.Column(12).Width = 20;

                return package.GetAsByteArray();
            }

            private static byte[] GetCorrectReportsData(
                IReadOnlyList<ExportReportListResponseDto> reports,
                StringsResourcesVault strings,
                ReportListDataCollectorType reportListDataCollectorType)
            {
                var isCollectionPoint = reportListDataCollectorType == ReportListDataCollectorType.CollectionPoint;
                var columnLabels = GetCorrectReportsColumnLabels(strings, reportListDataCollectorType);
                var title = strings["reports.export.title"];

                using var package = new ExcelPackage();
                package.Workbook.Properties.Title = title;
                var worksheet = CreateSheet(package, title, columnLabels);

                for (var i = 0; i < reports.Count; i++)
                {
                    var r = reports[i];
                    var row = i + 2;
                    worksheet.SetValue(row, 1, r.Id);
                    worksheet.SetValue(row, 2, r.DateTime);
                    worksheet.SetValue(row, 3, r.DateTime);
                    worksheet.SetValue(row, 4, r.EpiYear);
                    worksheet.SetValue(row, 5, r.EpiWeek);
                    worksheet.SetValue(row, 6, r.Status);
                    worksheet.SetValue(row, 7, r.Region);
                    worksheet.SetValue(row, 8, r.District);
                    worksheet.SetValue(row, 9, r.Village);
                    worksheet.SetValue(row, 10, r.Zone);
                    worksheet.SetValue(row, 11, r.HealthRiskName);
                    worksheet.SetValue(row, 12, r.CountMalesBelowFive);
                    worksheet.SetValue(row, 13, r.CountMalesAtLeastFive);
                    worksheet.SetValue(row, 14, r.CountFemalesBelowFive);
                    worksheet.SetValue(row, 15, r.CountFemalesAtLeastFive);

                    if (isCollectionPoint)
                    {
                        worksheet.SetValue(row, 16, r.ReferredCount);
                        worksheet.SetValue(row, 17, r.DeathCount);
                        worksheet.SetValue(row, 18, r.FromOtherVillagesCount);
                        worksheet.SetValue(row, 19, r.DataCollectorDisplayName);
                    }
                    else
                    {
                        worksheet.SetValue(row, 16, r.DataCollectorDisplayName);
                    }
                }

                if (reports.Count > 0)
                {
                    worksheet.Cells[2, 2, reports.Count + 1, 2].Style.Numberformat.Format = "yyyy-MM-dd";
                    worksheet.Cells[2, 3, reports.Count + 1, 3].Style.Numberformat.Format = "HH:mm";
                }

                worksheet.Column(2).Width = 12;
                worksheet.Column(6).Width = 14;
                worksheet.Column(11).Width = 20;
                if (isCollectionPoint) worksheet.Column(19).Width = 20;
                else worksheet.Column(16).Width = 20;

                return package.GetAsByteArray();
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