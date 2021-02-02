using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Projects.Access;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.DataCollectors.Access
{
    public interface IDataCollectorAccessService
    {
        Task<bool> HasCurrentUserAccessToDataCollector(int dataCollectorId);
        Task<bool> HasCurrentUserAccessToDataCollectors(IReadOnlyCollection<int> dataCollectorIds);
    }

    public class DataCollectorAccessService : IDataCollectorAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly IProjectAccessService _projectAccessService;

        public DataCollectorAccessService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService,
            IProjectAccessService projectAccessService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
            _projectAccessService = projectAccessService;
        }

        public Task<bool> HasCurrentUserAccessToDataCollector(int dataCollectorId) =>
            HasCurrentUserAccessToDataCollectors(new[] { dataCollectorId });

        public async Task<bool> HasCurrentUserAccessToDataCollectors(IReadOnlyCollection<int> dataCollectorIds)
        {
            if (!dataCollectorIds.Any())
            {
                return true;
            }

            var currentUser = await _authorizationService.GetCurrentUser();

            var query = _nyssContext.DataCollectors
                .Where(dc => dataCollectorIds.Contains(dc.Id))
                .Select(dc => new
                {
                    DataCollectorOrganizationId = dc.Supervisor.UserNationalSocieties
                        .Where(uns => uns.NationalSociety == dc.Project.NationalSociety)
                        .Select(uns => uns.OrganizationId)
                        .Single(),
                    CurrentUserOrganizationId = dc.Project.NationalSociety.NationalSocietyUsers
                        .Where(uns => uns.User == currentUser)
                        .Select(uns => uns.OrganizationId)
                        .Single(),
                    ProjectId = dc.Project.Id,
                    SupervisorEmailAddress = dc.Supervisor.EmailAddress,
                    HeadSupervisorEmailAddress = dc.Supervisor.HeadSupervisor.EmailAddress
                });

            if (!_authorizationService.IsCurrentUserInAnyRole(Role.Administrator))
            {
                query = query
                    .Where(p => p.CurrentUserOrganizationId == p.DataCollectorOrganizationId);
            }

            var dataCollectorData = await query
                .Distinct()
                .ToListAsync();

            var projectId = dataCollectorData.Select(d => d.ProjectId).Distinct().Single();

            return await _projectAccessService.HasCurrentUserAccessToProject(projectId)
                && VerifyAccessIfUserIsSupervisorOrHeadSupervisor(dataCollectorData.Select(dc => dc.SupervisorEmailAddress), dataCollectorData.Select(dc => dc.HeadSupervisorEmailAddress));
        }

        private bool VerifyAccessIfUserIsSupervisorOrHeadSupervisor(IEnumerable<string> supervisorEmailAddresses, IEnumerable<string> headSupervisorEmailAddresses)
        {
            var currentUserName = _authorizationService.GetCurrentUserName();

            return !_authorizationService.IsCurrentUserInAnyRole(Role.HeadSupervisor, Role.Supervisor)
                || (_authorizationService.IsCurrentUserInRole(Role.HeadSupervisor) && headSupervisorEmailAddresses.All(e => e == currentUserName))
                || (_authorizationService.IsCurrentUserInRole(Role.Supervisor) && supervisorEmailAddresses.All(e => e == currentUserName));
        }
    }
}
