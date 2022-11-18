using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Alerts.Commands;

public class CloseCommand : IRequest<Result>
{
    public CloseCommand(int alertId)
    {
        AlertId = alertId;
    }

    private int AlertId { get; }

    public class Handler : IRequestHandler<CloseCommand, Result>
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

        public async Task<Result> Handle(CloseCommand request, CancellationToken cancellationToken)
        {
            var alertId = request.AlertId;

            if (!await _alertService.HasCurrentUserAlertEditAccess(alertId))
            {
                return Error(ResultKey.Alert.CloseAlert.NoPermission);
            }

            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var alert = await _nyssContext.Alerts
                .Where(a => a.Id == alertId)
                .SingleAsync();

            if (alert.Status != AlertStatus.Escalated)
            {
                return Error(ResultKey.Alert.CloseAlert.WrongStatus);
            }

            alert.Status = AlertStatus.Closed;
            alert.ClosedAt = _dateTimeProvider.UtcNow;
            alert.ClosedBy = await _authorizationService.GetCurrentUser();

            FormattableString updateReportsCommand = $@"UPDATE Nyss.Reports SET Status = {ReportStatus.Closed.ToString()} WHERE Status = {ReportStatus.Pending.ToString()}
                                AND Id IN (SELECT ReportId FROM Nyss.AlertReports WHERE AlertId = {alert.Id}) ";
            await _nyssContext.ExecuteSqlInterpolatedAsync(updateReportsCommand);

            await _nyssContext.SaveChangesAsync();

            transactionScope.Complete();

            return Success();
        }
    }
}


