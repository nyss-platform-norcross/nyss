using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Project.Access;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.DataCollector.Access
{
    public interface IDataCollectorAccessService
    {
        Task<bool> HasCurrentUserAccessToDataCollector(int dataCollectorId);
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

        public async Task<bool> HasCurrentUserAccessToDataCollector(int dataCollectorId)
        {
            var dataCollectorData = await _nyssContext.DataCollectors
                .Where(dc => dc.Id == dataCollectorId)
                .Select(dc => new { ProjectId = dc.Project.Id, SupervisorEmailAddress = dc.Supervisor.EmailAddress })
                .SingleAsync();

            return await _projectAccessService.HasCurrentUserAccessToProject(dataCollectorData.ProjectId)
                && VerifyIfUserIsSupervisor(dataCollectorData.SupervisorEmailAddress);
        }

        private bool VerifyIfUserIsSupervisor(string supervisorEmailAddress) =>
            !_authorizationService.IsCurrentUserInRole(Role.Supervisor) || supervisorEmailAddress == _authorizationService.GetCurrentUserName();
    }
}
