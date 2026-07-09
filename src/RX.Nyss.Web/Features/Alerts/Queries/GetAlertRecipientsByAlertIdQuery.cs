using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Alerts.Queries;

public class GetAlertRecipientsByAlertIdQuery : IRequest<Result<AlertRecipientsResponseDto>>
{
    public GetAlertRecipientsByAlertIdQuery(int alertId)
    {
        AlertId = alertId;
    }

    private int AlertId { get; }


    public class Handler : IRequestHandler<GetAlertRecipientsByAlertIdQuery, Result<AlertRecipientsResponseDto>>
    {
        private readonly INyssContext _nyssContext;
        private readonly IAlertService _alertService;
        private readonly IAuthorizationService _authorizationService;

        public Handler(
            INyssContext nyssContext,
            IAlertService alertService,
            IAuthorizationService authorizationService
        )
        {
            _nyssContext = nyssContext;
            _alertService = alertService;
            _authorizationService = authorizationService;
        }

        public async Task<Result<AlertRecipientsResponseDto>> Handle(GetAlertRecipientsByAlertIdQuery request, CancellationToken cancellationToken)
        {
            var alertId = request.AlertId;

            var currentUserOrganization = await _nyssContext.Alerts.Where(a => a.Id == alertId)
                .SelectMany(a => a.ProjectHealthRisk.Project.ProjectOrganizations
                    .Where(po => po.Organization.NationalSocietyUsers.Any(nsu => nsu.User.EmailAddress == _authorizationService.GetCurrentUserName())))
                .SingleOrDefaultAsync();

            var recipients = (await _alertService.GetAlertRecipients(alertId)).Select(r => new
            {
                IsAnonymized = !_authorizationService.IsCurrentUserInRole(Role.Administrator) && r.OrganizationId != currentUserOrganization.OrganizationId,
                r.PhoneNumber,
                r.Email
            }).ToList();

            return Success(new AlertRecipientsResponseDto
            {
                Emails = recipients.Where(r => !string.IsNullOrEmpty(r.Email)).Select(r => r.IsAnonymized
                    ? "***"
                    : r.Email).ToList(),
                PhoneNumbers = recipients.Where(r => !string.IsNullOrEmpty(r.PhoneNumber)).Select(r => r.IsAnonymized
                    ? "***"
                    : r.PhoneNumber).ToList(),
            });
        }
    }
}
