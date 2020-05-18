using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.ProjectAlertRecipients;
using RX.Nyss.Web.Features.ProjectAlertRecipients.Dto;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.ProjectAlertRecipients
{
    public class ProjectAlertRecipientsTests
    {
        private const int ProjectId = 1;
        private const int NationalSocietyId = 1;
        private readonly ProjectAlertRecipientService _projectAlertRecipientService;
        private readonly IAuthorizationService _authorizationServiceMock;
        private readonly IHttpContextAccessor _httpContextAccessorMock;
        private readonly List<AlertNotificationRecipient> _alertNotificationRecipients;
        private readonly INyssContext _nyssContextMock;

        public ProjectAlertRecipientsTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();

            var users = new List<User>
            {
                new ManagerUser
                {
                    Id = 1,
                    EmailAddress = "manager@example.com"
                },
                new ManagerUser
                {
                    Id = 2,
                    EmailAddress = "manager2@example.com"
                }
            };

            var userNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety
                {
                    NationalSocietyId = 1,
                    UserId = 1,
                    OrganizationId = 1
                },
                new UserNationalSociety
                {
                    NationalSocietyId = 1,
                    UserId = 2
                }
            };

            var alertRecipients = new List<AlertNotificationRecipient>
            {
                new AlertNotificationRecipient
                {
                    Id = 1,
                    Role = "Someguy",
                    Organization = "RCRC",
                    Email = "test@example.com",
                    PhoneNumber = "+123456",
                    OrganizationId = 1,
                    ProjectId = 1
                }
            };

            var usersDbSet = users.AsQueryable().BuildMockDbSet();
            var userNationalSocietiesDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            var alertRecipientsDbSet = alertRecipients.AsQueryable().BuildMockDbSet();

            _httpContextAccessorMock.HttpContext.User.Identity.Name.Returns("manager@example.com");
            _nyssContextMock.Users.Returns(usersDbSet);
            _nyssContextMock.UserNationalSocieties.Returns(userNationalSocietiesDbSet);
            _nyssContextMock.AlertNotificationRecipients.Returns(alertRecipientsDbSet);

            _authorizationServiceMock = new AuthorizationService(_httpContextAccessorMock, _nyssContextMock);
            _projectAlertRecipientService = new ProjectAlertRecipientService(_nyssContextMock, _authorizationServiceMock);
        }

        [Fact]
        public async Task Create_WhenAlertRecipientDoesntExist_ShouldReturnSuccess()
        {
            // Arrange
            var alertRecipient = new ProjectAlertRecipientRequestDto
            {
                Role = "Head",
                Organization = "RCRC",
                Email = "head@rcrc.org",
                PhoneNumber = "+35235243"
            };

            // Act
            var res = await _projectAlertRecipientService.Create(NationalSocietyId, ProjectId, alertRecipient);

            // Assert
            res.IsSuccess.ShouldBe(true);
        }

        [Fact]
        public async Task Create_WhenAlertRecipientDoesExist_ShouldFail()
        {
            // Arrange
            var alertRecipient = new ProjectAlertRecipientRequestDto
            {
                Role = "Head",
                Organization = "RCRC",
                Email = "test@example.com",
                PhoneNumber = "+123456"
            };

            // Act
            var res = await _projectAlertRecipientService.Create(NationalSocietyId, ProjectId, alertRecipient);

            // Assert
            res.IsSuccess.ShouldBe(false);
            res.Message.Key.ShouldBe(ResultKey.AlertRecipient.AlertRecipientAlreadyAdded);
        }

        [Fact]
        public async Task Create_WhenUserIsNotTiedToOrganization_ShouldFail()
        {
            // Arrange
            _httpContextAccessorMock.HttpContext.User.Identity.Name.Returns("manager2@example.com");
            var alertRecipient = new ProjectAlertRecipientRequestDto
            {
                Role = "Head",
                Organization = "RCRC",
                Email = "head@rcrc.org",
                PhoneNumber = "+35235243"
            };

            // Act
            var res = await _projectAlertRecipientService.Create(NationalSocietyId, ProjectId, alertRecipient);

            // Assert
            res.IsSuccess.ShouldBe(false);
            res.Message.Key.ShouldBe(ResultKey.AlertRecipient.CurrentUserMustBeTiedToAnOrganization);
        }

        [Fact]
        public async Task Edit_WhenAlertRecipientDoesntExist_ShouldFail()
        {
            // Arrange
            var alertRecipientId = 2;
            var alertRecipient = new ProjectAlertRecipientRequestDto
            {
                Id = alertRecipientId,
                Role = "Head",
                Organization = "RCRC",
                Email = "head@rcrc.org",
                PhoneNumber = "+35235243"
            };

            // Act
            var res = await _projectAlertRecipientService.Edit(alertRecipientId, alertRecipient);

            // Assert
            res.IsSuccess.ShouldBe(false);
            res.Message.Key.ShouldBe(ResultKey.AlertRecipient.AlertRecipientDoesNotExist);
        }

        [Fact]
        public async Task Edit_WhenAlertRecipientDoesExist_ShouldReturnSuccess()
        {
            // Arrange
            var alertRecipientId = 1;
            var alertRecipient = new ProjectAlertRecipientRequestDto
            {
                Id = alertRecipientId,
                Role = "Head",
                Organization = "RCRC",
                Email = "head@rcrc.org",
                PhoneNumber = "+35235243"
            };

            // Act
            var res = await _projectAlertRecipientService.Edit(alertRecipientId, alertRecipient);

            // Assert
            res.IsSuccess.ShouldBe(true);
            res.Message.Key.ShouldBe(ResultKey.AlertRecipient.AlertRecipientSuccessfullyEdited);
        }

        [Fact]
        public async Task Delete_WhenAlertRecipientDoesExist_ShouldReturnSuccess()
        {
            // Arrange
            var alertRecipientId = 1;

            // Act
            var res = await _projectAlertRecipientService.Delete(alertRecipientId);

            // Assert
            res.IsSuccess.ShouldBe(true);
        }

        [Fact]
        public async Task Delete_WhenAlertRecipientDoesntExist_ShouldFail()
        {
            // Arrange
            var alertRecipientId = 2;

            // Act
            var res = await _projectAlertRecipientService.Delete(alertRecipientId);

            // Assert
            res.IsSuccess.ShouldBe(false);
            res.Message.Key.ShouldBe(ResultKey.AlertRecipient.AlertRecipientDoesNotExist);
        }

        [Fact]
        public async Task Get_WhenAlertRecipientDoesExist_ShouldReturnAlertRecipient()
        {
            // Arrange
            var alertRecipientId = 1;

            // Act
            var res = await _projectAlertRecipientService.Get(alertRecipientId);

            // Assert
            res.IsSuccess.ShouldBe(true);
            res.Value.Id.ShouldBe(alertRecipientId);
        }

        [Theory]
        [InlineData("manager@example.com")]
        [InlineData("manager2@example.com")]
        public async Task List_ShouldReturnAlertRecipientForTheUsersOrganizationOnly(string userName)
        {
            // Arrange
            _httpContextAccessorMock.HttpContext.User.Identity.Name.Returns(userName);
            var nationalSocietyId = 1;
            var projectId = 1;

            // Act
            var res = await _projectAlertRecipientService.List(nationalSocietyId, projectId);

            // Assert
            res.IsSuccess.ShouldBe(true);
            res.Value.Count.ShouldBe(userName == "manager@example.com" ? 1 : 0);
        }
    }
}
