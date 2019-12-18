using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Internal;

namespace RX.Nyss.Web.Services
{
    public interface IExcelExportService
    {
        byte[] ToCsv<T>(IEnumerable<T> data, IEnumerable<string> columnLabels) where T : class;
    }

    public class ExcelExportService: IExcelExportService
    {
        public byte[] ToCsv<T>(IEnumerable<T> data, IEnumerable<string> columnLabels) where T:class
        {
            var columnData = data.Select(x =>
            {
                var type = typeof(T);
                var rowValues = type.GetProperties().Select(p => $"\"{p.GetValue(x)?.ToString()}\"").ToList();
                return rowValues;
            })
            .ToList();

            var builder = new StringBuilder();
            builder.AppendLine(string.Join(";", columnLabels));
            columnData.ForEach(row =>
            {
                builder.AppendLine(string.Join(";", row));
            });

            return Encoding.UTF8.GetBytes(builder.ToString());
        }
    }
}
