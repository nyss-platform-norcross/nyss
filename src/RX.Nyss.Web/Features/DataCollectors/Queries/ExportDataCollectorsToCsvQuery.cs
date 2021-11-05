using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.DataCollectors.Queries
{
    public class ExportDataCollectorsToCsvQuery : IExportDataCollectorsQuery
    {
        public int ProjectId { get; }
        public DataCollectorsFiltersRequestDto Filters { get; }

        public ExportDataCollectorsToCsvQuery(int projectId, DataCollectorsFiltersRequestDto filters)
        {
            ProjectId = projectId;
            Filters = filters;
        }

        public class Handler : ExportDataCollectorsBaseHandler<ExportDataCollectorsToCsvQuery>
        {
            private readonly IExcelExportService _excelExportService;

            public Handler(
                IDataCollectorExportService dataCollectorExportService,
                IStringsService stringsService,
                IExcelExportService excelExportService)
            : base(dataCollectorExportService, stringsService)
            {
                _excelExportService = excelExportService;
            }

            public override async Task<FileResultDto> Handle(ExportDataCollectorsToCsvQuery query, CancellationToken cancellationToken)
            {
                var strings = await FetchStrings();

                var dataCollectors = await GetDataCollectorExportData(query, strings);

                var csvData = GetCsvData(dataCollectors, strings);

                return new FileResultDto(
                    csvData,
                    MimeTypes.Csv);
            }

            private byte[] GetCsvData(List<ExportDataCollectorsResponseDto> dataCollectors, StringsResourcesVault strings)
            {
                var columnLabels = GetColumnLabels(strings);

                var dataCollectorsData = dataCollectors
                    .Select(dc => new
                    {
                        DataCollectorType = dc.DataCollectorType,
                        dc.Name,
                        dc.DisplayName,
                        dc.PhoneNumber,
                        dc.AdditionalPhoneNumber,
                        dc.Sex,
                        BirthYear = dc.BirthDecade.HasValue
                            ? $"{dc.BirthDecade}-{dc.BirthDecade.ToString()[..3]}9"
                            : "",
                        dc.Region,
                        dc.District,
                        dc.Village,
                        dc.Zone,
                        dc.Latitude,
                        dc.Longitude,
                        dc.Supervisor,
                        dc.TrainingStatus,
                        dc.Deployed
                    });

                return _excelExportService.ToCsv(dataCollectorsData, columnLabels);
            }
        }
    }
}
