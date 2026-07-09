using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Alerts.Commands;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Alerts.Commands;

public class CloseTests : AlertFeatureBase
{

    [Theory]
    [InlineData(AlertStatus.Closed)]
    [InlineData(AlertStatus.Dismissed)]
    [InlineData(AlertStatus.Open)]
    public async Task CloseAlert_WhenAlertIsNotPending_ShouldReturnError(AlertStatus status)
    {
        Alerts.First().Status = status;

        var handler = new CloseCommand.Handler(NyssContext, AlertService, AuthorizationService, DateTimeProvider);
        var result = await handler.Handle(new CloseCommand(TestData.AlertId), CancellationToken.None);

        result.IsSuccess.ShouldBeFalse();
        result.Message.Key.ShouldBe(ResultKey.Alert.CloseAlert.WrongStatus);
    }

    [Fact]
    public async Task CloseAlert_WhenAlertMeetsTheCriteria_ChangesAlertStatus()
    {
        Alerts.First().Status = AlertStatus.Escalated;

        var handler = new CloseCommand.Handler(NyssContext, AlertService, AuthorizationService, DateTimeProvider);
        var result = await handler.Handle(new CloseCommand(TestData.AlertId), CancellationToken.None);

        Alerts.First().Status.ShouldBe(AlertStatus.Closed);
        Alerts.First().ClosedAt.ShouldBe(Now);
        Alerts.First().ClosedBy.ShouldBe(CurrentUser);
        await NyssContext.Received(1).SaveChangesAsync();
        result.IsSuccess.ShouldBeTrue();
    }
}
