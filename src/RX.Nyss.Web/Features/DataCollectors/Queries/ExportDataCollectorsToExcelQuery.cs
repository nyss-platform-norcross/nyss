using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OfficeOpenXml;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.DataCollectors.Queries
{
    public class ExportDataCollectorsToExcelQuery : IExportDataCollectorsQuery
    {
        public int ProjectId { get; }
        public DataCollectorsFiltersRequestDto Filters { get; }

        public ExportDataCollectorsToExcelQuery(int projectId, DataCollectorsFiltersRequestDto filters)
        {
            ProjectId = projectId;
            Filters = filters;
        }

        public class Handler : ExportDataCollectorsBaseHandler<ExportDataCollectorsToExcelQuery>
        {
            public Handler(
                IDataCollectorExportService dataCollectorExportService,
                IStringsService stringsService)
            : base(dataCollectorExportService, stringsService)
            {
            }

            public override async Task<FileResultDto> Handle(ExportDataCollectorsToExcelQuery query, CancellationToken cancellationToken)
            {
                var strings = await FetchStrings();

                var dataCollectors = await GetDataCollectorExportData(query, strings);

                var excelSheet = GetExcelData(dataCollectors, strings);

                return new FileResultDto(
                    excelSheet.GetAsByteArray(),
                    MimeTypes.Excel);
            }

            private ExcelPackage GetExcelData(
                List<ExportDataCollectorsResponseDto> dataCollectors,
                StringsResourcesVault strings)
            {
                var title = GetSheetTitle(strings);
                var columnLabels = GetColumnLabels(strings);
                var package = new ExcelPackage();

                package.Workbook.Properties.Title = title;
                var worksheet = package.Workbook.Worksheets.Add(title);

                foreach (var label in columnLabels)
                {
                    var index = columnLabels.IndexOf(label) + 1;
                    worksheet.Cells[1, index].Value = label;
                    worksheet.Cells[1, index].Style.Font.Bold = true;
                }

                foreach (var dataCollector in dataCollectors)
                {
                    var index = dataCollectors.IndexOf(dataCollector) + 2;
                    worksheet.Cells[index, 1].Value = dataCollector.DataCollectorType;
                    worksheet.Cells[index, 2].Value = dataCollector.Name;
                    worksheet.Cells[index, 3].Value = dataCollector.DisplayName;
                    worksheet.Cells[index, 4].Value = dataCollector.PhoneNumber;
                    worksheet.Cells[index, 5].Value = dataCollector.AdditionalPhoneNumber;
                    worksheet.Cells[index, 6].Value = dataCollector.Sex;
                    worksheet.Cells[index, 7].Value = dataCollector.BirthDecade.HasValue
                        ? $"{dataCollector.BirthDecade}-{dataCollector.BirthDecade.ToString()[..3]}9"
                        : "";
                    worksheet.Cells[index, 8].Value = dataCollector.Region;
                    worksheet.Cells[index, 9].Value = dataCollector.District;
                    worksheet.Cells[index, 10].Value = dataCollector.Village;
                    worksheet.Cells[index, 11].Value = dataCollector.Zone;
                    worksheet.Cells[index, 12].Value = dataCollector.Latitude;
                    worksheet.Cells[index, 13].Value = dataCollector.Longitude;
                    worksheet.Cells[index, 14].Value = dataCollector.Supervisor;
                    worksheet.Cells[index, 15].Value = dataCollector.TrainingStatus;
                    worksheet.Cells[index, 16].Value = dataCollector.Deployed;
                }

                worksheet.Column(1).Width = 20;
                worksheet.Column(4).Width = 20;

                return package;
            }
        }
    }
}
