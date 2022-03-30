using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Projects.Dto;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Projects.Queries;

public class GetProjectBasicDataQuery : IRequest<Result<ProjectBasicDataResponseDto>>
{
    public int ProjectId { get; set; }

    public GetProjectBasicDataQuery(int projectId)
    {
        ProjectId = projectId;
    }

    public class Handler : IRequestHandler<GetProjectBasicDataQuery, Result<ProjectBasicDataResponseDto>>
    {
        private readonly INyssContext _nyssContext;

        public Handler(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public async Task<Result<ProjectBasicDataResponseDto>> Handle(GetProjectBasicDataQuery request, CancellationToken cancellationToken)
        {
            var project = await _nyssContext.Projects
                .Select(p => new ProjectBasicDataResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    IsClosed = p.State == ProjectState.Closed,
                    AllowMultipleOrganizations = p.AllowMultipleOrganizations,
                    NationalSociety = new ProjectBasicDataResponseDto.NationalSocietyIdDto
                    {
                        Id = p.NationalSociety.Id,
                        Name = p.NationalSociety.Name,
                        CountryName = p.NationalSociety.Country.Name
                    }
                })
                .SingleAsync(p => p.Id == request.ProjectId, cancellationToken);

            return Success(project);
        }
    }
}
