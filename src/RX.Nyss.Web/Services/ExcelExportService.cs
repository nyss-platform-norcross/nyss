using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Services
{
    public interface IExcelExportService
    {
        byte[] ToCsv<T>(IEnumerable<T> data, IEnumerable<string> columnLabels) where T : class;
        ExcelPackage CorrectReportsToExcel(List<IReportListResponseDto> exportReportListResponseDtos, List<string> columnLabels, string title, ReportListDataCollectorType reportListDataCollectorType);
        ExcelPackage IncorrectReportsToExcel(List<IReportListResponseDto> exportReportListResponseDtos, List<string> columnLabels, string title);
        ExcelPackage ToExcel(List<AlertListExportResponseDto> exportAlertListResponseDtos, List<string> columnLabels, string title);
    }

    public class ExcelExportService : IExcelExportService
    {
        private readonly INyssWebConfig _config;

        private readonly char[] _charsToEscape = { ',', ';' };
        private readonly string _quote = "\"";

        public ExcelExportService(INyssWebConfig config)
        {
            _config = config;
        }

        public byte[] ToCsv<T>(IEnumerable<T> data, IEnumerable<string> columnLabels) where T : class
        {
            var columnData = data.Select(x =>
                {
                    var type = typeof(T);
                    var rowValues = type.GetProperties()
                        .Select(p => EscapeCharacters(p.GetValue(x)))
                        .ToList();
                    return rowValues;
                })
                .ToList();

            var builder = new StringBuilder();
            builder.AppendLine(string.Join(_config.Export.CsvFieldSeparator, columnLabels));
            columnData.ForEach(row =>
                builder.AppendLine(string.Join(_config.Export.CsvFieldSeparator, row)));

            return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
        }

        public ExcelPackage CorrectReportsToExcel(List<IReportListResponseDto> columnData, List<string> columnLabels, string title, ReportListDataCollectorType reportListDataCollectorType)
        {
            var package = new ExcelPackage();
            package.Workbook.Properties.Title = title;

            var worksheet = package.Workbook.Worksheets.Add(title);

            foreach (var label in columnLabels)
            {
                worksheet.Cells[1, 1 + columnLabels.IndexOf(label)].Value = label;
                worksheet.Cells[1, 1 + columnLabels.IndexOf(label)].Style.Font.Bold = true;
            }

            foreach (var reportListResponseDto in columnData)
            {
                var data = (ExportReportListResponseDto)reportListResponseDto;
                var columnIndex = columnData.IndexOf(data) + 2;
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
                worksheet.Cells[columnIndex, 16].Value = data.CountMalesBelowFive + data.CountFemalesBelowFive;
                worksheet.Cells[columnIndex, 17].Value = data.CountMalesAtLeastFive + data.CountFemalesAtLeastFive;
                worksheet.Cells[columnIndex, 18].Value = data.CountMalesBelowFive + data.CountMalesAtLeastFive;
                worksheet.Cells[columnIndex, 19].Value = data.CountFemalesBelowFive + data.CountFemalesAtLeastFive;
                worksheet.Cells[columnIndex, 20].Value = data.CountMalesBelowFive + data.CountMalesAtLeastFive + data.CountFemalesBelowFive + data.CountFemalesAtLeastFive;

                if (reportListDataCollectorType == ReportListDataCollectorType.CollectionPoint)
                {
                    worksheet.Cells[columnIndex, 21].Value = data.ReferredCount;
                    worksheet.Cells[columnIndex, 22].Value = data.DeathCount;
                    worksheet.Cells[columnIndex, 23].Value = data.FromOtherVillagesCount;
                    worksheet.Cells[columnIndex, 24].Value = data.DataCollectorDisplayName;
                    worksheet.Cells[columnIndex, 25].Value = data.PhoneNumber;
                    worksheet.Cells[columnIndex, 26].Value = data.Message;
                    worksheet.Cells[columnIndex, 27].Value = data.Location != null ? $"{data.Location.Y}/{data.Location.X}" : "";
                }
                else
                {
                    worksheet.Cells[columnIndex, 21].Value = data.DataCollectorDisplayName;
                    worksheet.Cells[columnIndex, 22].Value = data.PhoneNumber;
                    worksheet.Cells[columnIndex, 23].Value = data.Message;
                    worksheet.Cells[columnIndex, 24].Value = data.ReportAlertId;
                    worksheet.Cells[columnIndex, 25].Value = data.Location != null ? $"{data.Location.Y}/{data.Location.X}" : "";
                }
            }
            worksheet.Column(2).Width = 12; //Date
            worksheet.Column(6).Width = 14; //ReportStatus
            worksheet.Column(11).Width = 20; //HealthRiskName

            if (reportListDataCollectorType == ReportListDataCollectorType.CollectionPoint)
            {
                worksheet.Column(24).Width = 20; //DcpName
                worksheet.Column(25).Width = 20; //PhoneNr
                worksheet.Column(26).Width = 20; //Message
                worksheet.Column(27).Width = 37; //Location
            }
            else
            {
                worksheet.Column(21).Width = 20; //DcName
                worksheet.Column(22).Width = 20; //PhoneNr
                worksheet.Column(23).Width = 12; //Message
                worksheet.Column(25).Width = 37; //Location
            }

            return package;
        }

        public ExcelPackage IncorrectReportsToExcel(List<IReportListResponseDto> columnData, List<string> columnLabels, string title)
        {
            var package = new ExcelPackage();
            package.Workbook.Properties.Title = title;

            var worksheet = package.Workbook.Worksheets.Add(title);

            foreach (var label in columnLabels)
            {
                worksheet.Cells[1, 1 + columnLabels.IndexOf(label)].Value = label;
                worksheet.Cells[1, 1 + columnLabels.IndexOf(label)].Style.Font.Bold = true;
            }

            foreach (var reportListResponseDto in columnData)
            {
                var data = (ExportReportListResponseDto)reportListResponseDto;
                var columnIndex = columnData.IndexOf(data) + 2;
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
                worksheet.Cells[columnIndex, 13].Value = data.PhoneNumber;
                worksheet.Cells[columnIndex, 14].Value = data.Location != null ? $"{data.Location.Y}/{data.Location.X}" : "";
            }

            worksheet.Column(2).Width = 12; //Date
            worksheet.Column(7).Width = 50; //ErrorType
            worksheet.Column(12).Width = 20; //DcName
            worksheet.Column(13).Width = 20; //PhoneNr
            worksheet.Column(14).Width = 37; //Location

            return package;
        }

        public ExcelPackage ToExcel(List<AlertListExportResponseDto> exportAlertListResponseDtos, List<string> columnLabels, string title)
        {
            var package = new ExcelPackage();
            package.Workbook.Properties.Title = title;

            var worksheet = package.Workbook.Worksheets.Add(title);

            foreach (var label in columnLabels)
            {
                worksheet.Cells[1, 1 + columnLabels.IndexOf(label)].Value = label;
                worksheet.Cells[1, 1 + columnLabels.IndexOf(label)].Style.Font.Bold = true;
            }

            foreach (var alert in exportAlertListResponseDtos)
            {
                var triggeredAt = alert.TriggeredAt.ToOADate();
                var lastReport = alert.LastReportTimestamp.ToOADate();
                var escalatedAt = alert.EscalatedAt?.ToOADate();
                var closedAt = alert.ClosedAt?.ToOADate();
                var dismissedAt = alert.DismissedAt?.ToOADate();
                var columnIndex = exportAlertListResponseDtos.IndexOf(alert) + 2;
                worksheet.Cells[columnIndex, 1].Value = alert.Id;
                worksheet.Cells[columnIndex, 2].Value = triggeredAt;
                worksheet.Cells[columnIndex, 2].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[columnIndex, 3].Value = triggeredAt;
                worksheet.Cells[columnIndex, 3].Style.Numberformat.Format = "HH:mm";
                worksheet.Cells[columnIndex, 4].Value = lastReport;
                worksheet.Cells[columnIndex, 4].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[columnIndex, 5].Value = lastReport;
                worksheet.Cells[columnIndex, 5].Style.Numberformat.Format = "HH:mm";
                worksheet.Cells[columnIndex, 6].Value = alert.HealthRisk;
                worksheet.Cells[columnIndex, 7].Value = alert.ReportCount;
                worksheet.Cells[columnIndex, 8].Value = alert.Status;
                worksheet.Cells[columnIndex, 9].Value = alert.LastReportRegion;
                worksheet.Cells[columnIndex, 10].Value = alert.LastReportDistrict;
                worksheet.Cells[columnIndex, 11].Value = alert.LastReportVillage;
                worksheet.Cells[columnIndex, 12].Value = alert.LastReportZone;
                worksheet.Cells[columnIndex, 13].Value = escalatedAt;
                worksheet.Cells[columnIndex, 13].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[columnIndex, 14].Value = escalatedAt;
                worksheet.Cells[columnIndex, 14].Style.Numberformat.Format = "HH:mm";
                worksheet.Cells[columnIndex, 15].Value = closedAt;
                worksheet.Cells[columnIndex, 15].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[columnIndex, 16].Value = closedAt;
                worksheet.Cells[columnIndex, 16].Style.Numberformat.Format = "HH:mm";
                worksheet.Cells[columnIndex, 17].Value = dismissedAt;
                worksheet.Cells[columnIndex, 17].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[columnIndex, 18].Value = dismissedAt;
                worksheet.Cells[columnIndex, 18].Style.Numberformat.Format = "HH:mm";
                worksheet.Cells[columnIndex, 19].Value = alert.Investigation;
                worksheet.Cells[columnIndex, 20].Value = alert.Outcome;
                worksheet.Cells[columnIndex, 21].Value = alert.EscalatedOutcome;
                worksheet.Cells[columnIndex, 22].Value = alert.Comments;
            }

            worksheet.Column(2).Width = 16;
            worksheet.Column(3).Width = 16;
            worksheet.Column(4).Width = 16;
            worksheet.Column(5).Width = 16;
            worksheet.Column(6).Width = 20;
            worksheet.Column(7).Width = 16;
            worksheet.Column(8).Width = 10;
            worksheet.Column(9).Width = 10;
            worksheet.Column(10).Width = 15;
            worksheet.Column(11).Width = 15;
            worksheet.Column(12).Width = 14;
            worksheet.Column(13).Width = 15;
            worksheet.Column(14).Width = 15;
            worksheet.Column(15).Width = 15;
            worksheet.Column(16).Width = 15;
            worksheet.Column(17).Width = 15;
            worksheet.Column(18).Width = 15;
            worksheet.Column(19).Width = 20;
            worksheet.Column(20).Width = 30;
            worksheet.Column(21).Width = 16;
            worksheet.Column(22).Width = 20;

            return package;
        }

        private string EscapeCharacters(object data)
        {
            if (data == null)
            {
                return null;
            }

            var value = data.ToString();

            if (value.IndexOfAny(_charsToEscape) > -1)
            {
                return $"{_quote}{value}{_quote}";
            }

            return value;
        }
    }
}
