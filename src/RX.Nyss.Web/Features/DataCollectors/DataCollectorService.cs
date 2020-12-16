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
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Services.Geolocation;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.DataCollectors
{
    public interface IDataCollectorService
    {
        Task<Result> Create(int projectId, CreateDataCollectorRequestDto createDto);
        Task<Result> Edit(EditDataCollectorRequestDto editDto);
        Task<Result> Delete(int dataCollectorId);
        Task<Result<GetDataCollectorResponseDto>> Get(int dataCollectorId);
        Task<Result<DataCollectorFiltersReponseDto>> GetFiltersData(int projectId);
        Task<Result<PaginatedList<DataCollectorResponseDto>>> List(int projectId, DataCollectorsFiltersRequestDto dataCollectorsFilters);
        Task<Result<List<DataCollectorResponseDto>>> ListAll(int projectId, DataCollectorsFiltersRequestDto dataCollectorsFilters);

        Task<Result<DataCollectorFormDataResponse>> GetFormData(int projectId);
        Task<Result<MapOverviewResponseDto>> MapOverview(int projectId, DateTime from, DateTime to);
        Task<Result<List<MapOverviewDataCollectorResponseDto>>> MapOverviewDetails(int projectId, DateTime from, DateTime to, double lat, double lng);
        Task<Result<DataCollectorPerformanceResponseDto>> Performance(int projectId, DataCollectorPerformanceFiltersRequestDto dataCollectorsFilters);
        Task AnonymizeDataCollectorsWithReports(int projectId);
        Task<Result> SetTrainingState(SetDataCollectorsTrainingStateRequestDto dto);
        Task<Result> ReplaceSupervisor(ReplaceSupervisorRequestDto replaceSupervisorRequestDto);
    }

    public class DataCollectorService : IDataCollectorService
    {
        private const double DefaultLatitude = 59.90822188626548; // Oslo
        private const double DefaultLongitude = 10.744628906250002;

        private readonly INyssContext _nyssContext;
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;
        private readonly IGeolocationService _geolocationService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEmailToSMSService _emailToSMSService;
        private readonly ISmsPublisherService _smsPublisherService;
        private readonly ISmsTextGeneratorService _smsTextGeneratorService;
        private readonly INyssWebConfig _config;

        public DataCollectorService(
            INyssContext nyssContext,
            INyssWebConfig config,
            INationalSocietyStructureService nationalSocietyStructureService,
            IGeolocationService geolocationService,
            IDateTimeProvider dateTimeProvider,
            IAuthorizationService authorizationService,
            IEmailToSMSService emailToSMSService,
            ISmsPublisherService smsPublisherService,
            ISmsTextGeneratorService smsTextGeneratorService)
        {
            _nyssContext = nyssContext;
            _config = config;
            _nationalSocietyStructureService = nationalSocietyStructureService;
            _geolocationService = geolocationService;
            _dateTimeProvider = dateTimeProvider;
            _authorizationService = authorizationService;
            _emailToSMSService = emailToSMSService;
            _smsPublisherService = smsPublisherService;
            _smsTextGeneratorService = smsTextGeneratorService;
        }

        public async Task<Result<GetDataCollectorResponseDto>> Get(int dataCollectorId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            var dataCollector = await _nyssContext.DataCollectors
                .Include(dc => dc.Project)
                .ThenInclude(p => p.NationalSociety)
                .Include(dc => dc.Supervisor)
                .Include(dc => dc.Zone)
                .Include(dc => dc.Village)
                .ThenInclude(v => v.District)
                .ThenInclude(d => d.Region)
                .SingleAsync(dc => dc.Id == dataCollectorId);

            var organizationId = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.UserId == currentUser.Id && uns.NationalSocietyId == dataCollector.Project.NationalSocietyId)
                .Select(uns => uns.OrganizationId)
                .SingleOrDefaultAsync();

            var regions = await _nationalSocietyStructureService.ListRegions(dataCollector.Project.NationalSociety.Id);
            var districts = await _nationalSocietyStructureService.ListDistricts(dataCollector.Village.District.Region.Id);
            var villages = await _nationalSocietyStructureService.ListVillages(dataCollector.Village.District.Id);
            var zones = await _nationalSocietyStructureService.ListZones(dataCollector.Village.Id);

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
                    Supervisors = await GetSupervisors(dataCollector.Project.Id, currentUser, organizationId)
                }
            };

            return Success(dto);
        }

        public async Task<Result<DataCollectorFormDataResponse>> GetFormData(int projectId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            var projectData = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(dc => new
                {
                    NationalSocietyId = dc.NationalSociety.Id,
                    CountryName = dc.NationalSociety.Country.Name,
                    OrganizationId = dc.NationalSociety.NationalSocietyUsers
                        .Where(nsu => nsu.UserId == currentUser.Id)
                        .Select(nsu => nsu.OrganizationId)
                        .FirstOrDefault()
                })
                .SingleAsync();

            var regions = await _nationalSocietyStructureService.ListRegions(projectData.NationalSocietyId);

            var locationFromCountry = await _geolocationService.GetLocationFromCountry(projectData.CountryName);

            var defaultSupervisorId = await _nyssContext.Users.FilterAvailable()
                .Where(u => u.Id == currentUser.Id && u.Role == Role.Supervisor)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();

            return Success(new DataCollectorFormDataResponse
            {
                NationalSocietyId = projectData.NationalSocietyId,
                Regions = regions.Value,
                Supervisors = await GetSupervisors(projectId, currentUser, projectData.OrganizationId),
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

        public async Task<Result<DataCollectorFiltersReponseDto>> GetFiltersData(int projectId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            var projectData = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(dc => new
                {
                    NationalSocietyId = dc.NationalSociety.Id,
                    OrganizationId = dc.NationalSociety.NationalSocietyUsers
                        .Where(nsu => nsu.User == currentUser)
                        .Select(nsu => nsu.OrganizationId)
                        .FirstOrDefault()
                })
                .SingleAsync();

            var filtersData = new DataCollectorFiltersReponseDto
            {
                NationalSocietyId = projectData.NationalSocietyId,
                Supervisors = await GetSupervisors(projectId, currentUser, projectData.OrganizationId)
            };

            return Success(filtersData);
        }

        public async Task<Result<PaginatedList<DataCollectorResponseDto>>> List(int projectId, DataCollectorsFiltersRequestDto dataCollectorsFilters)
        {
            var dataCollectorsQuery = (await GetDataCollectorsForCurrentUserInProject(projectId))
                .FilterOnlyNotDeleted()
                .FilterByArea(dataCollectorsFilters.Area)
                .FilterBySupervisor(dataCollectorsFilters.SupervisorId)
                .FilterBySex(dataCollectorsFilters.Sex)
                .FilterByTrainingMode(dataCollectorsFilters.TrainingStatus)
                .FilterByName(dataCollectorsFilters.Name);

            var rowsPerPage = _config.PaginationRowsPerPage;
            var totalCount = await dataCollectorsQuery.CountAsync();

            var dataCollectors = await dataCollectorsQuery
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
                    IsInTrainingMode = dc.IsInTrainingMode,
                    Supervisor = dc.Supervisor
                })
                .OrderBy(dc => dc.Name)
                .ThenBy(dc => dc.DisplayName)
                .Page(dataCollectorsFilters.PageNumber, rowsPerPage)
                .AsNoTracking()
                .ToListAsync();

            var paginatedDataCollectors = dataCollectors
                .AsPaginatedList(dataCollectorsFilters.PageNumber, totalCount, rowsPerPage);

            return Success(paginatedDataCollectors);
        }

        public async Task<Result<List<DataCollectorResponseDto>>> ListAll(int projectId, DataCollectorsFiltersRequestDto dataCollectorsFilters)
        {
            var dataCollectorsQuery = (await GetDataCollectorsForCurrentUserInProject(projectId))
                .FilterOnlyNotDeleted()
                .FilterByArea(dataCollectorsFilters.Area)
                .FilterBySupervisor(dataCollectorsFilters.SupervisorId)
                .FilterBySex(dataCollectorsFilters.Sex)
                .FilterByTrainingMode(dataCollectorsFilters.TrainingStatus)
                .FilterByName(dataCollectorsFilters.Name);

            var dataCollectors = await dataCollectorsQuery
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
                    IsInTrainingMode = dc.IsInTrainingMode,
                    Supervisor = dc.Supervisor
                })
                .OrderBy(dc => dc.Name)
                .ThenBy(dc => dc.DisplayName)
                .ToListAsync();

            return Success(dataCollectors);
        }

        public async Task<Result> Create(int projectId, CreateDataCollectorRequestDto createDto)
        {
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

        public async Task<Result> Edit(EditDataCollectorRequestDto editDto)
        {
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

        public async Task<Result> Delete(int dataCollectorId)
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
                    await Anonymize(dataCollectorId);
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

        public async Task AnonymizeDataCollectorsWithReports(int projectId)
        {
            await _nyssContext.DataCollectors
                .Where(x => x.Project.Id == projectId && x.RawReports.Any())
                .BatchUpdateAsync(x => new DataCollector
                {
                    Name = Anonymization.Text,
                    DisplayName = Anonymization.Text,
                    PhoneNumber = Anonymization.Text,
                    AdditionalPhoneNumber = Anonymization.Text,
                    DeletedAt = DateTime.UtcNow
                });

            await _nyssContext.RawReports
                .Where(rawReport => rawReport.DataCollector.Project.Id == projectId)
                .BatchUpdateAsync(x => new RawReport { Sender = Anonymization.Text });

            await _nyssContext.Reports
                .Where(report => report.ProjectHealthRisk.Project.Id == projectId)
                .BatchUpdateAsync(x => new Report { PhoneNumber = Anonymization.Text });
        }

        public async Task<Result<MapOverviewResponseDto>> MapOverview(int projectId, DateTime from, DateTime to)
        {
            var dataCollectors = await GetDataCollectorsForCurrentUserInProject(projectId);
            var endDate = to.Date.AddDays(1);

            var dataCollectorsWithNoReports = dataCollectors
                .Where(dc => dc.CreatedAt < endDate && dc.DeletedAt == null)
                .Where(dc => !dc.RawReports.Any(r => r.IsTraining.HasValue && !r.IsTraining.Value
                    && r.ReceivedAt >= from.Date && r.ReceivedAt < endDate))
                .Select(dc => new
                {
                    dc.Location.X,
                    dc.Location.Y,
                    InvalidReport = 0,
                    ValidReport = 0,
                    NoReport = 1
                });

            var dataCollectorsWithReports = dataCollectors
                .Where(dc => dc.CreatedAt < endDate && dc.Name != Anonymization.Text && dc.DeletedAt == null)
                .Where(dc => dc.RawReports.Any(r => r.IsTraining.HasValue && !r.IsTraining.Value
                    && r.ReceivedAt >= from.Date && r.ReceivedAt < endDate))
                .Where(dc => dc.Project.Id == projectId)
                .Select(r => new
                {
                    r.Location.X,
                    r.Location.Y,
                    InvalidReport = r.RawReports
                        .Count(rr => !rr.ReportId.HasValue && rr.IsTraining.HasValue && !rr.IsTraining.Value
                            && rr.ReceivedAt >= from.Date && rr.ReceivedAt < endDate),
                    ValidReport = r.RawReports
                        .Count(rr => rr.ReportId.HasValue && rr.IsTraining.HasValue && !rr.IsTraining.Value
                            && rr.ReceivedAt >= from.Date && rr.ReceivedAt < endDate),
                    NoReport = 0
                });

            var locations = await dataCollectorsWithReports
                .Union(dataCollectorsWithNoReports)
                .GroupBy(x => new
                {
                    x.X,
                    x.Y
                })
                .Select(location => new MapOverviewLocationResponseDto
                {
                    Location = new LocationDto
                    {
                        Latitude = location.Key.Y,
                        Longitude = location.Key.X
                    },
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

        public async Task<Result<List<MapOverviewDataCollectorResponseDto>>> MapOverviewDetails(int projectId, DateTime from, DateTime to, double lat, double lng)
        {
            var dataCollectors = await GetDataCollectorsForCurrentUserInProject(projectId);

            var result = await dataCollectors
                .Where(dc => dc.Location.X == lng && dc.Location.Y == lat && dc.DeletedAt == null)
                .Select(dc => new
                {
                    DataCollector = dc,
                    ReportsInTimeRange = dc.RawReports.Where(r => r.IsTraining.HasValue && !r.IsTraining.Value
                        && r.ReceivedAt >= from.Date && r.ReceivedAt < to.Date.AddDays(1))
                })
                .Select(dc => new MapOverviewDataCollectorResponseDto
                {
                    Id = dc.DataCollector.Id,
                    DisplayName = dc.DataCollector.DataCollectorType == DataCollectorType.Human
                        ? $"{dc.DataCollector.DisplayName}: {dc.DataCollector.PhoneNumber}"
                        : $"{dc.DataCollector.Name}: {dc.DataCollector.PhoneNumber}",
                    Status = dc.ReportsInTimeRange.Any()
                        ? dc.ReportsInTimeRange.All(r => r.Report != null) ? ReportingStatus.ReportingCorrectly : ReportingStatus.ReportingWithErrors
                        : ReportingStatus.NotReporting
                })
                .ToListAsync();

            return Success(result);
        }

        public async Task<Result> SetTrainingState(SetDataCollectorsTrainingStateRequestDto dto)
        {
            var dataCollectors = await _nyssContext.DataCollectors
                .Where(dc => dto.DataCollectorIds.Contains(dc.Id))
                .ToListAsync();

            dataCollectors.ForEach(dc => dc.IsInTrainingMode = dto.InTraining);
            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(dto.InTraining
                ? ResultKey.DataCollector.SetInTrainingSuccess
                : ResultKey.DataCollector.SetOutOfTrainingSuccess);
        }

        public async Task<Result<DataCollectorPerformanceResponseDto>> Performance(int projectId, DataCollectorPerformanceFiltersRequestDto dataCollectorsFilters)
        {
            var dataCollectors = (await GetDataCollectorsForCurrentUserInProject(projectId))
                .FilterOnlyNotDeleted()
                .FilterByArea(dataCollectorsFilters.Area)
                .FilterByName(dataCollectorsFilters.Name)
                .FilterBySupervisor(dataCollectorsFilters.SupervisorId)
                .Include(dc => dc.Village);

            var toDate = _dateTimeProvider.UtcNow;
            var fromDate = toDate.AddMonths(-2);
            var rowsPerPage = _config.PaginationRowsPerPage;
            var totalRows = await dataCollectors.CountAsync();

            var dataCollectorsWithReportsData = await dataCollectors
                .Select(dc => new DataCollectorWithRawReportData
                {
                    Name = dc.Name,
                    PhoneNumber = dc.PhoneNumber,
                    VillageName = dc.Village.Name,
                    ReportsInTimeRange = dc.RawReports.Where(r => r.IsTraining.HasValue && !r.IsTraining.Value
                            && r.ReceivedAt >= fromDate.Date && r.ReceivedAt < toDate.Date.AddDays(1))
                        .Select(r => new RawReportData
                        {
                            IsValid = r.ReportId.HasValue,
                            ReceivedAt = r.ReceivedAt.Date
                        })
                }).ToListAsync();

            var dataCollectorCompleteness = GetDataCollectorCompleteness(dataCollectorsFilters, dataCollectorsWithReportsData, totalRows, toDate);

            var paginatedDataCollectorsWithReportsData = dataCollectorsWithReportsData
                .Page(dataCollectorsFilters.PageNumber, rowsPerPage);

            var dataCollectorPerformances = paginatedDataCollectorsWithReportsData.Select(r => new
                {
                    r.Name,
                    r.PhoneNumber,
                    r.VillageName,
                    ReportsGroupedByWeek = r.ReportsInTimeRange.GroupBy(report => (int)(toDate - report.ReceivedAt).TotalDays / 7)
                })
                .Select(dc => new DataCollectorPerformance
                {
                    Name = dc.Name,
                    PhoneNumber = dc.PhoneNumber,
                    VillageName = dc.VillageName,
                    DaysSinceLastReport = dc.ReportsGroupedByWeek.Any()
                        ? (int)(toDate - dc.ReportsGroupedByWeek
                            .SelectMany(g => g)
                            .OrderByDescending(r => r.ReceivedAt)
                            .First().ReceivedAt).TotalDays
                        : -1,
                    StatusLastWeek = GetDataCollectorStatus(0, dc.ReportsGroupedByWeek),
                    StatusTwoWeeksAgo = GetDataCollectorStatus(1, dc.ReportsGroupedByWeek),
                    StatusThreeWeeksAgo = GetDataCollectorStatus(2, dc.ReportsGroupedByWeek),
                    StatusFourWeeksAgo = GetDataCollectorStatus(3, dc.ReportsGroupedByWeek),
                    StatusFiveWeeksAgo = GetDataCollectorStatus(4, dc.ReportsGroupedByWeek),
                    StatusSixWeeksAgo = GetDataCollectorStatus(5, dc.ReportsGroupedByWeek),
                    StatusSevenWeeksAgo = GetDataCollectorStatus(6, dc.ReportsGroupedByWeek),
                    StatusEightWeeksAgo = GetDataCollectorStatus(7, dc.ReportsGroupedByWeek)
                })
                .FilterByStatusLastWeek(dataCollectorsFilters.LastWeek)
                .FilterByStatusTwoWeeksAgo(dataCollectorsFilters.TwoWeeksAgo)
                .FilterByStatusThreeWeeksAgo(dataCollectorsFilters.ThreeWeeksAgo)
                .FilterByStatusFourWeeksAgo(dataCollectorsFilters.FourWeeksAgo)
                .FilterByStatusFiveWeeksAgo(dataCollectorsFilters.FiveWeeksAgo)
                .FilterByStatusSixWeeksAgo(dataCollectorsFilters.SixWeeksAgo)
                .FilterByStatusSevenWeeksAgo(dataCollectorsFilters.SevenWeeksAgo)
                .FilterByStatusEightWeeksAgo(dataCollectorsFilters.EightWeeksAgo)
                .AsPaginatedList(dataCollectorsFilters.PageNumber, totalRows, rowsPerPage);

            var dataCollectorPerformanceDto = new DataCollectorPerformanceResponseDto
            {
                Completeness = dataCollectorCompleteness,
                Performance = dataCollectorPerformances
            };

            return Success(dataCollectorPerformanceDto);
        }

        public async Task<Result> ReplaceSupervisor(ReplaceSupervisorRequestDto replaceSupervisorRequestDto)
        {
            var dataCollectors = await _nyssContext.DataCollectors
                .Where(dc => replaceSupervisorRequestDto.DataCollectorIds.Contains(dc.Id))
                .ToListAsync();

            var supervisorData = await _nyssContext.Users
                .Select(u => new
                {
                    Supervisor = (SupervisorUser)u,
                    NationalSociety = u.UserNationalSocieties.Select(uns => uns.NationalSociety).Single()
                })
                .FirstOrDefaultAsync(u => u.Supervisor.Id == replaceSupervisorRequestDto.SupervisorId);

            var gatewaySetting = await _nyssContext.GatewaySettings
                .Include(gs => gs.NationalSociety)
                .ThenInclude(ns => ns.ContentLanguage)
                .FirstOrDefaultAsync(gs => gs.NationalSociety == supervisorData.NationalSociety);

            foreach (var dc in dataCollectors)
            {
                dc.Supervisor = supervisorData.Supervisor;
            }

            await _nyssContext.SaveChangesAsync();

            await SendReplaceSupervisorSms(gatewaySetting, dataCollectors, supervisorData.Supervisor);

            return Success();
        }

        private DataCollectorCompleteness GetDataCollectorCompleteness(DataCollectorPerformanceFiltersRequestDto filters, IEnumerable<DataCollectorWithRawReportData> dataCollectors, int totalDataCollectors, DateTime toDate)
        {
            if (IsWeekFiltersActive(filters) || totalDataCollectors == 0)
            {
                return null;
            }

            var dataCollectorCompleteness = dataCollectors
                .Select(dc => new
                {
                    ReportsGroupedByWeek = dc.ReportsInTimeRange.GroupBy(report => (int)(toDate - report.ReceivedAt).TotalDays / 7)
                }).Select(dc => new
                {
                    HasReportedLastWeek = dc.ReportsGroupedByWeek.Where(g => g.Key == 0).SelectMany(g => g).Any(),
                    HasReportedTwoWeeksAgo = dc.ReportsGroupedByWeek.Where(g => g.Key == 1).SelectMany(g => g).Any(),
                    HasReportedThreeWeeksAgo = dc.ReportsGroupedByWeek.Where(g => g.Key == 2).SelectMany(g => g).Any(),
                    HasReportedFourWeeksAgo = dc.ReportsGroupedByWeek.Where(g => g.Key == 3).SelectMany(g => g).Any(),
                    HasReportedFiveWeeksAgo = dc.ReportsGroupedByWeek.Where(g => g.Key == 4).SelectMany(g => g).Any(),
                    HasReportedSixWeeksAgo = dc.ReportsGroupedByWeek.Where(g => g.Key == 5).SelectMany(g => g).Any(),
                    HasReportedSevenWeeksAgo = dc.ReportsGroupedByWeek.Where(g => g.Key == 6).SelectMany(g => g).Any(),
                    HasReportedEightWeeksAgo = dc.ReportsGroupedByWeek.Where(g => g.Key == 7).SelectMany(g => g).Any()
                }).Aggregate(new
                {
                    ActiveLastWeek = 0,
                    ActiveTwoWeeksAgo = 0,
                    ActiveThreeWeeksAgo = 0,
                    ActiveFourWeeksAgo = 0,
                    ActiveFiveWeeksAgo = 0,
                    ActiveSixWeeksAgo = 0,
                    ActiveSevenWeeksAgo = 0,
                    ActiveEightWeeksAgo = 0
                }, (a, dc) => new
                {
                    ActiveLastWeek = a.ActiveLastWeek + (dc.HasReportedLastWeek ? 1 : 0),
                    ActiveTwoWeeksAgo = a.ActiveTwoWeeksAgo + (dc.HasReportedTwoWeeksAgo ? 1 : 0),
                    ActiveThreeWeeksAgo = a.ActiveThreeWeeksAgo + (dc.HasReportedThreeWeeksAgo ? 1 : 0),
                    ActiveFourWeeksAgo = a.ActiveFourWeeksAgo + (dc.HasReportedFourWeeksAgo ? 1 : 0),
                    ActiveFiveWeeksAgo = a.ActiveFiveWeeksAgo + (dc.HasReportedFiveWeeksAgo ? 1 : 0),
                    ActiveSixWeeksAgo = a.ActiveSixWeeksAgo + (dc.HasReportedSixWeeksAgo ? 1 : 0),
                    ActiveSevenWeeksAgo = a.ActiveSevenWeeksAgo + (dc.HasReportedSevenWeeksAgo ? 1 : 0),
                    ActiveEightWeeksAgo = a.ActiveEightWeeksAgo + (dc.HasReportedEightWeeksAgo ? 1 : 0)
                });

            return new DataCollectorCompleteness
            {
                LastWeek = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveLastWeek,
                    Percentage = (dataCollectorCompleteness.ActiveLastWeek * 100) / totalDataCollectors
                },
                TwoWeeksAgo = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveTwoWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveTwoWeeksAgo * 100) / totalDataCollectors
                },
                ThreeWeeksAgo = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveThreeWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveThreeWeeksAgo * 100) / totalDataCollectors
                },
                FourWeeksAgo = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveFourWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveFourWeeksAgo * 100) / totalDataCollectors
                },
                FiveWeeksAgo = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveFiveWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveFiveWeeksAgo * 100) / totalDataCollectors
                },
                SixWeeksAgo = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveSixWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveSixWeeksAgo * 100) / totalDataCollectors
                },
                SevenWeeksAgo = new Completeness
                {

                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveSevenWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveSevenWeeksAgo * 100) / totalDataCollectors
                },
                EightWeeksAgo = new Completeness
                {
                    TotalDataCollectors = totalDataCollectors,
                    ActiveDataCollectors = dataCollectorCompleteness.ActiveEightWeeksAgo,
                    Percentage = (dataCollectorCompleteness.ActiveEightWeeksAgo * 100) / totalDataCollectors
                }
            };
        }

        private async Task<IQueryable<DataCollector>> GetDataCollectorsForCurrentUserInProject(int projectId)
        {
            var currentUserEmail = _authorizationService.GetCurrentUserName();
            var projectData = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => new
                {
                    CurrentUserOrganization = p.NationalSociety.NationalSocietyUsers
                        .Where(uns => uns.User.EmailAddress == currentUserEmail)
                        .Select(uns => uns.Organization)
                        .SingleOrDefault(),
                    HasCoordinator = p.NationalSociety.NationalSocietyUsers
                        .Any(uns => uns.User.Role == Role.Coordinator)
                })
                .SingleAsync();

            var dataCollectorsQuery = _nyssContext.DataCollectors.FilterByProject(projectId);

            if (_authorizationService.IsCurrentUserInRole(Role.Supervisor))
            {
                dataCollectorsQuery = dataCollectorsQuery.Where(dc => dc.Supervisor.EmailAddress == currentUserEmail);
            }

            if (projectData.HasCoordinator && !_authorizationService.IsCurrentUserInAnyRole(Role.Administrator))
            {
                dataCollectorsQuery = dataCollectorsQuery.FilterByOrganization(projectData.CurrentUserOrganization);
            }

            return dataCollectorsQuery;
        }

        private async Task Anonymize(int dataCollectorId)
        {
            await _nyssContext.DataCollectors
                .Where(x => x.Id == dataCollectorId)
                .BatchUpdateAsync(x => new DataCollector
                {
                    Name = Anonymization.Text,
                    DisplayName = Anonymization.Text,
                    PhoneNumber = Anonymization.Text,
                    AdditionalPhoneNumber = Anonymization.Text,
                    DeletedAt = DateTime.UtcNow
                });

            await _nyssContext.RawReports
                .Where(rawReport => rawReport.DataCollector.Id == dataCollectorId)
                .BatchUpdateAsync(x => new RawReport { Sender = Anonymization.Text });

            await _nyssContext.Reports
                .Where(report => report.DataCollector.Id == dataCollectorId)
                .BatchUpdateAsync(x => new Report { PhoneNumber = Anonymization.Text });
        }

        private async Task<List<DataCollectorSupervisorResponseDto>> GetSupervisors(int projectId, User currentUser, int? organizationId) =>
            await _nyssContext.SupervisorUserProjects
                .FilterAvailableUsers()
                .Where(sup => sup.ProjectId == projectId
                    && (currentUser.Role == Role.Administrator || sup.Project.NationalSociety.NationalSocietyUsers.Single(nsu => nsu.UserId == sup.SupervisorUserId).OrganizationId == organizationId))
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

        private ReportingStatus GetDataCollectorStatus(int week, IEnumerable<IGrouping<int, RawReportData>> grouping)
        {
            var reports = grouping.Where(g => g.Key == week).SelectMany(g => g);
            return reports.Any()
                ? reports.All(x => x.IsValid) ? ReportingStatus.ReportingCorrectly : ReportingStatus.ReportingWithErrors
                : ReportingStatus.NotReporting;
        }

        private bool IsWeekFiltersActive(DataCollectorPerformanceFiltersRequestDto filters) =>
            IsReportingStatusFilterActiveForWeek(filters.LastWeek)
            || IsReportingStatusFilterActiveForWeek(filters.TwoWeeksAgo)
            || IsReportingStatusFilterActiveForWeek(filters.ThreeWeeksAgo)
            || IsReportingStatusFilterActiveForWeek(filters.FourWeeksAgo)
            || IsReportingStatusFilterActiveForWeek(filters.FiveWeeksAgo)
            || IsReportingStatusFilterActiveForWeek(filters.SixWeeksAgo)
            || IsReportingStatusFilterActiveForWeek(filters.SevenWeeksAgo)
            || IsReportingStatusFilterActiveForWeek(filters.EightWeeksAgo);

        private bool IsReportingStatusFilterActiveForWeek(PerformanceStatusFilterDto weekFilter) =>
            !weekFilter.NotReporting
            || !weekFilter.ReportingCorrectly
            || !weekFilter.ReportingWithErrors;

        private async Task<LocationDto> GetCountryLocationFromProject(int projectId)
        {
            var countryName = _nyssContext.Projects.Where(p => p.Id == projectId)
                .Select(p => p.NationalSociety.Country.Name)
                .Single();

            var result = await _geolocationService.GetLocationFromCountry(countryName);
            return result.IsSuccess
                ? result.Value
                : null;
        }

        private async Task SendReplaceSupervisorSms(GatewaySetting gatewaySetting, List<DataCollector> dataCollectors, SupervisorUser newSupervisor)
        {
            var phoneNumbers = dataCollectors.Select(dc => dc.PhoneNumber).ToList();
            var message = await _smsTextGeneratorService.GenerateReplaceSupervisorSms(gatewaySetting.NationalSociety.ContentLanguage.LanguageCode);

            message = message.Replace("{{supervisorName}}", newSupervisor.Name);
            message = message.Replace("{{phoneNumber}}", newSupervisor.PhoneNumber);

            if (string.IsNullOrEmpty(gatewaySetting.IotHubDeviceName))
            {
                await _emailToSMSService.SendMessage(gatewaySetting, phoneNumbers, message);
            }
            else
            {
                await _smsPublisherService.SendSms(gatewaySetting.IotHubDeviceName, phoneNumbers, message);
            }
        }

        private class RawReportData
        {
            public bool IsValid { get; set; }
            public DateTime ReceivedAt { get; set; }
        }

        private class DataCollectorWithRawReportData
        {
            public string Name { get; set; }
            public string VillageName { get; set; }
            public string PhoneNumber { get; set; }
            public IEnumerable<RawReportData> ReportsInTimeRange { get; set; }
        }
    }
}
