using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.DataCollectors
{
    public interface IDataCollectorExportService
    {
        Task<byte[]> Export(int projectId);
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

        public async Task<byte[]> Export(int projectId)
        {
            var dataCollectors = await _nyssContext.DataCollectors
                .Where(dc => dc.DeletedAt == null)
                .Select(dc => new ExportDataCollectorsResponseDto
                {
                    DataCollectorType = dc.DataCollectorType,
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
                    Supervisor = dc.Supervisor.Name
                }).ToListAsync();

            return await GetExcelData(dataCollectors);
        }

        public async Task<byte[]> GetExcelData(List<ExportDataCollectorsResponseDto> dataCollectors)
        {
            var userName = _authorizationService.GetCurrentUserName();
            var userApplicationLanguage = _nyssContext.Users
                .Where(u => u.EmailAddress == userName)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .Single();

            var stringResources = (await _stringsResourcesService.GetStringsResources(userApplicationLanguage)).Value;

            var columnLabels = new List<string>()
            {
                GetStringResource(stringResources,"dataCollectors.export.dataCollectorType"),
                GetStringResource(stringResources,"dataCollectors.export.name"),
                GetStringResource(stringResources,"dataCollectors.export.displayName"),
                GetStringResource(stringResources,"dataCollectors.export.phoneNumber"),
                GetStringResource(stringResources,"dataCollectors.export.additionalPhoneNumber"),
                GetStringResource(stringResources,"dataCollectors.export.sex"),
                GetStringResource(stringResources,"dataCollectors.export.birthGroupDecade"),
                GetStringResource(stringResources,"dataCollectors.export.region"),
                GetStringResource(stringResources,"dataCollectors.export.district"),
                GetStringResource(stringResources,"dataCollectors.export.village"),
                GetStringResource(stringResources,"dataCollectors.export.zone"),
                GetStringResource(stringResources,"dataCollectors.export.latitude"),
                GetStringResource(stringResources,"dataCollectors.export.longitude"),
                GetStringResource(stringResources,"dataCollectors.export.supervisor")
            };

            var dataCollectorsData = dataCollectors
                .Select(dc => new
                {
                    dc.DataCollectorType,
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
                    dc.Supervisor
                });

            return _excelExportService.ToCsv(dataCollectorsData, columnLabels);
        }

        private string GetStringResource(IDictionary<string, string> stringResources, string key) =>
            stringResources.Keys.Contains(key) ? stringResources[key] : key;
    }
}
