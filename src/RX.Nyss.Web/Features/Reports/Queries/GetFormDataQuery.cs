using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Reports.Dto;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Reports.Queries
{
    public class GetFormDataQuery : IRequest<Result<SendReportFormDataDto>>
    {
        public GetFormDataQuery(int nationalSocietyId)
        {
            NationalSocietyId = nationalSocietyId;
        }

        public int NationalSocietyId { get; }

        public class Handler : IRequestHandler<GetFormDataQuery, Result<SendReportFormDataDto>>
        {
            private readonly INyssContext _nyssContext;

            private readonly IAuthorizationService _authorizationService;

            public Handler(INyssContext nyssContext, IAuthorizationService authorizationService)
            {
                _nyssContext = nyssContext;
                _authorizationService = authorizationService;
            }

            public async Task<Result<SendReportFormDataDto>> Handle(GetFormDataQuery request, CancellationToken cancellationToken)
            {
                var currentUser = await _authorizationService.GetCurrentUser();

                var currentUserModemId = currentUser.Role switch
                {
                    Role.Manager => ((ManagerUser)currentUser).ModemId,
                    Role.TechnicalAdvisor => await GetTechnicalAdvisorModemId(currentUser.Id, request.NationalSocietyId),
                    Role.Supervisor => ((SupervisorUser)currentUser).ModemId,
                    Role.HeadSupervisor => ((HeadSupervisorUser)currentUser).ModemId,
                    _ => null
                };

                var gatewayModems = await _nyssContext.GatewayModems
                    .Where(gm => gm.GatewaySetting.NationalSocietyId == request.NationalSocietyId)
                    .Select(gm => new GatewayModemResponseDto
                    {
                        Id = gm.Id,
                        Name = gm.Name
                    })
                    .ToListAsync(cancellationToken);

                var formData = new SendReportFormDataDto
                {
                    CurrentUserModemId = currentUserModemId,
                    Modems = gatewayModems
                };

                return Result.Success(formData);
            }

            private async Task<int?> GetTechnicalAdvisorModemId(int technicalAdvisorId, int nationalSocietyId) =>
                await _nyssContext.TechnicalAdvisorUserGatewayModems
                    .Where(tam => tam.TechnicalAdvisorUserId == technicalAdvisorId && tam.GatewayModem.GatewaySetting.NationalSocietyId == nationalSocietyId)
                    .Select(tam => tam.GatewayModemId)
                    .FirstOrDefaultAsync();
        }
    }
}
