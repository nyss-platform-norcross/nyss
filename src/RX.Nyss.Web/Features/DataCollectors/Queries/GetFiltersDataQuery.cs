using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.DataCollectors.Queries;

public class GetFiltersDataQuery : IRequest<Result<DataCollectorFiltersReponseDto>>
{
    public int ProjectId { get; set; }

    public GetFiltersDataQuery(int projectId)
    {
        ProjectId = projectId;
    }

    public class Handler : IRequestHandler<GetFiltersDataQuery, Result<DataCollectorFiltersReponseDto>>
    {
        private readonly INyssContext _nyssContext;
        private readonly IDataCollectorService _dataCollectorService;
        private readonly IAuthorizationService _authorizationService;
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;

        public Handler(
            INyssContext nyssContext,
            IDataCollectorService dataCollectorService,
            IAuthorizationService authorizationService,
            INationalSocietyStructureService nationalSocietyStructureService)
        {
            _nyssContext = nyssContext;
            _dataCollectorService = dataCollectorService;
            _authorizationService = authorizationService;
            _nationalSocietyStructureService = nationalSocietyStructureService;
        }

        public async Task<Result<DataCollectorFiltersReponseDto>> Handle(GetFiltersDataQuery query, CancellationToken cancellationToken)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            var projectData = await _nyssContext.Projects
                .Where(p => p.Id == query.ProjectId)
                .Select(dc => new
                {
                    NationalSocietyId = dc.NationalSociety.Id,
                    OrganizationId = dc.NationalSociety.NationalSocietyUsers
                        .Where(nsu => nsu.User == currentUser)
                        .Select(nsu => nsu.OrganizationId)
                        .FirstOrDefault()
                })
                .SingleAsync(cancellationToken);

            var supervisors = await _dataCollectorService.GetAllSupervisors(query.ProjectId, currentUser, projectData.OrganizationId)
                .ToListAsync(cancellationToken);

            var filtersData = new DataCollectorFiltersReponseDto
            {
                Supervisors = supervisors,
                Locations = await _nationalSocietyStructureService.Get(projectData.NationalSocietyId)
            };

            return Success(filtersData);
        }
    }
}
