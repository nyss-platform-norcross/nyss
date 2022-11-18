using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Features.NationalSocietyStructure;
using RX.Nyss.Web.Features.Projects;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Alerts.Queries;

public class GetFiltersDataQuery : IRequest<Result<AlertListFilterResponseDto>>
{

    public GetFiltersDataQuery(int projectId)
    {
        ProjectId = projectId;
    }

    private int ProjectId { get; }

    public class Handler : IRequestHandler<GetFiltersDataQuery, Result<AlertListFilterResponseDto>>
    {
        private readonly IProjectService _projectService;
        private readonly INyssContext _nyssContext;
        private readonly INationalSocietyStructureService _nationalSocietyStructureService;

        public Handler(
            INyssContext nyssContext,
            IProjectService projectService,
            INationalSocietyStructureService nationalSocietyStructureService
            )
        {
            _projectService = projectService;
            _nyssContext = nyssContext;
            _nationalSocietyStructureService = nationalSocietyStructureService;
        }

        public async Task<Result<AlertListFilterResponseDto>> Handle(GetFiltersDataQuery request, CancellationToken cancellationToken)
        {
            var healthRiskTypes = new List<HealthRiskType>
            {
                HealthRiskType.Human,
                HealthRiskType.NonHuman,
                HealthRiskType.UnusualEvent
            };
            var healthRisks = await _projectService.GetHealthRiskNames(request.ProjectId, healthRiskTypes);
            var nationalSocietyId = await _nyssContext.Projects
                .Where(p => p.Id == request.ProjectId)
                .Select(p => p.NationalSocietyId)
                .SingleAsync();
            var locations = await _nationalSocietyStructureService.Get(nationalSocietyId);

            return Success(new AlertListFilterResponseDto
            {
                HealthRisks = healthRisks,
                Locations = locations
            });
        }
    }

}


