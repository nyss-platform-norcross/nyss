using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Reports.Access
{
    public interface IReportAccessService
    {
        Task<bool> HasCurrentUserAccessToReport(int reportId);
    }

    public class ReportAccessService : IReportAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public ReportAccessService(INyssContext nyssContext, IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<bool> HasCurrentUserAccessToReport(int reportId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            if (currentUser == null) return false;

            if (currentUser.Role == Role.Administrator)
            {
                return true;
            }

            var reportData = await _nyssContext.RawReports
                .Where(r => r.Id == reportId)
                .Select(r => new
                {
                    ProjectId = r.Report != null ? r.Report.ProjectHealthRisk!.Project!.Id : (int?)null,
                    SupervisorId = r.DataCollector != null && r.DataCollector.Supervisor != null
                        ? r.DataCollector.Supervisor.Id
                        : (int?)null,
                    HeadSupervisorId = r.DataCollector != null && r.DataCollector.HeadSupervisor != null
                        ? r.DataCollector.HeadSupervisor.Id
                        : (int?)null,
                    SupervisorHeadSupervisorId = r.DataCollector != null
                        && r.DataCollector.Supervisor != null
                        && r.DataCollector.Supervisor.HeadSupervisor != null
                        ? r.DataCollector.Supervisor.HeadSupervisor.Id
                        : (int?)null,
                    NationalSocietyId = r.NationalSociety!.Id,
                    IsUnknownSender = r.DataCollector == null
                })
                .FirstOrDefaultAsync();

            if (reportData == null) return false;

            if (reportData.IsUnknownSender)
            {
                return await _nyssContext.UserNationalSocieties
                    .AnyAsync(uns => uns.UserId == currentUser.Id
                        && uns.NationalSocietyId == reportData.NationalSocietyId);
            }

            var responsibleSupervisorUserId = reportData.SupervisorId ?? reportData.HeadSupervisorId;
            if (responsibleSupervisorUserId == null) return false;

            var reportOrganizationId = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.UserId == responsibleSupervisorUserId
                    && uns.NationalSocietyId == reportData.NationalSocietyId)
                .Select(uns => uns.OrganizationId)
                .FirstOrDefaultAsync();

            var currentUserOrganizationId = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.UserId == currentUser.Id
                    && uns.NationalSocietyId == reportData.NationalSocietyId)
                .Select(uns => uns.OrganizationId)
                .FirstOrDefaultAsync();

            if (reportOrganizationId != currentUserOrganizationId) return false;

            if (reportData.ProjectId == null) return false;

            if (currentUser.Role == Role.Supervisor)
            {
                return reportData.SupervisorId == currentUser.Id;
            }

            if (currentUser.Role == Role.HeadSupervisor)
            {
                return reportData.HeadSupervisorId == currentUser.Id
                    || reportData.SupervisorHeadSupervisorId == currentUser.Id;
            }

            return await _nyssContext.UserNationalSocieties
                .AnyAsync(uns => uns.UserId == currentUser.Id
                    && uns.NationalSocietyId == reportData.NationalSocietyId
                    && uns.OrganizationId == currentUserOrganizationId);
        }
    }
}