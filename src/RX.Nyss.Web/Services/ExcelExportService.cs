using System.Collections.Generic;
using System.Linq;
using System.Text;
using RX.Nyss.Web.Configuration;

namespace RX.Nyss.Web.Services
{
    public interface IExcelExportService
    {
        byte[] ToCsv<T>(IEnumerable<T> data, IEnumerable<string> columnLabels) where T : class;
    }

    public class ExcelExportService: IExcelExportService
    {
        private readonly INyssWebConfig _config;

        private readonly char[] CHARS_TO_ESCAPE = new char[] { ',', ';' };
        private readonly string QUOTE = "\"";

        public ExcelExportService(INyssWebConfig config)
        {
            _config = config;
        }

        public byte[] ToCsv<T>(IEnumerable<T> data, IEnumerable<string> columnLabels) where T:class
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

        public string EscapeCharacters(object data)
        {
            if (data == null)
            {
                return null;
            }

            var value = data.ToString();

            if (value.IndexOfAny(CHARS_TO_ESCAPE) > - 1)
            {
                return QUOTE + value + QUOTE;
            }
            return value;
        }
    }
}
