using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.Eidsr.Dto;
using RX.Nyss.Web.Services.EidsrClient.Dto;
using RX.Nyss.Web.Services.EidsrService;

namespace RX.Nyss.Web.Features.Eidsr.Queries;

public class GetEidsrOrganisationUnitsQuery : IRequest<Result<EidsrOrganisationUnitsResponse>>
{
    public GetEidsrOrganisationUnitsQuery(EidsrRequestDto requestDto)
    {
        RequestDto = requestDto;
    }

    public EidsrRequestDto RequestDto { get; }

    public class Handler : IRequestHandler<GetEidsrOrganisationUnitsQuery, Result<EidsrOrganisationUnitsResponse>>
    {
        private readonly IEidsrService _eidsrService;

        public Handler(IEidsrService eidsrService)
        {
            _eidsrService = eidsrService;
        }

        public async Task<Result<EidsrOrganisationUnitsResponse>> Handle(GetEidsrOrganisationUnitsQuery request, CancellationToken cancellationToken)
            => await _eidsrService.GetOrganizationUnits(request.RequestDto.EidsrApiProperties, request.RequestDto.ProgramId);
    }
}