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
                .Where(uns => uns.NationalSocietyId == nationalSocietyId && uns.User == currentUser)
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
                .FilterByErrorType(filter.ErrorType);

            var result = await baseQuery.Select(r => new NationalSocietyReportListResponseDto
                {
                    Id = r.Id,
                    DateTime = r.ReceivedAt.AddHours(filter.UtcOffset),
                    HealthRiskName = r.Report.ProjectHealthRisk.HealthRisk.LanguageContents
                        .Where(lc => lc.ContentLanguage.LanguageCode == userApplicationLanguageCode)
                        .Select(lc => lc.Name)
                        .SingleOrDefault(),
                    IsValid = r.Report != null,
                    IsAnonymized = currentUser.Role != Role.Administrator && !r.NationalSociety.NationalSocietyUsers.Any(
                        nsu => nsu.UserId == r.DataCollector.Supervisor.Id && nsu.OrganizationId == currentUserOrganizationId),
                    OrganizationName = r.NationalSociety.NationalSocietyUsers
                        .Where(nsu => nsu.UserId == r.DataCollector.Supervisor.Id)
                        .Select(nsu => nsu.Organization.Name)
                        .FirstOrDefault(),
                    ProjectName = r.Report != null
                        ? r.Report.ProjectHealthRisk.Project.Name
                        : r.DataCollector.Project.Name,
                    Region = r.Village.District.Region.Name,
                    District = r.Village.District.Name,
                    Village = r.Village.Name,
                    Zone = r.Zone.Name,
                    DataCollectorDisplayName = r.DataCollector.DataCollectorType == DataCollectorType.CollectionPoint
                        ? r.DataCollector.Name
                        : r.DataCollector.DisplayName,
                    PhoneNumber = r.Sender,
                    Message = r.Text,
                    CountMalesBelowFive = r.Report.ReportedCase.CountMalesBelowFive,
                    CountMalesAtLeastFive = r.Report.ReportedCase.CountMalesAtLeastFive,
                    CountFemalesBelowFive = r.Report.ReportedCase.CountFemalesBelowFive,
                    CountFemalesAtLeastFive = r.Report.ReportedCase.CountFemalesAtLeastFive,
                    ReferredCount = r.Report.DataCollectionPointCase.ReferredCount,
                    DeathCount = r.Report.DataCollectionPointCase.DeathCount,
                    FromOtherVillagesCount = r.Report.DataCollectionPointCase.FromOtherVillagesCount,
                    Status = r.Report.Status != null
                        ? r.Report.Status
                        : ReportStatus.New,
                    ErrorType = r.ErrorType
                })
                //ToDo: order base on filter.OrderBy property
                .OrderBy(r => r.DateTime, filter.SortAscending)
                .Page(pageNumber, rowsPerPage)
                .ToListAsync();

            foreach (var report in result)
            {
                if (report.IsAnonymized)
                {
                    report.DataCollectorDisplayName = report.OrganizationName;
                    report.PhoneNumber = AnonymizePhoneNumber(report.PhoneNumber);
                    report.Region = "";
                    report.Zone = "";
                    report.District = "";
                    report.Village = "";
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
