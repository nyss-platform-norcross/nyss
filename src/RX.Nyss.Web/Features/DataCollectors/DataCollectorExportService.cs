using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.DataCollectors
{
    public interface IDataCollectorExportService
    {
        Task<byte[]> ExportAsCsv(int projectId);
        Task<byte[]> ExportAsXls(int projectId);
    }

    public class DataCollectorExportService : IDataCollectorExportService
    {
        private readonly INyssContext _nyssContext;
        private readonly IExcelExportService _excelExportService;
        private readonly IStringsResourcesService _stringsResourcesService;
        private readonly IAuthorizationService _authorizationService;

        public DataCollectorExportService(
            INyssContext nyssContext,
            IExcelExportService excelExportService,
            IStringsResourcesService stringsResourcesService,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _excelExportService = excelExportService;
            _stringsResourcesService = stringsResourcesService;
            _authorizationService = authorizationService;
        }

        public async Task<byte[]> ExportAsCsv(int projectId)
        {
            var userName = _authorizationService.GetCurrentUserName();
            var userApplicationLanguage = _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == userName)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .Single();

            var stringResources = (await _stringsResourcesService.GetStringsResources(userApplicationLanguage)).Value;

            var dataCollectors = await GetDataCollectorsExportData(projectId, stringResources);

            return GetCsvData(dataCollectors, stringResources);
        }

        public async Task<byte[]> ExportAsXls(int projectId)
        {
            var userName = _authorizationService.GetCurrentUserName();
            var userApplicationLanguage = _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == userName)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .Single();

            var stringResources = (await _stringsResourcesService.GetStringsResources(userApplicationLanguage)).Value;

            var dataCollectors = await GetDataCollectorsExportData(projectId, stringResources);

            var excelSheet = GetExcelData(dataCollectors, stringResources);
            return excelSheet.GetAsByteArray();
        }

        private byte[] GetCsvData(List<ExportDataCollectorsResponseDto> dataCollectors, IDictionary<string, string> stringResources)
        {
            var columnLabels = new List<string>
            {
                GetStringResource(stringResources, "dataCollectors.export.dataCollectorType"),
                GetStringResource(stringResources, "dataCollectors.export.name"),
                GetStringResource(stringResources, "dataCollectors.export.displayName"),
                GetStringResource(stringResources, "dataCollectors.export.phoneNumber"),
                GetStringResource(stringResources, "dataCollectors.export.additionalPhoneNumber"),
                GetStringResource(stringResources, "dataCollectors.export.sex"),
                GetStringResource(stringResources, "dataCollectors.export.birthGroupDecade"),
                GetStringResource(stringResources, "dataCollectors.export.region"),
                GetStringResource(stringResources, "dataCollectors.export.district"),
                GetStringResource(stringResources, "dataCollectors.export.village"),
                GetStringResource(stringResources, "dataCollectors.export.zone"),
                GetStringResource(stringResources, "dataCollectors.export.latitude"),
                GetStringResource(stringResources, "dataCollectors.export.longitude"),
                GetStringResource(stringResources, "dataCollectors.export.supervisor"),
                GetStringResource(stringResources, "dataCollectors.export.trainingStatus")
            };

            var dataCollectorsData = dataCollectors
                .Select(dc => new
                {
                    DataCollectorType = dc.DataCollectorType,
                    dc.Name,
                    dc.DisplayName,
                    dc.PhoneNumber,
                    dc.AdditionalPhoneNumber,
                    dc.Sex,
                    dc.BirthGroupDecade,
                    dc.Region,
                    dc.District,
                    dc.Village,
                    dc.Zone,
                    dc.Latitude,
                    dc.Longitude,
                    dc.Supervisor,
                    dc.TrainingStatus
                });

            return _excelExportService.ToCsv(dataCollectorsData, columnLabels);
        }

        private ExcelPackage GetExcelData(List<ExportDataCollectorsResponseDto> dataCollectors, IDictionary<string, string> stringResources)
        {
            var columnLabels = new List<string>
            {
                GetStringResource(stringResources, "dataCollectors.export.dataCollectorType"),
                GetStringResource(stringResources, "dataCollectors.export.name"),
                GetStringResource(stringResources, "dataCollectors.export.displayName"),
                GetStringResource(stringResources, "dataCollectors.export.phoneNumber"),
                GetStringResource(stringResources, "dataCollectors.export.additionalPhoneNumber"),
                GetStringResource(stringResources, "dataCollectors.export.sex"),
                GetStringResource(stringResources, "dataCollectors.export.birthGroupDecade"),
                GetStringResource(stringResources, "dataCollectors.export.region"),
                GetStringResource(stringResources, "dataCollectors.export.district"),
                GetStringResource(stringResources, "dataCollectors.export.village"),
                GetStringResource(stringResources, "dataCollectors.export.zone"),
                GetStringResource(stringResources, "dataCollectors.export.latitude"),
                GetStringResource(stringResources, "dataCollectors.export.longitude"),
                GetStringResource(stringResources, "dataCollectors.export.supervisor"),
                GetStringResource(stringResources, "dataCollectors.export.trainingStatus")
            };

            var package = new ExcelPackage();
            var title = GetStringResource(stringResources, "dataCollectors.export.title");

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
                worksheet.Cells[index, 7].Value = dataCollector.BirthGroupDecade;
                worksheet.Cells[index, 8].Value = dataCollector.Region;
                worksheet.Cells[index, 9].Value = dataCollector.District;
                worksheet.Cells[index, 10].Value = dataCollector.Village;
                worksheet.Cells[index, 11].Value = dataCollector.Zone;
                worksheet.Cells[index, 12].Value = dataCollector.Latitude;
                worksheet.Cells[index, 13].Value = dataCollector.Longitude;
                worksheet.Cells[index, 14].Value = dataCollector.Supervisor;
                worksheet.Cells[index, 15].Value = dataCollector.TrainingStatus;
            }

            worksheet.Column(1).Width = 20;
            worksheet.Column(4).Width = 20;

            return package;
        }

        private async Task<List<ExportDataCollectorsResponseDto>> GetDataCollectorsExportData(int projectId, IDictionary<string, string> stringResources) => 
            await _nyssContext.DataCollectors
                .FilterByProject(projectId)
                .Where(dc => dc.DeletedAt == null)
                .Select(dc => new ExportDataCollectorsResponseDto
                {
                    DataCollectorType = dc.DataCollectorType == DataCollectorType.Human ? GetStringResource(stringResources, "dataCollectors.dataCollectorType.human") :
                        dc.DataCollectorType == DataCollectorType.CollectionPoint ? GetStringResource(stringResources, "dataCollectors.dataCollectorType.collectionPoint") : string.Empty,
                    Name = dc.Name,
                    DisplayName = dc.DisplayName,
                    PhoneNumber = dc.PhoneNumber,
                    AdditionalPhoneNumber = dc.AdditionalPhoneNumber,
                    Sex = dc.Sex,
                    BirthGroupDecade = dc.BirthGroupDecade,
                    Region = dc.Village.District.Region.Name,
                    District = dc.Village.District.Name,
                    Village = dc.Village.Name,
                    Zone = dc.Zone.Name,
                    Latitude = dc.Location.Y,
                    Longitude = dc.Location.X,
                    Supervisor = dc.Supervisor.Name,
                    TrainingStatus = dc.IsInTrainingMode
                        ? GetStringResource(stringResources, "dataCollectors.export.isInTraining")
                        : GetStringResource(stringResources, "dataCollectors.export.isNotInTraining")
                })
                .OrderBy(dc => dc.Name)
                .ThenBy(dc => dc.DisplayName)
                .ToListAsync();

        private string GetStringResource(IDictionary<string, string> stringResources, string key) =>
            stringResources.Keys.Contains(key)
                ? stringResources[key]
                : key;
    }
}
