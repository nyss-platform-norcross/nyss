using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Features.Users;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Reports
{
    public abstract class ExportReportBaseHandler<TRequest> : IRequestHandler<TRequest, FileResultDto>
        where TRequest : IExportReportQuery
    {
        private readonly IReportExportService _reportExportService;

        private readonly IStringsResourcesService _stringsResourcesService;

        private readonly IAuthorizationService _authorizationService;

        private readonly IUserService _userService;

        protected ExportReportBaseHandler(
            IReportExportService reportExportService,
            IStringsResourcesService stringsResourcesService,
            IAuthorizationService authorizationService,
            IUserService userService)
        {
            _reportExportService = reportExportService;
            _stringsResourcesService = stringsResourcesService;
            _authorizationService = authorizationService;
            _userService = userService;
        }

        public abstract Task<FileResultDto> Handle(TRequest request, CancellationToken cancellationToken);

        protected async Task<IReadOnlyList<ExportReportListResponseDto>> FetchData(TRequest request) =>
            (await _reportExportService.FetchData(request.ProjectId, request.Filter)).Cast<ExportReportListResponseDto>().ToArray();

        protected async Task<StringsResourcesVault> FetchStrings()
        {
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(_authorizationService.GetCurrentUserName());

            return await _stringsResourcesService.GetStrings(userApplicationLanguageCode);
        }

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
