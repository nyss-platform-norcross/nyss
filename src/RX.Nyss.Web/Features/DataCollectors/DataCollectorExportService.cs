using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.DataCollectors
{
    public interface IDataCollectorExportService
    {
        Task<List<ExportDataCollectorsResponseDto>> GetDataCollectorsExportData(int projectId, StringsResourcesVault strings, DataCollectorsFiltersRequestDto filters);
    }

    public class DataCollectorExportService : IDataCollectorExportService
    {
        private readonly INyssContext _nyssContext;

        private readonly IAuthorizationService _authorizationService;

        public DataCollectorExportService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<List<ExportDataCollectorsResponseDto>> GetDataCollectorsExportData(
            int projectId,
            StringsResourcesVault strings,
            DataCollectorsFiltersRequestDto filters)
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
                .FilterByArea(filters.Locations)
                .FilterBySupervisor(filters.SupervisorId)
                .FilterBySex(filters.Sex)
                .FilterByTrainingMode(filters.TrainingStatus)
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
                    BirthDecade = dc.BirthGroupDecade,
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
