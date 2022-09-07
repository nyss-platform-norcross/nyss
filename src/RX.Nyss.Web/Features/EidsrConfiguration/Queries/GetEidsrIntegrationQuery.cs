namespace RX.Nyss.Web.Features.EidsrConfiguration.Queries;

using RX.Nyss.Web.Features.EidsrConfiguration.Dto;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;

public class GetEidsrIntegrationQuery : IRequest<Result<EidsrIntegrationResponseDto>>
{
    public GetEidsrIntegrationQuery(int id)
    {
        Id = id;
    }

    public int Id { get; }

    public class Handler : IRequestHandler<GetEidsrIntegrationQuery, Result<EidsrIntegrationResponseDto>>
    {
        private readonly INyssContext _nyssContext;

        public Handler(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public async Task<Result<EidsrIntegrationResponseDto>> Handle(GetEidsrIntegrationQuery request, CancellationToken cancellationToken)
        {
            var eidsrConfiguration = await _nyssContext.EidsrConfiguration
                .FirstOrDefaultAsync(x =>
                    x.NationalSocietyId == request.Id, cancellationToken: cancellationToken);

            var eidsrConfigurationDto = new EidsrIntegrationResponseDto
            {
                Id = eidsrConfiguration?.Id,
                Username = eidsrConfiguration?.Username,
                Password = eidsrConfiguration?.PasswordHash, //TODO: implement hashing algorithm
                ApiBaseUrl = eidsrConfiguration?.ApiBaseUrl,
                TrackerProgramId = eidsrConfiguration?.TrackerProgramId,
                LocationDataElementId = eidsrConfiguration?.LocationDataElementId,
                DateOfOnsetDataElementId = eidsrConfiguration?.DateOfOnsetDataElementId,
                PhoneNumberDataElementId = eidsrConfiguration?.PhoneNumberDataElementId,
                SuspectedDiseaseDataElementId = eidsrConfiguration?.SuspectedDiseaseDataElementId,
                EventTypeDataElementId = eidsrConfiguration?.EventTypeDataElementId,
                GenderDataElementId = eidsrConfiguration?.GenderDataElementId,
            };

            return Result.Success(eidsrConfigurationDto);
        }
    }
}


