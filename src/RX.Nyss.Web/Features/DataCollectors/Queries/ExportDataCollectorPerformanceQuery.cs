using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.DataCollectors.DataContracts;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.DataCollectors.Queries
{
    public class ExportDataCollectorPerformanceQuery : IRequest<FileResultDto>
    {
        public ExportDataCollectorPerformanceQuery(int projectId, DataCollectorPerformanceFiltersRequestDto filter)
        {
            ProjectId = projectId;
            Filter = filter;
        }

        public int ProjectId { get; }

        public DataCollectorPerformanceFiltersRequestDto Filter { get; }

        public class Handler : IRequestHandler<ExportDataCollectorPerformanceQuery, FileResultDto>
        {
            private readonly IDataCollectorService _dataCollectorService;

            private readonly IDateTimeProvider _dateTimeProvider;

            private readonly IStringsService _stringsService;

            private readonly NyssContext _nyssContext;

            public Handler(
                IDataCollectorService dataCollectorService,
                IDateTimeProvider dateTimeProvider,
                IStringsService stringsService,
                NyssContext nyssContext)
            {
                _dataCollectorService = dataCollectorService;
                _dateTimeProvider = dateTimeProvider;
                _stringsService = stringsService;
                _nyssContext = nyssContext;
            }

            public async Task<FileResultDto> Handle(ExportDataCollectorPerformanceQuery request, CancellationToken cancellationToken)
            {
                var filters = request.Filter;
                var projectId = request.ProjectId;

                var strings = await _stringsService.GetForCurrentUser();

                var dataCollectors = (await _dataCollectorService.GetDataCollectorsForCurrentUserInProject(projectId))
                    .FilterOnlyNotDeleted()
                    .FilterByArea(filters.Area)
                    .FilterByName(filters.Name)
                    .FilterBySupervisor(filters.SupervisorId)
                    .FilterByTrainingMode(filters.TrainingStatus)
                    .Include(dc => dc.DataCollectorLocations);

                var projectStartDate = await _nyssContext.Projects
                    .Where(p => p.Id == projectId)
                    .Select(p => p.StartDate)
                    .FirstOrDefaultAsync(cancellationToken);

                var currentDate = _dateTimeProvider.UtcNow;
                var epiDateRange = _dateTimeProvider.GetEpiDateRange(projectStartDate, currentDate).ToList();

                var dataCollectorsWithReportsData = await GetDataCollectorsWithReportData(dataCollectors);

                var dataCollectorPerformance = GetDataCollectorPerformance(dataCollectorsWithReportsData, currentDate, epiDateRange);

                var excelSheet = GetExcelData(strings, dataCollectorPerformance, epiDateRange);

                return new FileResultDto(
                    excelSheet.GetAsByteArray(),
                    MimeTypes.Excel);
            }

            private async Task<List<DataCollectorWithRawReportDataForExport>> GetDataCollectorsWithReportData(
                IIncludableQueryable<DataCollector, IEnumerable<DataCollectorLocation>> dataCollectors) =>
                await dataCollectors
                    .Select(dc => new DataCollectorWithRawReportDataForExport
                    {
                        Name = dc.Name,
                        PhoneNumber = dc.PhoneNumber,
                        VillageName = dc.DataCollectorLocations.First().Village.Name,
                        CreatedAt = dc.CreatedAt,
                        ReportsInTimeRange = dc.RawReports.Where(r => r.IsTraining.HasValue && !r.IsTraining.Value)
                            .Select(r => new RawReportDataForExport
                            {
                                IsValid = r.ReportId.HasValue,
                                ReceivedAt = r.ReceivedAt,
                                EpiDate = _dateTimeProvider.GetEpiDate(r.ReceivedAt)
                            })
                    }).ToListAsync();

            private List<DataCollectorPerformance> GetDataCollectorPerformance(IEnumerable<DataCollectorWithRawReportDataForExport> dataCollectorsWithReportsData, DateTime currentDate, IEnumerable<EpiDate> epiWeekRange) =>
                dataCollectorsWithReportsData
                    .Select(dc =>
                    {
                        var reportsGroupedByWeek = dc.ReportsInTimeRange
                            .GroupBy(report => report.EpiDate)
                            .ToList();

                        return new DataCollectorPerformance
                        {
                            Name = dc.Name,
                            PhoneNumber = dc.PhoneNumber,
                            VillageName = dc.VillageName,
                            DaysSinceLastReport = reportsGroupedByWeek.Any()
                                ? (int)(currentDate - reportsGroupedByWeek
                                    .SelectMany(g => g)
                                    .OrderByDescending(r => r.ReceivedAt)
                                    .First().ReceivedAt).TotalDays
                                : (int?)null,
                            PerformanceInEpiWeeks = epiWeekRange
                                .Select(epiDate => new PerformanceInEpiWeek
                                {
                                    EpiWeek = epiDate.EpiWeek,
                                    EpiYear = epiDate.EpiYear,
                                    ReportingStatus = DataCollectorExistedInWeek(epiDate, dc.CreatedAt)
                                        ? GetDataCollectorStatus(reportsGroupedByWeek.FirstOrDefault(r => r.Key.EpiWeek == epiDate.EpiWeek && r.Key.EpiYear == epiDate.EpiYear))
                                        : (ReportingStatus?)null
                                }).Reverse().ToList()
                        };
                    }).ToList();

            private ExcelPackage GetExcelData(StringsResourcesVault strings, List<DataCollectorPerformance> dataCollectorPerformances, List<EpiDate> epiDateRange)
            {
                var columnLabels = GetColumnLabels(strings, epiDateRange);
                var title = strings["dataCollectors.performanceExport.title"];

                var package = new ExcelPackage();
                package.Workbook.Properties.Title = title;
                var worksheet = package.Workbook.Worksheets.Add(title);

                foreach (var label in columnLabels)
                {
                    var index = columnLabels.IndexOf(label) + 1;
                    worksheet.Cells[1, index].Value = label;
                    worksheet.Cells[1, index].Style.Font.Bold = true;
                }

                foreach (var dataCollectorPerformance in dataCollectorPerformances)
                {
                    var index = dataCollectorPerformances.IndexOf(dataCollectorPerformance) + 2;
                    worksheet.Cells[index, 1].Value = dataCollectorPerformance.Name;
                    worksheet.Cells[index, 2].Value = dataCollectorPerformance.VillageName;
                    worksheet.Cells[index, 3].Value = dataCollectorPerformance.DaysSinceLastReport;

                    foreach (var performanceInWeek in dataCollectorPerformance.PerformanceInEpiWeeks)
                    {
                        var weekIndex = epiDateRange.IndexOf(new EpiDate(performanceInWeek.EpiWeek, performanceInWeek.EpiYear)) + 4;
                        worksheet.Cells[index, weekIndex].Value = (int?)performanceInWeek.ReportingStatus;

                        if (performanceInWeek.ReportingStatus.HasValue)
                        {
                            worksheet.Cells[index, weekIndex].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[index, weekIndex].Style.Fill.BackgroundColor.SetColor(performanceInWeek.ReportingStatus switch
                            {
                                // Colors are corresponding to those from front-end
                                ReportingStatus.ReportingCorrectly => Color.FromArgb(1, 21, 171, 21),
                                ReportingStatus.NotReporting => Color.FromArgb(1, 241, 183, 19),
                                ReportingStatus.ReportingWithErrors => Color.FromArgb(1, 210, 50, 50),
                                _ => Color.White,
                            });
                        }
                    }
                }

                var dimension = worksheet.Dimension;

                worksheet.Column(1).Width = 20;
                worksheet.Column(2).Width = 20;
                worksheet.Column(3).Width = 20;

                for (var i = 4; i <= dimension.Columns; i++)
                {
                    worksheet.Column(i).Width = 10;
                }

                return package;
            }

            private ReportingStatus GetDataCollectorStatus(IGrouping<EpiDate, RawReportDataForExport> grouping) =>
                grouping != null && grouping.Any()
                    ? grouping.All(x => x.IsValid) ? ReportingStatus.ReportingCorrectly : ReportingStatus.ReportingWithErrors
                    : ReportingStatus.NotReporting;

            private List<string> GetColumnLabels(StringsResourcesVault strings, IEnumerable<EpiDate> epiDateRange)
            {
                var epiWeeks = epiDateRange.Select(epiDate => $"{epiDate.EpiYear}/{strings["dataCollectors.performanceExport.epiWeekIdentifier"]} {epiDate.EpiWeek}");
                var columnbLabels = new List<string>
                {
                    strings["dataCollectors.performanceList.name"],
                    strings["dataCollectors.performanceList.villageName"],
                    strings["dataCollectors.performanceList.daysSinceLastReport"],
                };

                columnbLabels.AddRange(epiWeeks);

                return columnbLabels;
            }

            private bool DataCollectorExistedInWeek(EpiDate date, DateTime dataCollectorCreated)
            {
                var epiDataDataCollectorCreated = _dateTimeProvider.GetEpiDate(dataCollectorCreated);
                return epiDataDataCollectorCreated.EpiYear <= date.EpiYear && epiDataDataCollectorCreated.EpiWeek <= date.EpiWeek;
            }
        }
    }
}
