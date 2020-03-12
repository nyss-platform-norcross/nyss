using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Reports.Dto;

namespace RX.Nyss.Web.Services
{
    public interface IExcelExportService
    {
        byte[] ToCsv<T>(IEnumerable<T> data, IEnumerable<string> columnLabels) where T : class;
        ExcelPackage ToExcel(List<IReportListResponseDto> exportReportListResponseDtos, List<string> columnLabels, string title);
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

        public ExcelPackage ToExcel(List<IReportListResponseDto> columnData, List<string> columnLabels, string title)
        {
            var package = new ExcelPackage();
            package.Workbook.Properties.Title = title;

            var worksheet = package.Workbook.Worksheets.Add(title);

            foreach (var label in columnLabels)
            {
                worksheet.Cells[1, 1 + columnLabels.IndexOf(label)].Value = label;
                worksheet.Cells[1, 1 + columnLabels.IndexOf(label)].Style.Font.Bold = true;
            }

            foreach (ExportReportListResponseDto data in columnData)
            {
                var columnIndex = columnData.IndexOf(data) + 2;
                worksheet.Cells[columnIndex, 1].Value = data.DateTime;
                worksheet.Cells[columnIndex, 1].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[columnIndex, 2].Value = data.DateTime;
                worksheet.Cells[columnIndex, 2].Style.Numberformat.Format = "HH:mm";
                worksheet.Cells[columnIndex, 3].Value = data.Status;
                worksheet.Cells[columnIndex, 4].Value = data.DataCollectorDisplayName;
                worksheet.Cells[columnIndex, 5].Value = data.PhoneNumber;
                worksheet.Cells[columnIndex, 6].Value = data.Region;
                worksheet.Cells[columnIndex, 7].Value = data.District;
                worksheet.Cells[columnIndex, 8].Value = data.Village;
                worksheet.Cells[columnIndex, 9].Value = data.Zone;
                worksheet.Cells[columnIndex, 10].Value = data.HealthRiskName;
                worksheet.Cells[columnIndex, 11].Value = data.CountMalesBelowFive;
                worksheet.Cells[columnIndex, 12].Value = data.CountMalesAtLeastFive;
                worksheet.Cells[columnIndex, 13].Value = data.CountFemalesBelowFive;
                worksheet.Cells[columnIndex, 14].Value = data.CountFemalesAtLeastFive;
                worksheet.Cells[columnIndex, 15].Value = data.CountMalesBelowFive + data.CountFemalesBelowFive;
                worksheet.Cells[columnIndex, 16].Value = data.CountMalesAtLeastFive + data.CountFemalesAtLeastFive;
                worksheet.Cells[columnIndex, 17].Value = data.CountMalesBelowFive + data.CountMalesAtLeastFive;
                worksheet.Cells[columnIndex, 18].Value = data.CountFemalesBelowFive + data.CountFemalesAtLeastFive;
                worksheet.Cells[columnIndex, 19].Value = data.CountMalesBelowFive + data.CountMalesAtLeastFive + data.CountFemalesBelowFive + data.CountFemalesAtLeastFive;
                worksheet.Cells[columnIndex, 20].Value = data.Location != null ? $"{data.Location.Y}/{data.Location.X}" : "";
                worksheet.Cells[columnIndex, 21].Value = data.Message;
                worksheet.Cells[columnIndex, 22].Value = data.EpiYear;
                worksheet.Cells[columnIndex, 23].Value = data.EpiWeek;
            }

            worksheet.Column(1).Width = 20;
            worksheet.Column(5).Width = 20;
            worksheet.Column(20).Width = 30;

            return package;
        }

        public string EscapeCharacters(object data)
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
