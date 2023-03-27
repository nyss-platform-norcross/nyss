using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OfficeOpenXml;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Services;

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

            private byte[] GetIncorrectReportsData(
                IReadOnlyList<ExportReportListResponseDto> reports,
                StringsResourcesVault strings)
            {
                var columnLabels = GetIncorrectReportsColumnLabels(strings);
                var title = strings["reports.export.title"];

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var package = new ExcelPackage();
                package.Workbook.Properties.Title = title;

                var worksheet = package.Workbook.Worksheets.Add(title);

                for (var index = 0; index < columnLabels.Count; index++)
                {
                    worksheet.Cells[1, 1 + index].Value = columnLabels[index];
                    worksheet.Cells[1, 1 + index].Style.Font.Bold = true;
                }

                for (var index = 0; index < reports.Count; index++)
                {
                    var data = reports[index];
                    var columnIndex = index + 2;

                    worksheet.Cells[columnIndex, 1].Value = data.Id;
                    worksheet.Cells[columnIndex, 2].Value = data.DateTime;
                    worksheet.Cells[columnIndex, 2].Style.Numberformat.Format = "yyyy-MM-dd";
                    worksheet.Cells[columnIndex, 3].Value = data.DateTime;
                    worksheet.Cells[columnIndex, 3].Style.Numberformat.Format = "HH:mm";
                    worksheet.Cells[columnIndex, 4].Value = data.EpiYear;
                    worksheet.Cells[columnIndex, 5].Value = data.EpiWeek;
                    worksheet.Cells[columnIndex, 6].Value = data.Message;
                    worksheet.Cells[columnIndex, 7].Value = data.ErrorType;
                    worksheet.Cells[columnIndex, 8].Value = data.Region;
                    worksheet.Cells[columnIndex, 9].Value = data.District;
                    worksheet.Cells[columnIndex, 10].Value = data.Village;
                    worksheet.Cells[columnIndex, 11].Value = data.Zone;
                    worksheet.Cells[columnIndex, 12].Value = data.DataCollectorDisplayName;
                    
                }

                worksheet.Column(2).Width = 12; //Date
                worksheet.Column(7).Width = 50; //ErrorType
                worksheet.Column(12).Width = 20; //DcName

                return package.GetAsByteArray();
            }

            private byte[] GetCorrectReportsData(
                IReadOnlyList<ExportReportListResponseDto> reports,
                StringsResourcesVault strings,
                ReportListDataCollectorType reportListDataCollectorType)
            {
                var columnLabels = GetCorrectReportsColumnLabels(strings, reportListDataCollectorType);
                var title = strings["reports.export.title"];

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var package = new ExcelPackage();
                package.Workbook.Properties.Title = title;

                var worksheet = package.Workbook.Worksheets.Add(title);

                for (var index = 0; index < columnLabels.Count; index++)
                {
                    worksheet.Cells[1, 1 + index].Value = columnLabels[index];
                    worksheet.Cells[1, 1 + index].Style.Font.Bold = true;
                }

                for (var index = 0; index < reports.Count; index++)
                {
                    var data = reports[index];
                    var columnIndex = index + 2;

                    worksheet.Cells[columnIndex, 1].Value = data.Id;
                    worksheet.Cells[columnIndex, 2].Value = data.DateTime;
                    worksheet.Cells[columnIndex, 2].Style.Numberformat.Format = "yyyy-MM-dd";
                    worksheet.Cells[columnIndex, 3].Value = data.DateTime;
                    worksheet.Cells[columnIndex, 3].Style.Numberformat.Format = "HH:mm";
                    worksheet.Cells[columnIndex, 4].Value = data.EpiYear;
                    worksheet.Cells[columnIndex, 5].Value = data.EpiWeek;
                    worksheet.Cells[columnIndex, 6].Value = data.Status;
                    worksheet.Cells[columnIndex, 7].Value = data.Region;
                    worksheet.Cells[columnIndex, 8].Value = data.District;
                    worksheet.Cells[columnIndex, 9].Value = data.Village;
                    worksheet.Cells[columnIndex, 10].Value = data.Zone;
                    worksheet.Cells[columnIndex, 11].Value = data.HealthRiskName;
                    worksheet.Cells[columnIndex, 12].Value = data.CountMalesBelowFive;
                    worksheet.Cells[columnIndex, 13].Value = data.CountMalesAtLeastFive;
                    worksheet.Cells[columnIndex, 14].Value = data.CountFemalesBelowFive;
                    worksheet.Cells[columnIndex, 15].Value = data.CountFemalesAtLeastFive;
                   

                    if (reportListDataCollectorType == ReportListDataCollectorType.CollectionPoint)
                    {
                        worksheet.Cells[columnIndex, 16].Value = data.ReferredCount;
                        worksheet.Cells[columnIndex, 17].Value = data.DeathCount;
                        worksheet.Cells[columnIndex, 18].Value = data.FromOtherVillagesCount;
                        worksheet.Cells[columnIndex, 19].Value = data.DataCollectorDisplayName;
                        
                    }
                    else
                    {
                        worksheet.Cells[columnIndex, 16].Value = data.DataCollectorDisplayName;
                        
                    }
                }

                worksheet.Column(2).Width = 12; //Date
                worksheet.Column(6).Width = 14; //ReportStatus
                worksheet.Column(11).Width = 20; //HealthRiskName

                if (reportListDataCollectorType == ReportListDataCollectorType.CollectionPoint)
                {
                    worksheet.Column(19).Width = 20; //DcpName
                    
                }
                else
                {
                    worksheet.Column(16).Width = 20; //DcName

                }

                return package.GetAsByteArray();
            }
        }
    }
}
