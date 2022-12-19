using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using OfficeOpenXml;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Alerts.Dto;
using RX.Nyss.Web.Features.Alerts.Queries;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Alerts.Queries;

public class ExportTests : AlertFeatureBase
{
    [Fact]
    public async Task Export_WhenExporting_ShouldCallExportServiceToExcel()
    {
        // Arrange
        var alerts = TestData.GetAlerts();
        var alertsMockDbSet = alerts.AsQueryable().BuildMockDbSet();
        var projects = new List<Project> { alerts.Select(a => a.ProjectHealthRisk.Project).First() };
        var projectsMockDbSet = projects.AsQueryable().BuildMockDbSet();
        var users = TestData.GetUsers();
        var usersMockDbSet = users.AsQueryable().BuildMockDbSet();
        var excelDoc = new ExcelPackage();
        excelDoc.Workbook.Worksheets.Add("title");
        NyssContext.Users.Returns(usersMockDbSet);
        NyssContext.Projects.Returns(projectsMockDbSet);
        NyssContext.Projects.FindAsync(1).Returns(projects[0]);
        NyssContext.Alerts.Returns(alertsMockDbSet);
        AuthorizationService.GetCurrentUser().Returns(users[0]);
        AuthorizationService.GetCurrentUserName().Returns(users[0].EmailAddress);
        StringsService.GetForCurrentUser().Returns(new StringsResourcesVault(new Dictionary<string, StringResourceValue>()));
        ExcelExportService.ToExcel(
            Arg.Any<IReadOnlyList<AlertListExportResponseDto>>(),
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<string>()).Returns(excelDoc);

        // Act

        var handler = new ExportQuery.Handler(NyssContext,  AuthorizationService, ExcelExportService, StringsService);
        var res = await handler.Handle(new ExportQuery(1, new AlertListFilterRequestDto
        {
            Locations = null,
            Status = AlertStatusFilter.All,
            OrderBy = "Status",
            SortAscending = true
        }), CancellationToken.None);

        // Assert
        ExcelExportService.Received(1).ToExcel(
            Arg.Any<IReadOnlyList<AlertListExportResponseDto>>(),
            Arg.Any<IReadOnlyList<string>>(),
            Arg.Any<string>());
    }
}
