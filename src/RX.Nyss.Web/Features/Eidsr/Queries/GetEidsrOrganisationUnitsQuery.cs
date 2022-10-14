using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.Eidsr.Dto;
using RX.Nyss.Web.Services.EidsrClient;
using RX.Nyss.Web.Services.EidsrClient.Dto;

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
        private readonly IEidsrClient _eidsrClient;

        public Handler(IEidsrClient eidsrClient)
        {
            _eidsrClient = eidsrClient;
        }

        public async Task<Result<EidsrOrganisationUnitsResponse>> Handle(GetEidsrOrganisationUnitsQuery request, CancellationToken cancellationToken)
        {
            return await _eidsrClient.GetOrganizationUnits(request.RequestDto.EidsrApiProperties, request.RequestDto.ProgramId);
        }
    }
}