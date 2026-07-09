using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Alerts.Commands;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Alerts.Commands;

public class DismissTests : AlertFeatureBase
{
    [Fact]
    public async Task DismissAlert_WhenAlertIsClosed_ShouldReturnError()
    {
        Alerts.First().Status = AlertStatus.Closed;

        var handler = new DismissCommand.Handler(NyssContext, AlertService, AuthorizationService, DateTimeProvider);
        var result = await handler.Handle(new DismissCommand(TestData.AlertId), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Message.Key.ShouldBe(ResultKey.Alert.DismissAlert.WrongStatus);
    }

    [Fact]
    public async Task DismissAlert_WhenAlertAcceptedReportAndPendingReportCountIsGreaterOrEqualToThreshold_ShouldReturnError()
    {
        Alerts.First().Status = AlertStatus.Open;

        Alerts.First().AlertReports = new List<AlertReport>
            {
                new AlertReport
                {
                    Report = new Report
                    {
                        Status = ReportStatus.Accepted,
                        RawReport = new RawReport { Village = new Village() }
                    }
                },
                new AlertReport
                {
                    Report = new Report
                    {
                        Status = ReportStatus.Accepted,
                        RawReport = new RawReport { Village = new Village() }
                    }
                },
                new AlertReport
                {
                    Report = new Report
                    {
                        Status = ReportStatus.Pending,
                        RawReport = new RawReport { Village = new Village() }
                    }
                },
                new AlertReport
                {
                    Report = new Report
                    {
                        Status = ReportStatus.Rejected,
                        RawReport = new RawReport { Village = new Village() }
                    }
                },
                new AlertReport
                {
                    Report = new Report
                    {
                        Status = ReportStatus.Rejected,
                        RawReport = new RawReport { Village = new Village() }
                    }
                },
                new AlertReport
                {
                    Report = new Report
                    {
                        Status = ReportStatus.Rejected,
                        RawReport = new RawReport { Village = new Village() }
                    }
                },
                new AlertReport
                {
                    Report = new Report
                    {
                        Status = ReportStatus.Rejected,
                        RawReport = new RawReport { Village = new Village() }
                    }
                }
            };

        Alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 3;

        var handler = new DismissCommand.Handler(NyssContext, AlertService, AuthorizationService, DateTimeProvider);
        var result = await handler.Handle(new DismissCommand(TestData.AlertId), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Message.Key.ShouldBe(ResultKey.Alert.DismissAlert.PossibleEscalation);
    }

    [Fact]
    public async Task DismissAlert_WhenAlertMeetsTheCriteria_ChangesAlertStatus()
    {
        Alerts.First().Status = AlertStatus.Open;

        Alerts.First().AlertReports = new List<AlertReport>
            {
                new AlertReport
                {
                    Report = new Report
                    {
                        Status = ReportStatus.Rejected,
                        RawReport = new RawReport { Village = new Village() }
                    }
                }
            };

        Alerts.First().ProjectHealthRisk.AlertRule.CountThreshold = 1;

        var handler = new DismissCommand.Handler(NyssContext, AlertService, AuthorizationService, DateTimeProvider);
        var result = await handler.Handle(new DismissCommand(TestData.AlertId), CancellationToken.None);

        Alerts.First().Status.ShouldBe(AlertStatus.Dismissed);
        Alerts.First().DismissedAt.ShouldBe(Now);
        Alerts.First().DismissedBy.ShouldBe(CurrentUser);
        await NyssContext.Received(1).SaveChangesAsync();
        result.IsSuccess.ShouldBeTrue();
    }
}
