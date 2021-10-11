using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Features.Users;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Reports
{
    public class ExportReportCsvQuery : IRequest<FileResultDto>
    {
        public ExportReportCsvQuery(int projectId, ReportListFilterRequestDto filter)
        {
            ProjectId = projectId;
            Filter = filter;
        }

        public int ProjectId { get; }

        public ReportListFilterRequestDto Filter { get; }

        public class Handler : IRequestHandler<ExportReportCsvQuery, FileResultDto>
        {
            private readonly IReportExportService _reportExportService;

            private readonly IStringsResourcesService _stringsResourcesService;

            private readonly IAuthorizationService _authorizationService;

            private readonly IUserService _userService;

            private readonly IExcelExportService _excelExportService;

            public Handler(
                IReportExportService reportExportService,
                IStringsResourcesService stringsResourcesService,
                IAuthorizationService authorizationService,
                IUserService userService,
                IExcelExportService excelExportService)
            {
                _reportExportService = reportExportService;
                _stringsResourcesService = stringsResourcesService;
                _authorizationService = authorizationService;
                _userService = userService;
                _excelExportService = excelExportService;
            }

            public async Task<FileResultDto> Handle(ExportReportCsvQuery request, CancellationToken cancellationToken)
            {
                var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(_authorizationService.GetCurrentUserName());
                var strings = await _stringsResourcesService.GetStrings(userApplicationLanguageCode);

                var reports = await _reportExportService.FetchData(request.ProjectId, request.Filter);

                var data = request.Filter.FormatCorrect
                    ? GetCorrectReportsData(reports, strings, request.Filter.DataCollectorType)
                    : GetIncorrectReportsData(reports, strings);

                return new FileResultDto(data, MimeTypes.Csv);
            }

            private byte[] GetIncorrectReportsData(IReadOnlyCollection<IReportListResponseDto> reports, StringsResourcesVault strings)
            {
                var columnLabels = GetIncorrectReportsColumnLabels(strings);

                var reportData = reports.Select(r =>
                {
                    var report = (ExportReportListResponseDto)r;
                    return new ExportIncorrectReportListCsvContentDto
                    {
                        Id = r.Id,
                        Date = r.DateTime.ToString("yyyy-MM-dd"),
                        Time = report.DateTime.ToString("HH:mm"),
                        EpiWeek = report.EpiYear,
                        EpiYear = report.EpiWeek,
                        Region = report.Region,
                        District = report.District,
                        Village = report.Village,
                        Zone = report.Zone,
                        DataCollectorDisplayName = report.DataCollectorDisplayName,
                        PhoneNumber = report.PhoneNumber,
                        Message = report.Message,
                        Location = report.Location != null
                            ? $"{report.Location.Y}/{report.Location.X}"
                            : "",
                        ErrorType = report.ErrorType,
                        Corrected = report.Corrected
                            ? strings["report.export.corrected"]
                            : null
                    };
                });
                return _excelExportService.ToCsv(reportData, columnLabels);
            }

            private static IEnumerable<string> GetIncorrectReportsColumnLabels(StringsResourcesVault strings) =>
                new []
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

            private static IEnumerable<string> GetCorrectReportsColumnLabels(StringsResourcesVault strings, ReportListDataCollectorType reportListDataCollectorType)
            {
                if (reportListDataCollectorType == ReportListDataCollectorType.CollectionPoint)
                {
                    return new []
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

                return new []
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

            private byte[] GetCorrectReportsData(
                IReadOnlyCollection<IReportListResponseDto> reports,
                StringsResourcesVault strings,
                ReportListDataCollectorType reportListDataCollectorType)
            {
                var columnLabels = GetCorrectReportsColumnLabels(strings, reportListDataCollectorType);

                if (reportListDataCollectorType == ReportListDataCollectorType.CollectionPoint)
                {
                    var dcpReportData = reports.Select(r =>
                    {
                        var report = (ExportReportListResponseDto)r;
                        return new ExportCorrectDcpReportListCsvContentDto
                        {
                            Id = r.Id,
                            Date = r.DateTime.ToString("yyyy-MM-dd"),
                            Time = report.DateTime.ToString("HH:mm"),
                            EpiWeek = report.EpiYear,
                            EpiYear = report.EpiWeek,
                            Status = report.Status,
                            Region = report.Region,
                            District = report.District,
                            Village = report.Village,
                            Zone = report.Zone,
                            HealthRiskName = report.HealthRiskName,
                            CountMalesBelowFive = report.CountMalesBelowFive,
                            CountMalesAtLeastFive = report.CountMalesAtLeastFive,
                            CountFemalesBelowFive = report.CountFemalesBelowFive,
                            CountFemalesAtLeastFive = report.CountFemalesAtLeastFive,
                            TotalBelowFive = report.CountFemalesBelowFive + report.CountMalesBelowFive,
                            TotalAtLeastFive = report.CountMalesAtLeastFive + report.CountFemalesAtLeastFive,
                            TotalMale = report.CountMalesAtLeastFive + report.CountMalesBelowFive,
                            TotalFemale = report.CountFemalesAtLeastFive + report.CountFemalesBelowFive,
                            Total = report.CountMalesBelowFive + report.CountMalesAtLeastFive + report.CountFemalesBelowFive + report.CountFemalesAtLeastFive,
                            ReferredCount = report.ReferredCount,
                            DeathCount = report.DeathCount,
                            FromOtherVillagesCount = report.FromOtherVillagesCount,
                            DataCollectorDisplayName = report.DataCollectorDisplayName,
                            PhoneNumber = report.PhoneNumber,
                            Message = report.Message,
                            Location = report.Location != null
                                ? $"{report.Location.Y}/{report.Location.X}"
                                : "",
                            Corrected = report.Corrected ? strings["report.export.corrected"] : null
                        };
                    });
                    return _excelExportService.ToCsv(dcpReportData, columnLabels);
                }

                var reportData = reports.Select(r =>
                {
                    var report = (ExportReportListResponseDto)r;
                    return new ExportCorrectReportListCsvContentDto
                    {
                        Id = r.Id,
                        Date = r.DateTime.ToString("yyyy-MM-dd"),
                        Time = report.DateTime.ToString("HH:mm"),
                        EpiWeek = report.EpiYear,
                        EpiYear = report.EpiWeek,
                        Status = report.Status,
                        Region = report.Region,
                        District = report.District,
                        Village = report.Village,
                        Zone = report.Zone,
                        HealthRiskName = report.HealthRiskName,
                        CountMalesBelowFive = report.CountMalesBelowFive,
                        CountMalesAtLeastFive = report.CountMalesAtLeastFive,
                        CountFemalesBelowFive = report.CountFemalesBelowFive,
                        CountFemalesAtLeastFive = report.CountFemalesAtLeastFive,
                        TotalBelowFive = report.CountFemalesBelowFive + report.CountMalesBelowFive,
                        TotalAtLeastFive = report.CountMalesAtLeastFive + report.CountFemalesAtLeastFive,
                        TotalMale = report.CountMalesAtLeastFive + report.CountMalesBelowFive,
                        TotalFemale = report.CountFemalesAtLeastFive + report.CountFemalesBelowFive,
                        Total = report.CountMalesBelowFive + report.CountMalesAtLeastFive + report.CountFemalesBelowFive + report.CountFemalesAtLeastFive,
                        DataCollectorDisplayName = report.DataCollectorDisplayName,
                        PhoneNumber = report.PhoneNumber,
                        Message = report.Message,
                        ReportAlertId = report.ReportAlertId,
                        Location = report.Location != null
                            ? $"{report.Location.Y}/{report.Location.X}"
                            : "",
                        Corrected = report.Corrected ? strings["reports.export.corrected"] : null
                    };
                });
                return _excelExportService.ToCsv(reportData, columnLabels);
            }
        }
    }
}
