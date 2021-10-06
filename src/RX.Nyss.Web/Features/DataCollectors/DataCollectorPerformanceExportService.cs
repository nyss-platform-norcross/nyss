using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using OfficeOpenXml;
using OfficeOpenXml.ConditionalFormatting;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.DataCollectors.DataContracts;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.DataCollectors
{
    public interface IDataCollectorPerformanceExportService
    {
        public Task<byte[]> Export(int projectId, DataCollectorPerformanceFiltersRequestDto filters);
    }

    public class DataCollectorPerformanceExportService : IDataCollectorPerformanceExportService
    {
        private readonly INyssContext _nyssContext;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IDataCollectorService _dataCollectorService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAuthorizationService _authorizationService;

        public DataCollectorPerformanceExportService(
            INyssContext nyssContext,
            IStringsResourcesService stringsResourcesService,
            IDataCollectorService dataCollectorService,
            IDateTimeProvider dateTimeProvider,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _stringsResourcesService = stringsResourcesService;
            _dataCollectorService = dataCollectorService;
            _dateTimeProvider = dateTimeProvider;
            _authorizationService = authorizationService;
        }

        public async Task<byte[]> Export(int projectId, DataCollectorPerformanceFiltersRequestDto filters)
        {
            var userName = _authorizationService.GetCurrentUserName();
            var userApplicationLanguage = _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == userName)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .Single();

            var stringResources = (await _stringsResourcesService.GetStringsResources(userApplicationLanguage)).Value;

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
                .FirstOrDefaultAsync();

            var currentDate = _dateTimeProvider.UtcNow;
            var epiDateRange = _dateTimeProvider.GetEpiDateRange(projectStartDate, currentDate).ToList();

            var dataCollectorsWithReportsData = await GetDataCollectorsWithReportData(dataCollectors);

            var dataCollectorPerformance = GetDataCollectorPerformance(dataCollectorsWithReportsData, currentDate, epiDateRange);

            var excelSheet = GetExcelData(stringResources, dataCollectorPerformance, epiDateRange);
            return excelSheet.GetAsByteArray();
        }

        private ExcelPackage GetExcelData(IDictionary<string, StringResourceValue> stringResources, List<DataCollectorPerformance> dataCollectorPerformances, List<EpiDate> epiDateRange)
        {
            var columnLabels = GetColumnLabels(stringResources, epiDateRange);
            var package = new ExcelPackage();
            var title = GetStringResource(stringResources, "dataCollectors.performanceExport.title");

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
                }
            }

            var dimension = worksheet.Dimension;
            var rangeAddress = $"D2:{dimension.End.Address}";
            var excelAddress = new ExcelAddress(rangeAddress);
            var formatting = worksheet.ConditionalFormatting.AddThreeIconSet(excelAddress, eExcelconditionalFormatting3IconsSetType.Symbols);
            formatting.Reverse = true;

            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 20;
            worksheet.Column(3).Width = 20;

            for (var i = 4; i <= dimension.Columns; i++)
            {
                worksheet.Column(i).Width = 10;
            }

            return package;
        }

        private async Task<List<DataCollectorWithRawReportDataForExport>> GetDataCollectorsWithReportData(IIncludableQueryable<DataCollector, IEnumerable<DataCollectorLocation>> dataCollectors) =>
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

        private ReportingStatus GetDataCollectorStatus(IGrouping<EpiDate, RawReportDataForExport> grouping) =>
            grouping != null && grouping.Any()
                ? grouping.All(x => x.IsValid) ? ReportingStatus.ReportingCorrectly : ReportingStatus.ReportingWithErrors
                : ReportingStatus.NotReporting;

        private List<string> GetColumnLabels(IDictionary<string, StringResourceValue> stringResources, IEnumerable<EpiDate> epiDateRange)
        {
            var epiWeeks = epiDateRange.Select(epiDate => $"{epiDate.EpiYear}/{GetStringResource(stringResources, "dataCollectors.performanceExport.epiWeekIdentifier")} {epiDate.EpiWeek}");
            var columnbLabels = new List<string>
            {
                GetStringResource(stringResources, "dataCollectors.performanceList.name"),
                GetStringResource(stringResources, "dataCollectors.performanceList.villageName"),
                GetStringResource(stringResources, "dataCollectors.performanceList.daysSinceLastReport")
            };

            columnbLabels.AddRange(epiWeeks);
            return columnbLabels;
        }

        private string GetStringResource(IDictionary<string, StringResourceValue> stringResources, string key) =>
            stringResources.Keys.Contains(key)
                ? stringResources[key].Value
                : key;

        private bool DataCollectorExistedInWeek(EpiDate date, DateTime dataCollectorCreated)
        {
            var epiDataDataCollectorCreated = _dateTimeProvider.GetEpiDate(dataCollectorCreated);
            return epiDataDataCollectorCreated.EpiYear <= date.EpiYear && epiDataDataCollectorCreated.EpiWeek <= date.EpiWeek;
        }
    }
}
