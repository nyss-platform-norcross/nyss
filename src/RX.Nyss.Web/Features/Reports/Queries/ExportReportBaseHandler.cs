using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.Reports.Queries
{
    public abstract class ExportReportBaseHandler<TRequest> : IRequestHandler<TRequest, FileResultDto>
        where TRequest : IExportReportQuery
    {
        private readonly IReportExportService _reportExportService;

        private readonly IStringsService _stringsService;

        protected ExportReportBaseHandler(
            IReportExportService reportExportService,
            IStringsService stringsService)
        {
            _reportExportService = reportExportService;
            _stringsService = stringsService;
        }

        public abstract Task<FileResultDto> Handle(TRequest request, CancellationToken cancellationToken);

        protected async Task<IReadOnlyList<ExportReportListResponseDto>> FetchData(TRequest request) =>
            (await _reportExportService.FetchData(request.ProjectId, request.Filter)).Cast<ExportReportListResponseDto>().ToArray();

        protected async Task<StringsResourcesVault> FetchStrings() => await _stringsService.GetForCurrentUser();

        protected static IReadOnlyList<string> GetIncorrectReportsColumnLabels(StringsResourcesVault strings) =>
            new[]
            {
                strings["reports.export.id"],
                strings["reports.export.date"],
                strings["reports.export.time"],
                strings["reports.export.epiYear"],
                strings["reports.export.epiWeek"],
                strings["reports.export.message"],
                strings["reports.list.errorType"],
                strings["reports.list.region"],
                strings["reports.list.district"],
                strings["reports.list.village"],
                strings["reports.list.zone"],
                strings["reports.list.dataCollectorDisplayName"],
                strings["reports.list.dataCollectorPhoneNumber"],
                strings["reports.export.location"],
                strings["reports.export.corrected"],
            };

        protected static IReadOnlyList<string> GetCorrectReportsColumnLabels(
            StringsResourcesVault strings,
            ReportListDataCollectorType reportListDataCollectorType)
        {
            if (reportListDataCollectorType == ReportListDataCollectorType.CollectionPoint)
            {
                return new[]
                {
                    strings["reports.export.id"],
                    strings["reports.export.date"],
                    strings["reports.export.time"],
                    strings["reports.export.epiYear"],
                    strings["reports.export.epiWeek"],
                    strings["reports.list.status"],
                    strings["reports.list.region"],
                    strings["reports.list.district"],
                    strings["reports.list.village"],
                    strings["reports.list.zone"],
                    strings["reports.list.healthRisk"],
                    strings["reports.list.malesBelowFive"],
                    strings["reports.list.malesAtLeastFive"],
                    strings["reports.list.femalesBelowFive"],
                    strings["reports.list.femalesAtLeastFive"],
                    strings["reports.export.totalBelowFive"],
                    strings["reports.export.totalAtLeastFive"],
                    strings["reports.export.totalMale"],
                    strings["reports.export.totalFemale"],
                    strings["reports.export.total"],
                    strings["reports.export.referredCount"],
                    strings["reports.export.deathCount"],
                    strings["reports.export.fromOtherVillagesCount"],
                    strings["reports.list.dataCollectorDisplayName"],
                    strings["reports.list.dataCollectorPhoneNumber"],
                    strings["reports.export.message"],
                    strings["reports.export.location"],
                    strings["reports.export.corrected"],
                };
            }

            return new[]
            {
                strings["reports.export.id"],
                strings["reports.export.date"],
                strings["reports.export.time"],
                strings["reports.export.epiYear"],
                strings["reports.export.epiWeek"],
                strings["reports.list.status"],
                strings["reports.list.region"],
                strings["reports.list.district"],
                strings["reports.list.village"],
                strings["reports.list.zone"],
                strings["reports.list.healthRisk"],
                strings["reports.list.malesBelowFive"],
                strings["reports.list.malesAtLeastFive"],
                strings["reports.list.femalesBelowFive"],
                strings["reports.list.femalesAtLeastFive"],
                strings["reports.export.totalBelowFive"],
                strings["reports.export.totalAtLeastFive"],
                strings["reports.export.totalMale"],
                strings["reports.export.totalFemale"],
                strings["reports.export.total"],
                strings["reports.list.dataCollectorDisplayName"],
                strings["reports.list.dataCollectorPhoneNumber"],
                strings["reports.export.message"],
                strings["reports.export.reportAlertId"],
                strings["reports.export.location"],
                strings["reports.export.corrected"],
            };
        }
    }
}
