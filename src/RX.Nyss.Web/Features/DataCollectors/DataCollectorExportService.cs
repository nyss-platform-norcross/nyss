using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.DataCollectors
{
    public interface IDataCollectorExportService
    {
        Task<byte[]> ExportAsCsv(int projectId, DataCollectorsFiltersRequestDto dataCollectorsFiltersDto);

        Task<byte[]> ExportAsXls(int projectId, DataCollectorsFiltersRequestDto dataCollectorsFilter);
    }

    public class DataCollectorExportService : IDataCollectorExportService
    {
        private readonly INyssContext _nyssContext;

        private readonly IExcelExportService _excelExportService;

        private readonly IAuthorizationService _authorizationService;

        private readonly IStringsService _stringsService;

        public DataCollectorExportService(
            INyssContext nyssContext,
            IExcelExportService excelExportService,
            IAuthorizationService authorizationService,
            IStringsService stringsService)
        {
            _nyssContext = nyssContext;
            _excelExportService = excelExportService;
            _authorizationService = authorizationService;
            _stringsService = stringsService;
        }

        public async Task<byte[]> ExportAsCsv(int projectId, DataCollectorsFiltersRequestDto dataCollectorsFiltersDto)
        {
            var strings = await _stringsService.GetForCurrentUser();

            var dataCollectors = await GetDataCollectorsExportData(projectId, strings, dataCollectorsFiltersDto);

            return GetCsvData(dataCollectors, strings);
        }

        public async Task<byte[]> ExportAsXls(int projectId, DataCollectorsFiltersRequestDto dataCollectorsFilter)
        {
            var strings = await _stringsService.GetForCurrentUser();

            var dataCollectors = await GetDataCollectorsExportData(projectId, strings, dataCollectorsFilter);

            var excelSheet = GetExcelData(dataCollectors, strings);
            return excelSheet.GetAsByteArray();
        }

        private byte[] GetCsvData(List<ExportDataCollectorsResponseDto> dataCollectors, StringsResourcesVault strings)
        {
            var columnLabels = new List<string>
            {
                strings["dataCollectors.export.dataCollectorType"],
                strings["dataCollectors.export.name"],
                strings["dataCollectors.export.displayName"],
                strings["dataCollectors.export.phoneNumber"],
                strings["dataCollectors.export.additionalPhoneNumber"],
                strings["dataCollectors.export.sex"],
                strings["dataCollectors.export.birthGroupDecade"],
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
                    dc.TrainingStatus,
                    dc.Deployed
                });

            return _excelExportService.ToCsv(dataCollectorsData, columnLabels);
        }

        private ExcelPackage GetExcelData(
            List<ExportDataCollectorsResponseDto> dataCollectors,
            StringsResourcesVault strings)
        {
            var columnLabels = new List<string>
            {
                strings["dataCollectors.export.dataCollectorType"],
                strings["dataCollectors.export.name"],
                strings["dataCollectors.export.displayName"],
                strings["dataCollectors.export.phoneNumber"],
                strings["dataCollectors.export.additionalPhoneNumber"],
                strings["dataCollectors.export.sex"],
                strings["dataCollectors.export.birthGroupDecade"],
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

            var package = new ExcelPackage();
            var title = strings["dataCollectors.export.title"];

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
                worksheet.Cells[index, 16].Value = dataCollector.Deployed;
            }

            worksheet.Column(1).Width = 20;
            worksheet.Column(4).Width = 20;

            return package;
        }

        private async Task<List<ExportDataCollectorsResponseDto>> GetDataCollectorsExportData(
            int projectId,
            StringsResourcesVault strings,
            DataCollectorsFiltersRequestDto dataCollectorsFilter)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            var nationalSocietyId = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => p.NationalSocietyId)
                .SingleOrDefaultAsync();

            var currentUserOrganization = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.User == currentUser && uns.NationalSocietyId == nationalSocietyId)
                .Select(uns => uns.Organization)
                .SingleOrDefaultAsync();

            return await _nyssContext.DataCollectors
                .FilterByProject(projectId)
                .FilterByArea(dataCollectorsFilter.Area)
                .FilterBySupervisor(dataCollectorsFilter.SupervisorId)
                .FilterBySex(dataCollectorsFilter.Sex)
                .FilterByTrainingMode(dataCollectorsFilter.TrainingStatus)
                .FilterByOrganization(currentUserOrganization)
                .Where(dc => dc.DeletedAt == null)
                .SelectMany(dc => dc.DataCollectorLocations.Select(dcl => new ExportDataCollectorsResponseDto
                {
                    DataCollectorType = dc.DataCollectorType == DataCollectorType.Human ? strings["dataCollectors.dataCollectorType.human"] :
                        dc.DataCollectorType == DataCollectorType.CollectionPoint ? strings["dataCollectors.dataCollectorType.collectionPoint"] : string.Empty,
                    Name = dc.Name,
                    DisplayName = dc.DisplayName,
                    PhoneNumber = dc.PhoneNumber,
                    AdditionalPhoneNumber = dc.AdditionalPhoneNumber,
                    Sex = dc.Sex,
                    BirthGroupDecade = dc.BirthGroupDecade,
                    Region = dcl.Village.District.Region.Name,
                    District = dcl.Village.District.Name,
                    Village = dcl.Village.Name,
                    Zone = dcl.Zone.Name,
                    Latitude = dcl.Location.Y,
                    Longitude = dcl.Location.X,
                    Supervisor = dc.Supervisor.Name,
                    TrainingStatus = dc.IsInTrainingMode
                        ? strings["dataCollectors.export.isInTraining"]
                        : strings["dataCollectors.export.isNotInTraining"],
                    Deployed = dc.Deployed
                        ? strings["dataCollectors.deployedMode.deployed"]
                        : strings["dataCollectors.deployedMode.notDeployed"]
                }))
                .OrderBy(dc => dc.Name)
                .ThenBy(dc => dc.DisplayName)
                .ToListAsync();
        }
    }
}
