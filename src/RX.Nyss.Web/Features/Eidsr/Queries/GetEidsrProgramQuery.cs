using System.Threading;
using System.Threading.Tasks;
using MediatR;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.Eidsr.Dto;
using RX.Nyss.Web.Services.EidsrClient;
using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Web.Features.Eidsr.Queries;

public class GetEidsrProgramQuery : IRequest<Result<EidsrProgramResponse>>
{
    public GetEidsrProgramQuery(EidsrRequestDto requestDto)
    {
        RequestDto = requestDto;
    }

    public EidsrRequestDto RequestDto { get; }

    public class Handler : IRequestHandler<GetEidsrProgramQuery, Result<EidsrProgramResponse>>
    {
        private readonly IEidsrClient _eidsrClient;

        public Handler(IEidsrClient eidsrClient)
        {
            _eidsrClient = eidsrClient;
        }

        public async Task<Result<EidsrProgramResponse>> Handle(GetEidsrProgramQuery request, CancellationToken cancellationToken)
        {
            return await _eidsrClient.GetProgramFromApi(request.RequestDto.EidsrApiProperties, request.RequestDto.ProgramId);
        }
    }
}