using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollector.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Services.Geolocation;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.DataCollector
{
    public interface IDataCollectorService
    {
        Task<Result> CreateDataCollector(int projectId, CreateDataCollectorRequestDto createDto);
        Task<Result> EditDataCollector(EditDataCollectorRequestDto editDto);
        Task<Result> RemoveDataCollector(int dataCollectorId);
        Task<Result<GetDataCollectorResponseDto>> GetDataCollector(int dataCollectorId);
        Task<Result<IEnumerable<DataCollectorResponseDto>>> ListDataCollectors(int projectId, string userIdentityName, IEnumerable<string> roles);
        Task<Result<DataCollectorFormDataResponse>> GetFormData(int projectId, string identityName);
        Task<bool> GetDataCollectorIsSubordinateOfSupervisor(string supervisorIdentityName, int dataCollectorId);
        Task<Result<List<MapOverviewLocationResponseDto>>> GetMapOverview(int projectId, DateTime from, DateTime to, string userIdentityName, IEnumerable<string> roles);
    }

    public class DataCollectorService : IDataCollectorService
    {
        private const double DefaultLatitude = 11.5024338; // Africa
        private const double DefaultLongitude = 17.7578122;

        private readonly INyssContext _nyssContext;
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;
        private readonly IGeolocationService _geolocationService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public DataCollectorService(
            INyssContext nyssContext,
            INationalSocietyStructureService nationalSocietyStructureService,
            IGeolocationService geolocationService,
            IDateTimeProvider dateTimeProvider)
        {
            _nyssContext = nyssContext;
            _nationalSocietyStructureService = nationalSocietyStructureService;
            _geolocationService = geolocationService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result<GetDataCollectorResponseDto>> GetDataCollector(int dataCollectorId)
        {
            var dataCollector = await _nyssContext.DataCollectors
                .Include(dc => dc.Project)
                    .ThenInclude(p => p.NationalSociety)
                .Include(dc => dc.Supervisor)
                .Include(dc => dc.Zone)
                .Include(dc => dc.Village)
                    .ThenInclude(v => v.District)
                        .ThenInclude(d => d.Region)
                .SingleAsync(dc => dc.Id == dataCollectorId);

            var regions = await _nationalSocietyStructureService.GetRegions(dataCollector.Project.NationalSociety.Id);
            var districts = await _nationalSocietyStructureService.GetDistricts(dataCollector.Village.District.Region.Id);
            var villages = await _nationalSocietyStructureService.GetVillages(dataCollector.Village.District.Id);
            var zones = await _nationalSocietyStructureService.GetZones(dataCollector.Village.Id);

            var dto = new GetDataCollectorResponseDto
            {
                Id = dataCollector.Id,
                Name = dataCollector.Name,
                DisplayName = dataCollector.DisplayName,
                DataCollectorType = dataCollector.DataCollectorType,
                Sex = dataCollector.Sex,
                BirthGroupDecade = dataCollector.BirthGroupDecade,
                PhoneNumber = dataCollector.PhoneNumber,
                AdditionalPhoneNumber = dataCollector.AdditionalPhoneNumber,
                Latitude = dataCollector.Location.X,
                Longitude = dataCollector.Location.Y,
                SupervisorId = dataCollector.Supervisor.Id,
                RegionId = dataCollector.Village.District.Region.Id,
                DistrictId = dataCollector.Village.District.Id,
                VillageId = dataCollector.Village.Id,
                ZoneId = dataCollector.Zone?.Id,
                NationalSocietyId = dataCollector.Project.NationalSociety.Id,
                ProjectId = dataCollector.Project.Id,
                FormData = new GetDataCollectorResponseDto.FormDataDto
                {
                    Regions = regions.Value,
                    Districts = districts.Value,
                    Villages = villages.Value,
                    Zones = zones.Value,
                    Supervisors = await GetSupervisors(dataCollector.Project.NationalSociety.Id)
                }
            };

            return Success(dto);
        }

        public async Task<Result<DataCollectorFormDataResponse>> GetFormData(int projectId, string identityName)
        {
            var projectData = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(dc => new
                {
                    NationalSocietyId = dc.NationalSociety.Id,
                    CountryName = dc.NationalSociety.Country.Name
                })
                .SingleAsync();

            var regions = await _nationalSocietyStructureService.GetRegions(projectData.NationalSocietyId);

            var locationFromCountry = await _geolocationService.GetLocationFromCountry(projectData.CountryName);

            var defaultSupervisorId = await _nyssContext.Users
                .Where(u => u.EmailAddress == identityName && u.Role == Role.Supervisor)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();

            return Success(new DataCollectorFormDataResponse
            {
                NationalSocietyId = projectData.NationalSocietyId,
                Regions = regions.Value,
                Supervisors = await GetSupervisors(projectData.NationalSocietyId),
                DefaultSupervisorId = defaultSupervisorId,
                DefaultLocation = locationFromCountry.IsSuccess
                    ? new LocationDto
                    {
                        Latitude = locationFromCountry.Value.Latitude,
                        Longitude = locationFromCountry.Value.Longitude
                    }
                    : new LocationDto
                    {
                        Latitude = DefaultLatitude,
                        Longitude = DefaultLongitude
                    }
            });
        }

        public async Task<Result<IEnumerable<DataCollectorResponseDto>>> ListDataCollectors(int projectId, string userIdentityName, IEnumerable<string> roles)
        {
            var dataCollectorsQuery = roles.Contains(Role.Supervisor.ToString())
                ? _nyssContext.DataCollectors.Where(dc => dc.Supervisor.EmailAddress == userIdentityName)
                : _nyssContext.DataCollectors;

            var dataCollectors = await dataCollectorsQuery
                .Where(dc => dc.Project.Id == projectId)
                .OrderBy(dc => dc.Name)
                .Select(dc => new DataCollectorResponseDto
                {
                    Id = dc.Id,
                    Name = dc.Name,
                    DisplayName = dc.DisplayName,
                    PhoneNumber = dc.PhoneNumber,
                    Village = dc.Village.Name,
                    District = dc.Village.District.Name,
                    Region = dc.Village.District.Region.Name,
                    Sex = dc.Sex
                })
                .ToListAsync();

            return Success((IEnumerable<DataCollectorResponseDto>)dataCollectors);
        }

        public async Task<Result> CreateDataCollector(int projectId, CreateDataCollectorRequestDto createDto)
        {
            var phoneNumberExists = await _nyssContext.DataCollectors
                .AnyAsync(dc => dc.PhoneNumber == createDto.PhoneNumber);

            if (phoneNumberExists)
            {
                return Error(ResultKey.DataCollector.PhoneNumberAlreadyExists).Cast<int>();
            }

            var project = await _nyssContext.Projects
                .Include(p => p.NationalSociety)
                .SingleAsync(p => p.Id == projectId);

            var nationalSocietyId = project.NationalSociety.Id;

            var supervisor = await _nyssContext.UserNationalSocieties
                .Where(u => u.User.Id == createDto.SupervisorId && u.User.Role == Role.Supervisor && u.NationalSocietyId == nationalSocietyId)
                .Select(u => (SupervisorUser)u.User)
                .SingleAsync();

            var village = await _nyssContext.Villages
                .SingleAsync(v => v.Id == createDto.VillageId && v.District.Region.NationalSociety.Id == nationalSocietyId);

            var zone = createDto.ZoneId != null
                ? await _nyssContext.Zones.SingleAsync(z => z.Id == createDto.ZoneId.Value)
                : null;

            var dataCollector = new Nyss.Data.Models.DataCollector
            {
                Name = createDto.Name,
                DisplayName = createDto.DisplayName,
                PhoneNumber = createDto.PhoneNumber,
                AdditionalPhoneNumber = createDto.AdditionalPhoneNumber,
                BirthGroupDecade = createDto.BirthGroupDecade,
                Sex = createDto.Sex,
                DataCollectorType = DataCollectorType.Human,
                Location = CreatePoint(createDto.Latitude, createDto.Longitude),
                Village = village,
                Supervisor = supervisor,
                Project = project,
                Zone = zone,
                CreatedAt = _dateTimeProvider.UtcNow
            };

            await _nyssContext.AddAsync(dataCollector);
            await _nyssContext.SaveChangesAsync();
            return Success(ResultKey.DataCollector.CreateSuccess);
        }

        public async Task<Result> EditDataCollector(EditDataCollectorRequestDto editDto)
        {
            var phoneNumberExists = await _nyssContext.DataCollectors
                .AnyAsync(dc => dc.PhoneNumber == editDto.PhoneNumber && dc.Id != editDto.Id);

            if (phoneNumberExists)
            {
                return Error(ResultKey.DataCollector.PhoneNumberAlreadyExists).Cast<int>();
            }

            var dataCollector = await _nyssContext.DataCollectors
                .Include(dc => dc.Project)
                    .ThenInclude(x => x.NationalSociety)
                .Include(dc => dc.Supervisor)
                .Include(dc => dc.Village)
                    .ThenInclude(v => v.District)
                        .ThenInclude(d => d.Region)
                .Include(dc => dc.Zone)
                .SingleAsync(dc => dc.Id == editDto.Id);

            var nationalSocietyId = dataCollector.Project.NationalSociety.Id;

            var supervisor = await _nyssContext.UserNationalSocieties
                .Where(u => u.User.Id == editDto.SupervisorId && u.User.Role == Role.Supervisor && u.NationalSocietyId == nationalSocietyId)
                .Select(u => (SupervisorUser)u.User)
                .SingleAsync();

            var village = await _nyssContext.Villages
                .SingleAsync(v => v.Id == editDto.VillageId && v.District.Region.NationalSociety.Id == nationalSocietyId);

            var zone = editDto.ZoneId != null
                ? await _nyssContext.Zones.SingleAsync(z => z.Id == editDto.ZoneId.Value)
                : null;

            dataCollector.Name = editDto.Name;
            dataCollector.DisplayName = editDto.DisplayName;
            dataCollector.PhoneNumber = editDto.PhoneNumber;
            dataCollector.AdditionalPhoneNumber = editDto.AdditionalPhoneNumber;
            dataCollector.Location = CreatePoint(editDto.Latitude, editDto.Longitude);
            dataCollector.Sex = editDto.Sex;
            dataCollector.Village = village;
            dataCollector.Supervisor = supervisor;
            dataCollector.Zone = zone;

            await _nyssContext.SaveChangesAsync();
            return SuccessMessage(ResultKey.DataCollector.EditSuccess);
        }

        public async Task<Result> RemoveDataCollector(int dataCollectorId)
        {
            var dataCollector = await _nyssContext.DataCollectors.FindAsync(dataCollectorId);

            if (dataCollector == null)
            {
                return Error(ResultKey.DataCollector.DataCollectorNotFound);
            }

            _nyssContext.DataCollectors.Remove(dataCollector);
            await _nyssContext.SaveChangesAsync();
            return SuccessMessage(ResultKey.DataCollector.RemoveSuccess);
        }

        private async Task<List<DataCollectorSupervisorResponseDto>> GetSupervisors(int nationalSocietyId) =>
            await _nyssContext.UserNationalSocieties.Where(u => u.NationalSocietyId == nationalSocietyId && u.User is SupervisorUser)
                .Select(u => new DataCollectorSupervisorResponseDto
                {
                    Id = u.User.Id,                
                    Name = u.User.Name,                
                })
                .ToListAsync();

        private static Point CreatePoint(double latitude, double longitude)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            return geometryFactory.CreatePoint(new Coordinate(latitude, longitude));
        }

        public async Task<bool> GetDataCollectorIsSubordinateOfSupervisor(string supervisorIdentityName, int dataCollectorId) =>
            await _nyssContext.DataCollectors.AnyAsync(dc => dc.Id == dataCollectorId && dc.Supervisor.EmailAddress == supervisorIdentityName);


        public async Task<Result<List<MapOverviewLocationResponseDto>>> GetMapOverview(int projectId, DateTime from, DateTime to, string userIdentityName, IEnumerable<string> roles)
        {
            var dataCollectors = roles.Contains(Role.Supervisor.ToString())
                ? _nyssContext.DataCollectors.Where(dc => dc.Supervisor.EmailAddress == userIdentityName)
                : _nyssContext.DataCollectors;

            var dataCollectorsWithNoReports = dataCollectors
                .Where(dc => !dc.Reports.Any(r => r.CreatedAt >= from.Date && r.CreatedAt < to.Date.AddDays(1)))
                .Where(dc => dc.Project.Id == projectId)
                .Select(dc => new
                {
                    DataCollectorName = dc.DisplayName,
                    dc.Location.X,
                    dc.Location.Y,
                    InvalidReport = 0,
                    ValidReport = 0,
                    NoReport = 1
                });

            var dataCollectorsWithReports = dataCollectors
                .Where(dc => dc.Reports.Any(r => r.CreatedAt >= from.Date && r.CreatedAt < to.Date.AddDays(1)))
                .Where(dc => dc.Project.Id == projectId)
                .Select(dc => new
                {
                    DataCollectorName = dc.DisplayName,
                    dc.Location.X,
                    dc.Location.Y,
                    InvalidReport = dc.Reports.Where(r => r.CreatedAt >= from.Date && r.CreatedAt < to.Date.AddDays(1)).Any(r => !r.IsValid) ? 1 : 0,
                    ValidReport = dc.Reports.Where(r => r.CreatedAt >= from.Date && r.CreatedAt < to.Date.AddDays(1)).All(r => r.IsValid) ? 1 : 0,
                    NoReport = 0
                });

            var locations = await dataCollectorsWithReports
                .Union(dataCollectorsWithNoReports)
                .GroupBy(x => new { x.X, x.Y })
                .Select(location => new MapOverviewLocationResponseDto
                {
                    Location = new LocationDto { Latitude = location.Key.X, Longitude = location.Key.Y },
                    CountReportingCorrectly = location.Sum(x => x.ValidReport),
                    CountReportingWithErrors = location.Sum(x => x.InvalidReport),
                    CountNotReporting = location.Sum(x => x.NoReport)
                })
                .ToListAsync();

            return Success(locations);
        }
    }
}
