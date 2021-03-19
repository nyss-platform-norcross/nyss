using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients.Access;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.ProjectAlertNotHandledRecipients
{
    public class ProjectAlertNotHandledRecipientAccessServiceTests
    {
        private readonly INyssContext _nyssContextMock;
        private readonly IAuthorizationService _authorizationServiceMock;
        private readonly IProjectAlertNotHandledRecipientAccessService _projectAlertNotHandledRecipientAccessService;

        public ProjectAlertNotHandledRecipientAccessServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _authorizationServiceMock = Substitute.For<IAuthorizationService>();

            var user = new ManagerUser
            {
                Role = Role.Manager
            };
            var userNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety
                {
                    UserId = 1,
                    User = user,
                    OrganizationId = 1
                }
            };

            var userNationalSocietiesMockDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.UserNationalSocieties.Returns(userNationalSocietiesMockDbSet);
            _authorizationServiceMock.GetCurrentUser().Returns(user);

            _projectAlertNotHandledRecipientAccessService = new ProjectAlertNotHandledRecipientAccessService(_authorizationServiceMock, _nyssContextMock);
        }

        [Fact]
        public async Task HasAccessToCreateForOrganization_WhenUserIsInOrganization_ShouldReturnTrue()
        {
            // Arrange
            var organizationId = 1;

            // Act
            var res = await _projectAlertNotHandledRecipientAccessService.HasAccessToCreateForOrganization(organizationId);

            // Assert
            res.ShouldBeTrue();
        }

        [Fact]
        public async Task HasAccessToCreateForOrganization_WhenUserIsNotInOrganization_ShouldReturnFalse()
        {
            // Arrange
            var organizationId = 2;

            // Act
            var res = await _projectAlertNotHandledRecipientAccessService.HasAccessToCreateForOrganization(organizationId);

            // Assert
            res.ShouldBeFalse();
        }

        [Fact]
        public async Task UserIsInOrganization_WhenUserIsInOrganization_ShouldReturnTrue()
        {
            // Arrange
            var organizationId = 1;
            var userId = 1;

            // Act
            var res = await _projectAlertNotHandledRecipientAccessService.UserIsInOrganization(userId, organizationId);

            // Assert
            res.ShouldBeTrue();
        }

        [Fact]
        public async Task UserIsInOrganization_WhenUserIsNotInOrganization_ShouldReturnFalse()
        {
            // Arrange
            var organizationId = 2;
            var userId = 1;

            // Act
            var res = await _projectAlertNotHandledRecipientAccessService.UserIsInOrganization(userId, organizationId);

            // Assert
            res.ShouldBeFalse();
        }
    }
}
