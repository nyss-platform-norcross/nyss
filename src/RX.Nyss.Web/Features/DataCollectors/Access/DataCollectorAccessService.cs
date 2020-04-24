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
        Task<bool> HasCurrentUserAccessToDataCollectors(IEnumerable<int> dataCollectorIds);
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

        public async Task<bool> HasCurrentUserAccessToDataCollectors(IEnumerable<int> dataCollectorIds)
        {
            var dataCollectorData = await _nyssContext.DataCollectors
                .Where(dc => dataCollectorIds.Contains(dc.Id))
                .Select(dc => new
                {
                    ProjectId = dc.Project.Id,
                    SupervisorEmailAddress = dc.Supervisor.EmailAddress
                })
                .Distinct()
                .ToListAsync();

            var projectId = dataCollectorData.Select(d => d.ProjectId).Distinct().Single();

            return await _projectAccessService.HasCurrentUserAccessToProject(projectId)
                && VerifyIfUserIsSupervisor(dataCollectorData.Select(dc => dc.SupervisorEmailAddress));
        }

        private bool VerifyIfUserIsSupervisor(IEnumerable<string> supervisorEmailAddresses)
        {
            var currentUserName = _authorizationService.GetCurrentUserName();

            return !_authorizationService.IsCurrentUserInRole(Role.Supervisor) || supervisorEmailAddresses.All(e => e == currentUserName);
        }
    }
}
