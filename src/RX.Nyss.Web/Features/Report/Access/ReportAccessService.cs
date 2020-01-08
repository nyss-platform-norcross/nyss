using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Report.Access
{
    public interface IReportAccessService
    {
        Task<bool> HasCurrentUserAccessToReport(int reportId);
    }

    public class ReportAccessService: IReportAccessService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly INyssContext _nyssContext;

        public ReportAccessService(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task<bool> HasCurrentUserAccessToReport(int reportId)
        {
            var user = _authorizationService.GetCurrentUser();
            var userIdentityName = user.Name;
            var isAdministrator = user.Roles.Contains(Role.Administrator.ToString());
            if (isAdministrator)
            {
                return true;
            }

            var userIsSupervisor = await _nyssContext.Users.OfType<SupervisorUser>().AnyAsync(u => u.EmailAddress == userIdentityName);
            if (userIsSupervisor)
            {
                //var reportProjectId = await _nyssContext.Reports.Select(r => r.ProjectHealthRisk.Project.Id).FirstOrDefaultAsync();
                //return await _nyssContext.SupervisorUserProjects
                //    .AnyAsync(sup => sup.SupervisorUser.EmailAddress == userIdentityName && sup.ProjectId == reportProjectId);

                var reportProjectId = _nyssContext.Reports.Select(r => r.ProjectHealthRisk.Project.Id);
                return await _nyssContext.SupervisorUserProjects
                    .AnyAsync(sup => sup.SupervisorUser.EmailAddress == userIdentityName && reportProjectId.Contains(sup.ProjectId));
            }
            else
            {
                //var reportNationalSocietyId = await _nyssContext.Reports.Select(r => r.ProjectHealthRisk.Project.NationalSocietyId).FirstOrDefaultAsync();
                //return await _nyssContext.UserNationalSocieties
                //    .AnyAsync(uns => uns.User.EmailAddress == userIdentityName && uns.NationalSocietyId == reportNationalSocietyId);

                var reportNationalSocietyId = _nyssContext.Reports.Select(r => r.ProjectHealthRisk.Project.NationalSocietyId);
                return await _nyssContext.UserNationalSocieties
                    .AnyAsync(uns => uns.User.EmailAddress == userIdentityName && reportNationalSocietyId.Contains(uns.NationalSocietyId));
            }
        }
    }

}
