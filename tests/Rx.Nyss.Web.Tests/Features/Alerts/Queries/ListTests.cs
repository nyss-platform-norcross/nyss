using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Features.Alerts.Queries;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Alerts.Queries;

public class ListTests : AlertFeatureBase
{
    [Theory]
    [InlineData(AlertStatusFilter.All)]
    [InlineData(AlertStatusFilter.Open)]
    [InlineData(AlertStatusFilter.Escalated)]
    [InlineData(AlertStatusFilter.Dismissed)]
    [InlineData(AlertStatusFilter.Closed)]
    public async Task List_WhenFiltering_ShouldReturnSubsetOfAlerts(AlertStatusFilter alertStatusFilter)
    {
        var alerts = TestData.GetAlertsForFiltering();
        var alertsMockDbSet = alerts.AsQueryable().BuildMockDbSet();
        var projects = new List<Project> { alerts.Select(a => a.ProjectHealthRisk.Project).First() };
        var projectsMockDbSet = projects.AsQueryable().BuildMockDbSet();
        var users = TestData.GetUsers();
        var usersMockDbSet = users.AsQueryable().BuildMockDbSet();
        NyssContext.Users.Returns(usersMockDbSet);
        NyssContext.Projects.Returns(projectsMockDbSet);
        NyssContext.Projects.FindAsync(1).Returns(projects[0]);
        NyssContext.Alerts.Returns(alertsMockDbSet);
        AuthorizationService.GetCurrentUser().Returns(users[0]);
        AuthorizationService.GetCurrentUserName().Returns(users[0].EmailAddress);

        var handler = new GetListQuery.Handler(NyssContext, Config, AuthorizationService);
        var res = await handler.Handle(new GetListQuery(1, 1,
            new AlertListFilterRequestDto
            {
                Locations = null,
                HealthRiskId = null,
                OrderBy = "Status",
                SortAscending = true,
                Status = alertStatusFilter,
                StartDate = new DateTime(1990, 1, 1),
                EndDate = new DateTime(2032, 1, 1)
            }), CancellationToken.None);

        if (alertStatusFilter == AlertStatusFilter.All)
        {
            res.Value.Data.Count.ShouldBe(4);
        }
        else
        {
            res.Value.Data.All(a => a.Status == MapToAlertStatus(alertStatusFilter).ToString()).ShouldBeTrue();
            res.Value.Data.Count.ShouldBe(1);
        }
    }

    private AlertStatus MapToAlertStatus(AlertStatusFilter filter) =>
        filter switch
        {
            AlertStatusFilter.Open => AlertStatus.Open,
            AlertStatusFilter.Escalated => AlertStatus.Escalated,
            AlertStatusFilter.Dismissed => AlertStatus.Dismissed,
            AlertStatusFilter.Closed => AlertStatus.Closed,
            _ => AlertStatus.Open
        };
}
