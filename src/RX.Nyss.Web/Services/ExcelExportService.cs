using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Alerts.Dto;

namespace RX.Nyss.Web.Services
{
    public interface IExcelExportService
    {
        byte[] ToCsv<T>(IEnumerable<T> data, IEnumerable<string> columnLabels) where T : class;

        ExcelPackage ToExcel(
            IReadOnlyList<AlertListExportResponseDto> exportAlertListResponseDtos,
            IReadOnlyList<string> columnLabels,
            string title);
    }

    public class ExcelExportService : IExcelExportService
    {
        private const string Quote = "\"";

        private static readonly char[] CharsToEscape = { ',', ';' };

        private readonly INyssWebConfig _config;

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

        public ExcelPackage ToExcel(
            IReadOnlyList<AlertListExportResponseDto> exportAlertListResponseDtos,
            IReadOnlyList<string> columnLabels,
            string title)
        {
            var package = new ExcelPackage();
            package.Workbook.Properties.Title = title;

            var worksheet = package.Workbook.Worksheets.Add(title);

            for (var index = 0; index < columnLabels.Count; index++)
            {
                worksheet.Cells[1, 1 + index].Value = columnLabels[index];
                worksheet.Cells[1, 1 + index].Style.Font.Bold = true;
            }

            for (var index = 0; index < exportAlertListResponseDtos.Count; index++)
            {
                var alert = exportAlertListResponseDtos[index];

                var triggeredAt = alert.TriggeredAt.ToOADate();
                var lastReport = alert.LastReportTimestamp.ToOADate();
                var escalatedAt = alert.EscalatedAt?.ToOADate();
                var closedAt = alert.ClosedAt?.ToOADate();
                var dismissedAt = alert.DismissedAt?.ToOADate();
                var columnIndex = index + 2;
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

        private static string EscapeCharacters(object data)
        {
            var value = data?.ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.IndexOfAny(CharsToEscape) > -1
                ? $"{Quote}{value}{Quote}"
                : value;
        }
    }
}
