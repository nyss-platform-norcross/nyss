using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Services.Geolocation;
using RX.Nyss.Web.Utils;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.DataCollectors
{
    public interface IDataCollectorService
    {
        Task<Result> CreateDataCollector(int projectId, CreateDataCollectorRequestDto createDto);
        Task<Result> EditDataCollector(EditDataCollectorRequestDto editDto);
        Task<Result> RemoveDataCollector(int dataCollectorId);
        Task<Result<GetDataCollectorResponseDto>> GetDataCollector(int dataCollectorId);
        Task<Result<IEnumerable<DataCollectorResponseDto>>> ListDataCollectors(int projectId);
        Task<Result<DataCollectorFormDataResponse>> GetFormData(int projectId);
        Task<Result<MapOverviewResponseDto>> GetMapOverview(int projectId, DateTime from, DateTime to);
        Task<Result<List<MapOverviewDataCollectorResponseDto>>> GetMapOverviewDetails(int projectId, DateTime @from, DateTime to, double lat, double lng);
        Task<Result<List<DataCollectorPerformanceResponseDto>>> GetDataCollectorPerformance(int projectId);
        Task<Result> SetTrainingState(int dataCollectorId, bool isInTraining);
        Task AnonymizeDataCollectorsWithReports(int projectId);
    }

    public class DataCollectorService : IDataCollectorService
    {
        private const double DefaultLatitude = 11.5024338; // Africa
        private const double DefaultLongitude = 17.7578122;

        private readonly INyssContext _nyssContext;
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;
        private readonly IGeolocationService _geolocationService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAuthorizationService _authorizationService;

        public DataCollectorService(
            INyssContext nyssContext,
            INationalSocietyStructureService nationalSocietyStructureService,
            IGeolocationService geolocationService,
            IDateTimeProvider dateTimeProvider,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _nationalSocietyStructureService = nationalSocietyStructureService;
            _geolocationService = geolocationService;
            _dateTimeProvider = dateTimeProvider;
            _authorizationService = authorizationService;
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
                Latitude = dataCollector.Location.Y,
                Longitude = dataCollector.Location.X,
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
                    Supervisors = await GetSupervisors(dataCollector.Project.Id)
                }
            };

            return Success(dto);
        }

        public async Task<Result<DataCollectorFormDataResponse>> GetFormData(int projectId)
        {
            var identityName = _authorizationService.GetCurrentUserName();
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

            var defaultSupervisorId = await _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == identityName && u.Role == Role.Supervisor)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();

            return Success(new DataCollectorFormDataResponse
            {
                NationalSocietyId = projectData.NationalSocietyId,
                Regions = regions.Value,
                Supervisors = await GetSupervisors(projectId),
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

        public async Task<Result<IEnumerable<DataCollectorResponseDto>>> ListDataCollectors(int projectId)
        {
            var userIdentityName = _authorizationService.GetCurrentUserName();
            var dataCollectorsQuery = _authorizationService.IsCurrentUserInRole(Role.Supervisor)
                ? _nyssContext.DataCollectors.Where(dc => dc.Supervisor.EmailAddress == userIdentityName)
                : _nyssContext.DataCollectors;

            var dataCollectors = await dataCollectorsQuery
                .Where(dc => dc.DeletedAt == null)
                .Where(dc => dc.Project.Id == projectId)
                .Select(dc => new DataCollectorResponseDto
                {
                    Id = dc.Id,
                    DataCollectorType = dc.DataCollectorType,
                    Name = dc.Name,
                    DisplayName = dc.DisplayName,
                    PhoneNumber = dc.PhoneNumber,
                    Village = dc.Village.Name,
                    District = dc.Village.District.Name,
                    Region = dc.Village.District.Region.Name,
                    Sex = dc.Sex,
                    IsInTrainingMode = dc.IsInTrainingMode
                })
                .OrderBy(dc => dc.Name)
                    .ThenBy(dc => dc.DisplayName)
                .ToListAsync();

            return Success((IEnumerable<DataCollectorResponseDto>)dataCollectors);
        }

        public async Task<Result> CreateDataCollector(int projectId, CreateDataCollectorRequestDto createDto)
        {
            var phoneNumberExists = await _nyssContext.DataCollectors
                .Where(dc => dc.Project.State == ProjectState.Open)
                .AnyAsync(dc => dc.PhoneNumber == createDto.PhoneNumber);

            if (phoneNumberExists)
            {
                return Error(ResultKey.DataCollector.PhoneNumberAlreadyExists).Cast<int>();
            }

            var project = await _nyssContext.Projects
                .Include(p => p.NationalSociety)
                .SingleAsync(p => p.Id == projectId);

            if (project.State != ProjectState.Open)
            {
                return Error(ResultKey.DataCollector.ProjectIsClosed);
            }

            var nationalSocietyId = project.NationalSociety.Id;

            var supervisor = await _nyssContext.UserNationalSocieties
                .FilterAvailableUsers()
                .Where(u => u.User.Id == createDto.SupervisorId && u.User.Role == Role.Supervisor && u.NationalSocietyId == nationalSocietyId)
                .Select(u => (SupervisorUser)u.User)
                .SingleAsync();

            var village = await _nyssContext.Villages
                .SingleAsync(v => v.Id == createDto.VillageId && v.District.Region.NationalSociety.Id == nationalSocietyId);

            var zone = createDto.ZoneId != null
                ? await _nyssContext.Zones.SingleAsync(z => z.Id == createDto.ZoneId.Value)
                : null;

            var dataCollector = new DataCollector
            {
                Name = createDto.Name,
                DisplayName = createDto.DisplayName,
                PhoneNumber = createDto.PhoneNumber,
                AdditionalPhoneNumber = createDto.AdditionalPhoneNumber,
                BirthGroupDecade = createDto.BirthGroupDecade,
                Sex = createDto.Sex,
                DataCollectorType = createDto.DataCollectorType,
                Location = CreatePoint(createDto.Latitude, createDto.Longitude),
                Village = village,
                Supervisor = supervisor,
                Project = project,
                Zone = zone,
                CreatedAt = _dateTimeProvider.UtcNow,
                IsInTrainingMode = true
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

            if (dataCollector.Project.State != ProjectState.Open)
            {
                return Error(ResultKey.DataCollector.ProjectIsClosed);
            }

            var nationalSocietyId = dataCollector.Project.NationalSociety.Id;

            var supervisor = await _nyssContext.UserNationalSocieties
                .FilterAvailableUsers()
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
            dataCollector.BirthGroupDecade = editDto.BirthGroupDecade;
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
            var dataCollector = await _nyssContext.DataCollectors
                .Select(dc => new
                {
                    dc,
                    HasReports = dc.RawReports.Any(),
                    ProjectIsOpen = dc.Project.State == ProjectState.Open
                })
                .SingleOrDefaultAsync(dc => dc.dc.Id == dataCollectorId);

            if (dataCollector == null)
            {
                return Error(ResultKey.DataCollector.DataCollectorNotFound);
            }

            if (!dataCollector.ProjectIsOpen)
            {
                return Error(ResultKey.DataCollector.ProjectIsClosed);
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (dataCollector.HasReports)
                {
                    await AnonymizeDataCollector(dataCollectorId);
                }
                else
                {
                    _nyssContext.DataCollectors.Remove(dataCollector.dc);
                }

                await _nyssContext.SaveChangesAsync();
                transactionScope.Complete();
            }

            return SuccessMessage(ResultKey.DataCollector.RemoveSuccess);
        }

        private async Task AnonymizeDataCollector(int dataCollectorId)
        {
            await _nyssContext.DataCollectors
                .Where(x => x.Id == dataCollectorId)
                .BatchUpdateAsync(x => new Nyss.Data.Models.DataCollector
                {
                    Name = Anonymization.Text,
                    DisplayName = Anonymization.Text,
                    PhoneNumber = Anonymization.Text,
                    AdditionalPhoneNumber = Anonymization.Text,
                    DeletedAt = DateTime.UtcNow
                });

            await _nyssContext.RawReports
                .Where(rawReport => rawReport.DataCollector.Id == dataCollectorId)
                .BatchUpdateAsync(x => new RawReport
                {
                    Sender = Anonymization.Text
                });

            await _nyssContext.Reports
                .Where(report => report.DataCollector.Id == dataCollectorId)
                .BatchUpdateAsync(x => new Nyss.Data.Models.Report
                {
                    PhoneNumber = Anonymization.Text
                });
        }

        public async Task AnonymizeDataCollectorsWithReports(int projectId)
        {
            await _nyssContext.DataCollectors
                .Where(x => x.Project.Id == projectId && x.RawReports.Any())
                .BatchUpdateAsync(x => new Nyss.Data.Models.DataCollector
                {
                    Name = Anonymization.Text,
                    DisplayName = Anonymization.Text,
                    PhoneNumber = Anonymization.Text,
                    AdditionalPhoneNumber = Anonymization.Text,
                    DeletedAt = DateTime.UtcNow
                });

            await _nyssContext.RawReports
                .Where(rawReport => rawReport.DataCollector.Project.Id == projectId)
                .BatchUpdateAsync(x => new RawReport
                {
                    Sender = Anonymization.Text
                });

            await _nyssContext.Reports
                .Where(report => report.ProjectHealthRisk.Project.Id == projectId)
                .BatchUpdateAsync(x => new Nyss.Data.Models.Report
                {
                    PhoneNumber = Anonymization.Text
                });
        }

        private async Task<List<DataCollectorSupervisorResponseDto>> GetSupervisors(int projectId) =>
            await _nyssContext.SupervisorUserProjects
                .FilterAvailableUsers()
                .Where(sup => sup.ProjectId == projectId)
                .Select(sup => new DataCollectorSupervisorResponseDto
                {
                    Id = sup.SupervisorUserId,
                    Name = sup.SupervisorUser.Name
                })
                .ToListAsync();

        private static Point CreatePoint(double latitude, double longitude)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(SpatialReferenceSystemIdentifier.Wgs84);
            return geometryFactory.CreatePoint(new Coordinate(longitude, latitude));
        }

        public async Task<Result<MapOverviewResponseDto>> GetMapOverview(int projectId, DateTime from, DateTime to)
        {
            var userIdentityName = _authorizationService.GetCurrentUserName();
            var endDate = to.Date.AddDays(1);

            var dataCollectors = _authorizationService.IsCurrentUserInRole(Role.Supervisor)
                ? _nyssContext.DataCollectors.Where(dc => dc.Supervisor.EmailAddress == userIdentityName)
                : _nyssContext.DataCollectors;

            var dataCollectorsWithNoReports = dataCollectors
                .Where(dc => dc.CreatedAt < endDate && dc.Name != Anonymization.Text && dc.DeletedAt == null)
                .Where(dc => !dc.RawReports.Any(r => r.IsTraining.HasValue && !r.IsTraining.Value
                                                                           && r.ReceivedAt >= from.Date && r.ReceivedAt < endDate))
                .Where(dc => dc.Project.Id == projectId)
                .Select(dc => new
                {
                    dc.Location.X,
                    dc.Location.Y,
                    InvalidReport = 0,
                    ValidReport = 0,
                    NoReport = 1
                });


            var rawReports = _authorizationService.IsCurrentUserInRole(Role.Supervisor)
                ? _nyssContext.RawReports.Where(dc => dc.DataCollector.Supervisor.EmailAddress == userIdentityName)
                : _nyssContext.RawReports;

            var dataCollectorsWithReports = rawReports
                .Where(r => r.IsTraining.HasValue && !r.IsTraining.Value)
                .Where(r => r.ReceivedAt >= from.Date && r.ReceivedAt < endDate)
                .Where(r => r.DataCollector.CreatedAt < endDate && r.DataCollector.Name != Anonymization.Text && r.DataCollector.DeletedAt == null)
                .Where(dc => dc.DataCollector.Project.Id == projectId)
                .Select(r => new
                {
                    r.DataCollector.Location.X,
                    r.DataCollector.Location.Y,
                    InvalidReport = r.Report == null ? 1 : 0,
                    ValidReport = r.Report != null ? 1 : 0,
                    NoReport = 0
                });

            var locations = await dataCollectorsWithReports
                .Union(dataCollectorsWithNoReports)
                .GroupBy(x => new { x.X, x.Y })
                .Select(location => new MapOverviewLocationResponseDto
                {
                    Location = new LocationDto { Latitude = location.Key.Y, Longitude = location.Key.X },
                    CountReportingCorrectly = location.Sum(x => x.ValidReport),
                    CountReportingWithErrors = location.Sum(x => x.InvalidReport),
                    CountNotReporting = location.Sum(x => x.NoReport)
                })
                .ToListAsync();


            var result = new MapOverviewResponseDto
            {
                CenterLocation = locations.Count == 0
                    ? await GetCountryLocationFromProject(projectId)
                    : new LocationDto
                    {
                        Latitude = locations.Sum(l => l.Location.Latitude) / locations.Count,
                        Longitude = locations.Sum(l => l.Location.Longitude) / locations.Count
                    },
                DataCollectorLocations = locations
            };

            return Success(result);
        }

        public async Task<Result<List<MapOverviewDataCollectorResponseDto>>> GetMapOverviewDetails(int projectId, DateTime @from, DateTime to, double lat, double lng)
        {
            var userIdentityName = _authorizationService.GetCurrentUserName();

            var dataCollectors = _authorizationService.IsCurrentUserInRole(Role.Supervisor)
                ? _nyssContext.DataCollectors.Where(dc => dc.Supervisor.EmailAddress == userIdentityName)
                : _nyssContext.DataCollectors;

            var result = await dataCollectors
                .Where(dc => dc.Location.X == lng && dc.Location.Y == lat && dc.Name != Anonymization.Text && dc.DeletedAt == null)
                .Where(dc => dc.Project.Id == projectId)
                .Select(dc => new
                {
                    DataCollector = dc,
                    ReportsInTimeRange = dc.RawReports.Where(r => r.IsTraining.HasValue && !r.IsTraining.Value
                                                                  &&  r.ReceivedAt >= from.Date && r.ReceivedAt < to.Date.AddDays(1) )
                })
                .Select(dc => new MapOverviewDataCollectorResponseDto
                {
                    Id = dc.DataCollector.Id,
                    DisplayName = dc.DataCollector.DataCollectorType == DataCollectorType.Human ? dc.DataCollector.DisplayName : dc.DataCollector.Name,
                    Status = dc.ReportsInTimeRange.Any()
                        ? dc.ReportsInTimeRange.All(r => r.Report != null)
                            ? DataCollectorStatus.ReportingCorrectly
                            : DataCollectorStatus.ReportingWithErrors
                        : DataCollectorStatus.NotReporting
                })
                .ToListAsync();


            return Success(result);
        }

        public async Task<Result> SetTrainingState(int dataCollectorId, bool isInTraining)
        {
            var dataCollector = await _nyssContext.DataCollectors.FindAsync(dataCollectorId);

            if (dataCollector == null)
            {
                return Error(ResultKey.DataCollector.DataCollectorNotFound);
            }

            dataCollector.IsInTrainingMode = isInTraining;
            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(isInTraining ?
                ResultKey.DataCollector.SetInTrainingSuccess :
                ResultKey.DataCollector.SetOutOfTrainingSuccess);
        }

        public async Task<Result<List<DataCollectorPerformanceResponseDto>>> GetDataCollectorPerformance(int projectId)
        {
            var userIdentityName = _authorizationService.GetCurrentUserName();

            var dataCollectors = _authorizationService.IsCurrentUserInRole(Role.Supervisor)
                ? _nyssContext.DataCollectors.Where(dc => dc.Supervisor.EmailAddress == userIdentityName)
                : _nyssContext.DataCollectors;

            var to = _dateTimeProvider.UtcNow;
            var from = to.AddMonths(-2);

            var dataCollectorsWithReports = await dataCollectors
                .Where(dc => dc.Project.Id == projectId && dc.DeletedAt == null)
                .Select(dc => new
                {
                    DataCollectorName = dc.Name,
                    ReportsInTimeRange = dc.RawReports.Where(r => r.IsTraining.HasValue && !r.IsTraining.Value
                                                                                        &&  r.ReceivedAt >= from.Date && r.ReceivedAt < to.Date.AddDays(1))
                        .Select(r => new RawReportData
                        {
                            IsValid = r.ReportId.HasValue,
                            ReceivedAt = r.ReceivedAt.Date
                        })
                })
                .ToListAsync();

            var dataCollectorPerformances = dataCollectorsWithReports.Select(r => new
            {
                r.DataCollectorName,
                ReportsGroupedByWeek = r.ReportsInTimeRange.GroupBy(r => (int)(to - r.ReceivedAt).TotalDays / 7)
            })
            .Select(dc => new DataCollectorPerformanceResponseDto
            {
                Name = dc.DataCollectorName,
                DaysSinceLastReport = dc.ReportsGroupedByWeek.Any() ? (int)(to  - dc.ReportsGroupedByWeek.SelectMany(g => g).OrderByDescending(r => r.ReceivedAt).FirstOrDefault().ReceivedAt).TotalDays : -1,
                StatusLastWeek = GetDataCollectorStatus(0, dc.ReportsGroupedByWeek),
                StatusTwoWeeksAgo = GetDataCollectorStatus(1, dc.ReportsGroupedByWeek),
                StatusThreeWeeksAgo = GetDataCollectorStatus(2, dc.ReportsGroupedByWeek),
                StatusFourWeeksAgo = GetDataCollectorStatus(3, dc.ReportsGroupedByWeek),
                StatusFiveWeeksAgo = GetDataCollectorStatus(4, dc.ReportsGroupedByWeek),
                StatusSixWeeksAgo = GetDataCollectorStatus(5, dc.ReportsGroupedByWeek),
                StatusSevenWeeksAgo = GetDataCollectorStatus(6, dc.ReportsGroupedByWeek),
                StatusEightWeeksAgo = GetDataCollectorStatus(7, dc.ReportsGroupedByWeek)
            })
            .ToList();

            return Success(dataCollectorPerformances);
        }

        private DataCollectorStatus GetDataCollectorStatus(int week, IEnumerable<IGrouping<int, RawReportData>> grouping)
        {
            var reports = grouping.Where(g => g.Key == week).SelectMany(g => g);
            return reports.Any() ?
                reports.All(x => x.IsValid) ? DataCollectorStatus.ReportingCorrectly : DataCollectorStatus.ReportingWithErrors
                : DataCollectorStatus.NotReporting;
        }

        private async Task<LocationDto> GetCountryLocationFromProject(int projectId)
        {
            var countryName = _nyssContext.Projects.Where(p => p.Id == projectId)
                .Select(p => p.NationalSociety.Country.Name)
                .Single();

            var result = await _geolocationService.GetLocationFromCountry(countryName);
            return result.IsSuccess ? result.Value : null;
        }

        private class RawReportData
        {
            public bool IsValid { get; set; }
            public DateTime ReceivedAt { get; set; }
        }
    }
}
