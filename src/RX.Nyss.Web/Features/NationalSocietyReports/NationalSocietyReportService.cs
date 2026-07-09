using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Common.Extensions;
using RX.Nyss.Web.Features.NationalSocieties;
using RX.Nyss.Web.Features.NationalSocietyReports.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Features.Users;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Extensions;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocietyReports
{
    public interface INationalSocietyReportService
    {
        Task<Result<PaginatedList<NationalSocietyReportListResponseDto>>> List(int nationalSocietyId, int pageNumber, NationalSocietyReportListFilterRequestDto filter);
        Task<Result<NationalSocietyReportListFilterResponseDto>> Filters(int nationalSocietyId);
    }

    public class NationalSocietyReportService : INationalSocietyReportService
    {
        private readonly INyssWebConfig _config;
        private readonly INyssContext _nyssContext;
        private readonly IUserService _userService;
        private readonly INationalSocietyService _nationalSocietyService;
        private readonly IAuthorizationService _authorizationService;
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;

        public NationalSocietyReportService(
            INyssContext nyssContext,
            IUserService userService,
            INationalSocietyService nationalSocietyService,
            INyssWebConfig config,
            IAuthorizationService authorizationService,
            INationalSocietyStructureService nationalSocietyStructureService
            )
        {
            _nyssContext = nyssContext;
            _userService = userService;
            _nationalSocietyService = nationalSocietyService;
            _config = config;
            _authorizationService = authorizationService;
            _nationalSocietyStructureService = nationalSocietyStructureService;
        }

        public async Task<Result<PaginatedList<NationalSocietyReportListResponseDto>>> List(int nationalSocietyId, int pageNumber, NationalSocietyReportListFilterRequestDto filter)
        {
            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(_authorizationService.GetCurrentUserName());
            var rowsPerPage = _config.PaginationRowsPerPage;

            var currentUser = await _authorizationService.GetCurrentUser();

            var currentUserOrganizationId = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.NationalSocietyId == nationalSocietyId && uns.UserId == currentUser.Id)
                .Select(uns => uns.OrganizationId)
                .SingleOrDefaultAsync();

            var baseQuery = _nyssContext.RawReports
                .Where(r => r.NationalSociety.Id == nationalSocietyId)
                .Where(r => r.IsTraining == null || r.IsTraining == false)
                .FilterByDataCollectorType(filter.DataCollectorType)
                .FilterByHealthRisks(filter.HealthRisks)
                .FilterByArea(filter.Locations)
                .FilterByTrainingMode(TrainingStatusDto.Trained)
                .FilterByReportStatus(filter.ReportStatus)
                .FilterByErrorType(filter.ErrorType)
                .FilterByDate(filter.StartDate, filter.EndDate.AddDays(1));

            var result = await baseQuery.Select(r => new NationalSocietyReportListResponseDto
            {
                Id = r.Id,
                DateTime = r.ReceivedAt.AddHours(filter.UtcOffset),
                HealthRiskName = r.Report != null && r.Report.ProjectHealthRisk != null
                    ? r.Report.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode)
                        .Select(lc => lc.Name)
                        .FirstOrDefault() ?? string.Empty
                    : string.Empty,
                IsValid = r.Report != null,
                IsAnonymized = currentUser.Role != Role.Administrator
                    && r.DataCollector != null
                    && r.DataCollector.Supervisor != null
                    && !r.NationalSociety.NationalSocietyUsers.Any(
                        nsu => nsu.UserId == r.DataCollector.Supervisor.Id
                            && nsu.OrganizationId == currentUserOrganizationId),
                OrganizationName = r.DataCollector != null && r.DataCollector.Supervisor != null
                    ? r.NationalSociety.NationalSocietyUsers
                        .Where(nsu => nsu.UserId == r.DataCollector.Supervisor.Id)
                        .Select(nsu => nsu.Organization != null ? nsu.Organization.Name : null)
                        .FirstOrDefault()
                    : null,
                ProjectName = r.Report != null && r.Report.ProjectHealthRisk != null
                    ? r.Report.ProjectHealthRisk.Project.Name
                    : r.DataCollector != null
                        ? r.DataCollector.Project.Name
                        : string.Empty,
                Region = r.Village != null && r.Village.District != null && r.Village.District.Region != null
                    ? r.Village.District.Region.Name
                    : string.Empty,
                District = r.Village != null && r.Village.District != null
                    ? r.Village.District.Name
                    : string.Empty,
                Village = r.Village != null
                    ? r.Village.Name
                    : string.Empty,
                Zone = r.Zone != null
                    ? r.Zone.Name
                    : string.Empty,
                DataCollectorDisplayName = r.DataCollector != null
                    ? r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint
                        ? r.DataCollector.Name ?? string.Empty
                        : r.DataCollector.DisplayName ?? string.Empty
                    : string.Empty,
                PhoneNumber = r.Sender,
                Message = r.Text,
                IsActivityReport = r.Report != null
                    && r.Report.ProjectHealthRisk != null
                    && r.Report.ProjectHealthRisk.HealthRisk != null
                    && (r.Report.ProjectHealthRisk.HealthRisk.HealthRiskCode == 99
                        || r.Report.ProjectHealthRisk.HealthRisk.HealthRiskCode == 98),
                CountMalesBelowFive = r.Report != null && r.Report.ReportedCase != null
                    ? r.Report.ReportedCase.CountMalesBelowFive
                    : null,
                CountMalesAtLeastFive = r.Report != null && r.Report.ReportedCase != null
                    ? r.Report.ReportedCase.CountMalesAtLeastFive
                    : null,
                CountFemalesBelowFive = r.Report != null && r.Report.ReportedCase != null
                    ? r.Report.ReportedCase.CountFemalesBelowFive
                    : null,
                CountFemalesAtLeastFive = r.Report != null && r.Report.ReportedCase != null
                    ? r.Report.ReportedCase.CountFemalesAtLeastFive
                    : null,
                ReferredCount = r.Report != null && r.Report.DataCollectionPointCase != null
                    ? r.Report.DataCollectionPointCase.ReferredCount
                    : null,
                Status = r.Report != null
                    ? r.Report.Status
                    : ReportStatus.New,
                ErrorType = r.ErrorType
            })
            .OrderBy(r => r.DateTime, filter.SortAscending)
            .Page(pageNumber, rowsPerPage)
            .AsSplitQuery()
            .ToListAsync();

            foreach (var report in result)
            {
                if (report.IsAnonymized)
                {
                    report.DataCollectorDisplayName = report.OrganizationName ?? string.Empty;
                    report.PhoneNumber = AnonymizePhoneNumber(report.PhoneNumber);
                    report.Region = string.Empty;
                    report.Zone = string.Empty;
                    report.District = string.Empty;
                    report.Village = string.Empty;
                }
            }

            return Success(result.AsPaginatedList(pageNumber, await baseQuery.CountAsync(), rowsPerPage));
        }

        public async Task<Result<NationalSocietyReportListFilterResponseDto>> Filters(int nationalSocietyId)
        {
            var nationalSocietyHealthRiskNames = await _nationalSocietyService.GetHealthRiskNames(nationalSocietyId, false);
            var locations = await _nationalSocietyStructureService.Get(nationalSocietyId);

            var dto = new NationalSocietyReportListFilterResponseDto
            {
                HealthRisks = nationalSocietyHealthRiskNames
                    .Select(p => new HealthRiskDto
                    {
                        Id = p.Id,
                        Name = p.Name
                    }),
                Locations = locations
            };

            return Success(dto);
        }

        private static string AnonymizePhoneNumber(string phoneNumber) =>
            string.IsNullOrEmpty(phoneNumber)
            || phoneNumber == Anonymization.Text
            || phoneNumber.Length <= 4
                ? ""
                : $"***{phoneNumber.SubstringFromEnd(4)}";
    }
}