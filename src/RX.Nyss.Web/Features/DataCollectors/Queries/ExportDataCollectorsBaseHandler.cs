using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.DataCollectors.Queries
{
    public abstract class ExportDataCollectorsBaseHandler<TRequest> : IRequestHandler<TRequest, FileResultDto>
        where TRequest : IExportDataCollectorsQuery
    {
        private readonly IDataCollectorExportService _dataCollectorExportService;
        private readonly IStringsService _stringsService;

        public ExportDataCollectorsBaseHandler(
            IDataCollectorExportService dataCollectorExportService,
            IStringsService stringsService)
        {
            _dataCollectorExportService = dataCollectorExportService;
            _stringsService = stringsService;
        }

        public abstract Task<FileResultDto> Handle(TRequest request, CancellationToken cancellationToken);

        protected async Task<StringsResourcesVault> FetchStrings() =>
            await _stringsService.GetForCurrentUser();

        protected List<string> GetColumnLabels(StringsResourcesVault strings) =>
         new List<string>
            {
                strings["dataCollectors.export.dataCollectorType"],
                strings["dataCollectors.export.name"],
                strings["dataCollectors.export.displayName"],
                strings["dataCollectors.export.phoneNumber"],
                strings["dataCollectors.export.additionalPhoneNumber"],
                strings["dataCollectors.export.sex"],
                strings["dataCollectors.export.birthYear"],
                strings["dataCollectors.export.region"],
                strings["dataCollectors.export.district"],
                strings["dataCollectors.export.village"],
                strings["dataCollectors.export.zone"],
                strings["dataCollectors.export.latitude"],
                strings["dataCollectors.export.longitude"],
                strings["dataCollectors.export.supervisor"],
                strings["dataCollectors.export.trainingStatus"],
                strings["dataCollectors.filters.deployedMode"],
            };

        protected string GetSheetTitle(StringsResourcesVault strings) =>
            strings["dataCollectors.export.title"];

        protected async Task<List<ExportDataCollectorsResponseDto>> GetDataCollectorExportData(TRequest request, StringsResourcesVault strings) =>
            await _dataCollectorExportService.GetDataCollectorsExportData(request.ProjectId, strings, request.Filters);
    }
}
