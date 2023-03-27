using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.Reports.Queries
{
    public class ExportReportCsvQuery : IExportReportQuery
    {
        public ExportReportCsvQuery(int projectId, ReportListFilterRequestDto filter)
        {
            ProjectId = projectId;
            Filter = filter;
        }

        public int ProjectId { get; }

        public ReportListFilterRequestDto Filter { get; }

        public class Handler : ExportReportBaseHandler<ExportReportCsvQuery>
        {
            private readonly IExcelExportService _excelExportService;

            public Handler(IReportExportService reportExportService, IStringsService stringsService, IExcelExportService excelExportService)
                : base(reportExportService, stringsService)
            {
                _excelExportService = excelExportService;
            }

            public override async Task<FileResultDto> Handle(ExportReportCsvQuery request, CancellationToken cancellationToken)
            {
                var strings = await FetchStrings();
                var reports = await FetchData(request);

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
                        
                        ErrorType = report.ErrorType

                    };
                });
                return _excelExportService.ToCsv(reportData, columnLabels);
            }

            private byte[] GetCorrectReportsData(
                IReadOnlyCollection<IReportListResponseDto> reports,
                StringsResourcesVault strings,
                ReportListDataCollectorType reportListDataCollectorType)
            {
                var columnLabels = GetCorrectReportsColumnLabels(strings, reportListDataCollectorType);

                if (reportListDataCollectorType == ReportListDataCollectorType.CollectionPoint)
                {
                    var dcpReportData = reports.Cast<ExportReportListResponseDto>().Select(report => new ExportCorrectDcpReportListCsvContentDto
                    {
                        Id = report.Id,
                        Date = report.DateTime.ToString("yyyy-MM-dd"),
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
                       
                        ReferredCount = report.ReferredCount,
                        DeathCount = report.DeathCount,
                        FromOtherVillagesCount = report.FromOtherVillagesCount,
                        DataCollectorDisplayName = report.DataCollectorDisplayName
                        
                    });

                    return _excelExportService.ToCsv(dcpReportData, columnLabels);
                }

                var reportData = reports.Cast<ExportReportListResponseDto>().Select(report => new ExportCorrectReportListCsvContentDto
                {
                    Id = report.Id,
                    Date = report.DateTime.ToString("yyyy-MM-dd"),
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
                    DataCollectorDisplayName = report.DataCollectorDisplayName
                   
                });

                return _excelExportService.ToCsv(reportData, columnLabels);
            }
        }
    }
}
