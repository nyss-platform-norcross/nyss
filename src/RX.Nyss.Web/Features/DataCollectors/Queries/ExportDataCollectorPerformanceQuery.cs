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
                    .Include(dc => dc.DataCollectorLocations)
                    .Include(dc => dc.DatesNotDeployed)
                    .FilterOnlyNotDeleted()
                    .FilterByArea(filters.Locations)
                    .FilterByName(filters.Name)
                    .FilterBySupervisor(filters.SupervisorId)
                    .FilterByTrainingMode(filters.TrainingStatus);

                var projectStartDate = await _nyssContext.Projects
                    .Where(p => p.Id == projectId)
                    .Select(p => p.StartDate)
                    .FirstOrDefaultAsync(cancellationToken);

                var currentDate = _dateTimeProvider.UtcNow;
                var epiDateRange = _dateTimeProvider.GetEpiDateRange(projectStartDate, currentDate).ToList();

                var dataCollectorsWithReportsData = await GetDataCollectorsWithReportData(dataCollectors);

                var dataCollectorPerformance = GetDataCollectorPerformance(dataCollectorsWithReportsData, currentDate, epiDateRange);

                var completenessPerWeek = GetDataCollectorCompleteness(request.Filter, dataCollectorsWithReportsData, epiDateRange);

                var excelSheet = GetExcelData(strings, dataCollectorPerformance, completenessPerWeek, epiDateRange);

                return new FileResultDto(
                    excelSheet.GetAsByteArray(),
                    MimeTypes.Excel);
            }

            private async Task<List<DataCollectorWithRawReportDataForExport>> GetDataCollectorsWithReportData(
                IQueryable<DataCollector> dataCollectors) =>
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
                            }),
                        DatesNotDeployed = dc.DatesNotDeployed
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
                                : null,
                            PerformanceInEpiWeeks = epiWeekRange
                                .Select(epiDate => new PerformanceInEpiWeek
                                {
                                    EpiWeek = epiDate.EpiWeek,
                                    EpiYear = epiDate.EpiYear,
                                    ReportingStatus = DataCollectorExistedInWeek(epiDate, dc.CreatedAt) && DataCollectorWasDeployedInWeek(epiDate, dc.DatesNotDeployed.ToList())
                                        ? GetDataCollectorStatus(reportsGroupedByWeek.FirstOrDefault(r => r.Key.EpiWeek == epiDate.EpiWeek && r.Key.EpiYear == epiDate.EpiYear))
                                        : null
                                }).Reverse().ToList()
                        };
                    }).ToList();

            private List<Completeness> GetDataCollectorCompleteness(DataCollectorPerformanceFiltersRequestDto filters, IList<DataCollectorWithRawReportDataForExport> dataCollectors, List<EpiDate> epiDateRange)
            {
                if (IsWeekFiltersActive(filters) || !dataCollectors.Any())
                {
                    return null;
                }

                var dataCollectorCompleteness = dataCollectors
                    .Select(dc => dc.ReportsInTimeRange
                        .GroupBy(report => report.EpiDate)
                        .Select(g => new
                        {
                            EpiDate = g.Key,
                            HasReported = g.Any()
                        })).SelectMany(x => x)
                    .GroupBy(x => x.EpiDate)
                    .Select(g => new
                    {
                        EpiDate = g.Key,
                        Active = g.Sum(x => x.HasReported
                            ? 1
                            : 0)
                    }).ToList();

                return epiDateRange.Select(epiDate => new Completeness
                {
                    EpiWeek = epiDate.EpiWeek,
                    TotalDataCollectors = TotalDataCollectorsDeployedInWeek(epiDate, dataCollectors),
                    ActiveDataCollectors = dataCollectorCompleteness
                        .FirstOrDefault(dc => dc.EpiDate.EpiWeek == epiDate.EpiWeek && dc.EpiDate.EpiYear == epiDate.EpiYear)?.Active ?? 0
                }).ToList();
            }

            private ExcelPackage GetExcelData(StringsResourcesVault strings, List<DataCollectorPerformance> dataCollectorPerformances, List<Completeness> completenessPerWeek, List<EpiDate> epiDateRange)
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

                foreach (var completeness in completenessPerWeek)
                {
                    var columnIndex = completenessPerWeek.IndexOf(completeness) + 4;
                    worksheet.Cells[2, 1].Value = strings["dataCollectors.performanceList.completenessTitle"];
                    worksheet.Cells[2, columnIndex].Value = completeness.ActiveDataCollectors / completeness.TotalDataCollectors;
                    worksheet.Cells[2, columnIndex].Style.Numberformat.Format = "#0.0%";
                }

                foreach (var dataCollectorPerformance in dataCollectorPerformances)
                {
                    var index = dataCollectorPerformances.IndexOf(dataCollectorPerformance) + 3;
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

                worksheet.Row(2).Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Row(2).Style.Fill.BackgroundColor.SetColor(Color.DarkGray);
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

            private int TotalDataCollectorsDeployedInWeek(EpiDate epiDate, IEnumerable<DataCollectorWithRawReportDataForExport> dataCollectors) =>
                dataCollectors.Count(dc => DataCollectorExistedInWeek(epiDate, dc.CreatedAt)
                    && DataCollectorWasDeployedInWeek(epiDate, dc.DatesNotDeployed.ToList()));

            private bool DataCollectorExistedInWeek(EpiDate date, DateTime dataCollectorCreated)
            {
                var epiDataDataCollectorCreated = _dateTimeProvider.GetEpiDate(dataCollectorCreated);
                return epiDataDataCollectorCreated.EpiYear <= date.EpiYear && epiDataDataCollectorCreated.EpiWeek <= date.EpiWeek;
            }

            private bool DataCollectorWasDeployedInWeek(EpiDate date, List<DataCollectorNotDeployed> datesNotDeployed)
            {
                if (!datesNotDeployed.Any())
                {
                    return true;
                }

                var firstDayOfEpiWeek = _dateTimeProvider.GetFirstDateOfEpiWeek(date.EpiYear, date.EpiWeek);
                var lastDayOfEpiWeek = firstDayOfEpiWeek.AddDays(7);

                var dataCollectorNotDeployedInWeek = datesNotDeployed.Any(d => d.StartDate < firstDayOfEpiWeek
                    && (!d.EndDate.HasValue || d.EndDate > lastDayOfEpiWeek));

                return !dataCollectorNotDeployedInWeek;
            }

            private bool IsWeekFiltersActive(DataCollectorPerformanceFiltersRequestDto filters) =>
                filters.EpiWeekFilters.Any(IsReportingStatusFilterActiveForWeek);

            private bool IsReportingStatusFilterActiveForWeek(PerformanceStatusFilterDto weekFilter) =>
                !weekFilter.NotReporting
                || !weekFilter.ReportingCorrectly
                || !weekFilter.ReportingWithErrors;
        }
    }
}
