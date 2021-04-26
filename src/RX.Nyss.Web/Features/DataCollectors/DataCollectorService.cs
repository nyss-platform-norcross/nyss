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
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.DataCollectors.DataContracts;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Features.NationalSocietyStructure.Dto;
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
        Task AnonymizeDataCollectorsWithReports(int projectId);
        Task<Result> SetTrainingState(SetDataCollectorsTrainingStateRequestDto dto);
        Task<Result> ReplaceSupervisor(ReplaceSupervisorRequestDto replaceSupervisorRequestDto);
        Task<IQueryable<DataCollector>> GetDataCollectorsForCurrentUserInProject(int projectId);
        Task<Result> SetDeployedState(SetDeployedStateRequestDto dto);
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
                .Include(dc => dc.DataCollectorLocations)
                .Select(dc => new GetDataCollectorResponseDto
                {
                    Id = dc.Id,
                    Name = dc.Name,
                    DisplayName = dc.DisplayName,
                    DataCollectorType = dc.DataCollectorType,
                    Sex = dc.Sex,
                    BirthGroupDecade = dc.BirthGroupDecade,
                    PhoneNumber = dc.PhoneNumber,
                    AdditionalPhoneNumber = dc.AdditionalPhoneNumber,
                    Locations = dc.DataCollectorLocations.Select(dcl => new GetDataCollectorResponseDto.DataCollectorLocationDto
                    {
                        Id = dcl.Id,
                        Latitude = dcl.Location.Y,
                        Longitude = dcl.Location.X,
                        RegionId = dcl.Village.District.Region.Id,
                        DistrictId = dcl.Village.District.Id,
                        VillageId = dcl.Village.Id,
                        ZoneId = dcl.Zone.Id,
                        InitialFormData = new GetDataCollectorResponseDto.DataCollectorLocationFormDataDto
                        {
                            Districts = _nyssContext.Districts
                                .Where(d => d.Region.Id == dcl.Village.District.Region.Id)
                                .Select(d => new DistrictResponseDto
                                {
                                    Id = d.Id,
                                    Name = d.Name
                                }),
                            Villages = _nyssContext.Villages
                                .Where(v => v.District.Id == dcl.Village.District.Id)
                                .Select(v => new VillageResponseDto
                                {
                                    Id = v.Id,
                                    Name = v.Name
                                }),
                            Zones = _nyssContext.Zones
                                .Where(z => z.Village.Id == dcl.Village.Id)
                                .Select(z => new ZoneResponseDto
                                {
                                    Id = z.Id,
                                    Name = z.Name
                                })
                        }
                    }),
                    SupervisorId = dc.Supervisor.Id,
                    NationalSocietyId = dc.Project.NationalSociety.Id,
                    ProjectId = dc.Project.Id,
                    Deployed = dc.Deployed
                })
                .SingleAsync(dc => dc.Id == dataCollectorId);

            var organizationId = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.UserId == currentUser.Id && uns.NationalSocietyId == dataCollector.NationalSocietyId)
                .Select(uns => uns.OrganizationId)
                .SingleOrDefaultAsync();

            var regions = await _nationalSocietyStructureService.ListRegions(dataCollector.NationalSocietyId);

            dataCollector.FormData = new GetDataCollectorResponseDto.FormDataDto
            {
                Regions = regions.Value,
                Supervisors = await GetSupervisors(dataCollector.ProjectId, currentUser, organizationId)
            };

            return Success(dataCollector);
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
                .FilterByDeployedMode(dataCollectorsFilters.DeployedMode)
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
                    Sex = dc.Sex,
                    IsInTrainingMode = dc.IsInTrainingMode,
                    Supervisor = dc.Supervisor,
                    IsDeployed = dc.Deployed,
                    Locations = dc.DataCollectorLocations
                        .Select(dcl => new DataCollectorLocationResponseDto
                        {
                            Region = dcl.Village.District.Region.Name,
                            District = dcl.Village.District.Name,
                            Village = dcl.Village.Name
                        })
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
                .FilterByDeployedMode(dataCollectorsFilters.DeployedMode)
                .FilterByName(dataCollectorsFilters.Name);

            var dataCollectors = await dataCollectorsQuery
                .Select(dc => new DataCollectorResponseDto
                {
                    Id = dc.Id,
                    DataCollectorType = dc.DataCollectorType,
                    Name = dc.Name,
                    DisplayName = dc.DisplayName,
                    PhoneNumber = dc.PhoneNumber,
                    Sex = dc.Sex,
                    IsInTrainingMode = dc.IsInTrainingMode,
                    Supervisor = dc.Supervisor,
                    IsDeployed = dc.Deployed,
                    Locations = dc.DataCollectorLocations
                        .Select(dcl => new DataCollectorLocationResponseDto
                        {
                            Region = dcl.Village.District.Region.Name,
                            District = dcl.Village.District.Name,
                            Village = dcl.Village.Name
                        })
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

            var dataCollector = new DataCollector
            {
                Name = createDto.Name,
                DisplayName = createDto.DisplayName,
                PhoneNumber = createDto.PhoneNumber,
                AdditionalPhoneNumber = createDto.AdditionalPhoneNumber,
                BirthGroupDecade = createDto.BirthGroupDecade,
                Sex = createDto.Sex,
                DataCollectorType = createDto.DataCollectorType,
                Supervisor = supervisor,
                Project = project,
                CreatedAt = _dateTimeProvider.UtcNow,
                IsInTrainingMode = true,
                Deployed = createDto.Deployed,
                DataCollectorLocations = createDto.Locations.Select(l => new DataCollectorLocation
                {
                    Location = CreatePoint(l.Latitude, l.Longitude),
                    Village = _nyssContext.Villages
                        .Single(v => v.Id == l.VillageId && v.District.Region.NationalSociety.Id == nationalSocietyId),
                    Zone = l.ZoneId != null
                        ? _nyssContext.Zones.Single(z => z.Id == l.ZoneId.Value)
                        : null
                }).ToList()
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
                .Include(dc => dc.DataCollectorLocations)
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

            dataCollector.Name = editDto.Name;
            dataCollector.DisplayName = editDto.DisplayName;
            dataCollector.PhoneNumber = editDto.PhoneNumber;
            dataCollector.AdditionalPhoneNumber = editDto.AdditionalPhoneNumber;
            dataCollector.BirthGroupDecade = editDto.BirthGroupDecade;
            dataCollector.Sex = editDto.Sex;
            dataCollector.Supervisor = supervisor;
            dataCollector.Deployed = editDto.Deployed;

            dataCollector.DataCollectorLocations = await EditDataCollectorLocations(dataCollector.DataCollectorLocations, editDto.Locations, nationalSocietyId);

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
            var endDate = to.Date.AddDays(1);
            var dataCollectors = (await GetDataCollectorsForCurrentUserInProject(projectId))
                .Where(dc => dc.CreatedAt < endDate && dc.Name != Anonymization.Text && dc.DeletedAt == null && dc.Deployed);

            var dataCollectorsWithReports = dataCollectors
                .Select(r => new
                {
                    r.DataCollectorLocations.First().Location.X,
                    r.DataCollectorLocations.First().Location.Y,
                    InvalidReports = r.RawReports
                        .Count(rr => !rr.ReportId.HasValue && rr.IsTraining.HasValue && !rr.IsTraining.Value
                            && rr.ReceivedAt >= from.Date && rr.ReceivedAt < endDate),
                    ValidReports = r.RawReports
                        .Count(rr => rr.ReportId.HasValue && rr.IsTraining.HasValue && !rr.IsTraining.Value
                            && rr.ReceivedAt >= from.Date && rr.ReceivedAt < endDate)
                });

            var locations = await dataCollectorsWithReports
                .Select(dc => new MapOverviewLocationResponseDto
                {
                    Location = new LocationDto
                    {
                        Latitude = dc.Y,
                        Longitude = dc.X
                    },
                    CountReportingCorrectly = dc.ValidReports,
                    CountReportingWithErrors = dc.InvalidReports
                })
                .ToListAsync();

            locations.ForEach(dc => dc.CountNotReporting = dc.CountReportingCorrectly == 0 && dc.CountReportingWithErrors == 0
                ? 1
                : 0);

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
                .Where(dc => dc.DataCollectorLocations.Any(dcl => dcl.Location.X == lng && dcl.Location.Y == lat) && dc.DeletedAt == null && dc.Deployed)
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

        public async Task<Result> ReplaceSupervisor(ReplaceSupervisorRequestDto replaceSupervisorRequestDto)
        {
            var replaceSupervisorDatas = await _nyssContext.DataCollectors
                .Where(dc => replaceSupervisorRequestDto.DataCollectorIds.Contains(dc.Id))
                .Select(dc => new ReplaceSupervisorData
                {
                    DataCollector = dc,
                    Supervisor = dc.Supervisor,
                    LastReport = dc.RawReports.FirstOrDefault(r => r.ModemNumber.HasValue)
                })
                .ToListAsync();

            var supervisorData = await _nyssContext.Users
                .Select(u => new
                {
                    Supervisor = (SupervisorUser)u,
                    NationalSociety = u.UserNationalSocieties.Select(uns => uns.NationalSociety).Single()
                })
                .FirstOrDefaultAsync(u => u.Supervisor.Id == replaceSupervisorRequestDto.SupervisorId);

            var gatewaySetting = await _nyssContext.GatewaySettings
                .Include(gs => gs.Modems)
                .Include(gs => gs.NationalSociety)
                .ThenInclude(ns => ns.ContentLanguage)
                .FirstOrDefaultAsync(gs => gs.NationalSociety == supervisorData.NationalSociety);

            foreach (var dc in replaceSupervisorDatas)
            {
                dc.DataCollector.Supervisor = supervisorData.Supervisor;
            }

            await _nyssContext.SaveChangesAsync();

            await SendReplaceSupervisorSms(gatewaySetting, replaceSupervisorDatas, supervisorData.Supervisor);

            return Success();
        }

        public async Task<Result> SetDeployedState(SetDeployedStateRequestDto dto)
        {
            var dataCollectors = await _nyssContext.DataCollectors
                .Where(dc => dto.DataCollectorIds.Contains(dc.Id))
                .ToListAsync();

            dataCollectors.ForEach(dc => dc.Deployed = dto.Deployed);

            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(dto.Deployed
                ? ResultKey.DataCollector.SetToDeployedSuccess
                : ResultKey.DataCollector.SetToNotDeployedSuccess);
        }

        public async Task<IQueryable<DataCollector>> GetDataCollectorsForCurrentUserInProject(int projectId)
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

            if (_authorizationService.IsCurrentUserInRole(Role.HeadSupervisor))
            {
                dataCollectorsQuery = dataCollectorsQuery.Where(dc => dc.Supervisor.HeadSupervisor.EmailAddress == currentUserEmail);
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
                .FilterByProject(projectId)
                .FilterByCurrentUserRole(currentUser, organizationId)
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

        private async Task SendReplaceSupervisorSms(GatewaySetting gatewaySetting, List<ReplaceSupervisorData> replaceSupervisorDatas, SupervisorUser newSupervisor)
        {
            var previousSupervisor = replaceSupervisorDatas.Select(r => r.Supervisor).Distinct().Single();
            var recipients = replaceSupervisorDatas.Select(r => new SendSmsRecipient
            {
                PhoneNumber = r.DataCollector.PhoneNumber,
                Modem = r.LastReport != null ? r.LastReport.ModemNumber : previousSupervisor.ModemId
            }).ToList();
            var message = await _smsTextGeneratorService.GenerateReplaceSupervisorSms(gatewaySetting.NationalSociety.ContentLanguage.LanguageCode);

            message = message.Replace("{{supervisorName}}", newSupervisor.Name);
            message = message.Replace("{{phoneNumber}}", newSupervisor.PhoneNumber);

            if (string.IsNullOrEmpty(gatewaySetting.IotHubDeviceName))
            {
                await _emailToSMSService.SendMessage(gatewaySetting, recipients.Select(r => r.PhoneNumber).ToList(), message);
            }
            else
            {
                await _smsPublisherService.SendSms(gatewaySetting.IotHubDeviceName, recipients, message, gatewaySetting.Modems.Any());
            }
        }

        private async Task<List<DataCollectorLocation>> EditDataCollectorLocations(ICollection<DataCollectorLocation> dataCollectorLocations, IEnumerable<DataCollectorLocationRequestDto> dataCollectorLocationRequestDtos, int nationalSocietyId)
        {
            var newDataCollectorLocations = new List<DataCollectorLocation>();
            foreach (var dto in dataCollectorLocationRequestDtos)
            {
                var dcl = dto.Id.HasValue
                    ? dataCollectorLocations.First(l => l.Id == dto.Id)
                    : new DataCollectorLocation();

                dcl.Location = CreatePoint(dto.Latitude, dto.Longitude);
                dcl.Village = await _nyssContext.Villages.SingleAsync(v => v.Id == dto.VillageId && v.District.Region.NationalSociety.Id == nationalSocietyId);
                dcl.Zone = await _nyssContext.Zones.SingleOrDefaultAsync(z => z.Id == dto.ZoneId);

                newDataCollectorLocations.Add(dcl);
            }

            return newDataCollectorLocations;
        }
    }
}
