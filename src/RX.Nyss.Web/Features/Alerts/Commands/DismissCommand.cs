using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Alerts.Commands;

public class DismissCommand : IRequest<Result>
{

    public DismissCommand(int alertId)
    {
        AlertId = alertId;
    }

    private int AlertId { get; }

    public class Handler : IRequestHandler<DismissCommand, Result>
    {
        private readonly INyssContext _nyssContext;
        private readonly IAlertService _alertService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDateTimeProvider _dateTimeProvider;

        public Handler(
            INyssContext nyssContext,
            IAlertService alertService,
            IAuthorizationService authorizationService,
            IDateTimeProvider dateTimeProvider
        )
        {
            _nyssContext = nyssContext;
            _alertService = alertService;
            _authorizationService = authorizationService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> Handle(DismissCommand request, CancellationToken cancellationToken)
        {
            var alertId = request.AlertId;

            if (!await _alertService.HasCurrentUserAlertEditAccess(alertId))
            {
                return Error(ResultKey.Alert.DismissAlert.NoPermission);
            }

            var alertData = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .Select(alert => new
                {
                    Alert = alert,
                    LanguageCode = alert.ProjectHealthRisk.Project.NationalSociety.ContentLanguage.LanguageCode,
                    NotificationEmails = alert.ProjectHealthRisk.Project.AlertNotificationRecipients.Select(ar => ar.Email).ToList(),
                    CountThreshold = alert.ProjectHealthRisk.AlertRule.CountThreshold,
                    MaximumAcceptedReportCount = alert.AlertReports.Count(r => r.Report.Status == ReportStatus.Accepted || r.Report.Status == ReportStatus.Pending)
                })
                .SingleAsync();

            if (alertData.Alert.Status != AlertStatus.Open)
            {
                return Error(ResultKey.Alert.DismissAlert.WrongStatus);
            }

            if (alertData.MaximumAcceptedReportCount >= alertData.CountThreshold)
            {
                return Error(ResultKey.Alert.DismissAlert.PossibleEscalation);
            }

            alertData.Alert.Status = AlertStatus.Dismissed;
            alertData.Alert.DismissedAt = _dateTimeProvider.UtcNow;
            alertData.Alert.DismissedBy = await _authorizationService.GetCurrentUser();
            await _nyssContext.SaveChangesAsync();

            return Success();
        }
    }
}
