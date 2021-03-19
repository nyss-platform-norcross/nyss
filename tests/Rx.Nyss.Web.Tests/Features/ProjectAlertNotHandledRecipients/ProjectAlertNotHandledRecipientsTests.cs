using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients;
using RX.Nyss.Web.Features.ProjectAlertNotHandledRecipients.Dto;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.ProjectAlertNotHandledRecipients
{
    public class ProjectAlertNotHandledRecipientsTests
    {
        private readonly INyssContext _nyssContextMock;
        private readonly IAuthorizationService _authorizationServiceMock;
        private readonly IProjectAlertNotHandledRecipientService _projectAlertNotHandledRecipientService;

        public ProjectAlertNotHandledRecipientsTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            _authorizationServiceMock = Substitute.For<IAuthorizationService>();
            var logger = Substitute.For<ILoggerAdapter>();

            _projectAlertNotHandledRecipientService = new ProjectAlertNotHandledRecipientService(_nyssContextMock, _authorizationServiceMock, logger);

            var users = new List<User> { new ManagerUser { Id = 1 } };
            var userNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety
                {
                    UserId = 1,
                    User = users[0],
                    OrganizationId = 1
                }
            };
            var alertNotHandledRecipients = new List<AlertNotHandledNotificationRecipient>
            {
                new AlertNotHandledNotificationRecipient
                {
                    UserId = 1,
                    ProjectId = 1
                }
            };
            var projects = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    AlertNotHandledNotificationRecipients = alertNotHandledRecipients,
                    NationalSociety = new NationalSociety
                    {
                        NationalSocietyUsers = new List<UserNationalSociety>
                        {
                            new UserNationalSociety
                            {
                                User = new ManagerUser
                                {
                                    Role = Role.Manager
                                },
                                OrganizationId = 1,
                                Organization = new Organization
                                {
                                    Name = "Org"
                                }
                            },
                            new UserNationalSociety
                            {
                                User = new TechnicalAdvisorUser
                                {
                                    Role = Role.TechnicalAdvisor
                                },
                                OrganizationId = 1,
                                Organization = new Organization
                                {
                                    Name = "Org"
                                }
                            },
                            new UserNationalSociety
                            {
                                User = new SupervisorUser
                                {
                                    Role = Role.Supervisor
                                },
                                OrganizationId = 1,
                                Organization = new Organization
                                {
                                    Name = "Org"
                                }
                            }
                        }
                    }
                }
            };
            var projectOrganizations = new List<ProjectOrganization>
            {
                new ProjectOrganization
                {
                    Organization = new Organization
                    {
                        Id = 1,
                        Name = "Org"
                    },
                    Project = projects[0],
                    ProjectId = 1
                }
            };
            var projectsMockDbSet = projects.AsQueryable().BuildMockDbSet();
            var usersMockDbSet = users.AsQueryable().BuildMockDbSet();
            var userNationalSocietiesMockDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            var alertNotHandledRecipientsDbSet = alertNotHandledRecipients.AsQueryable().BuildMockDbSet();
            var projectOrganizationsMockDbSet = projectOrganizations.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            _nyssContextMock.Users.Returns(usersMockDbSet);
            _nyssContextMock.UserNationalSocieties.Returns(userNationalSocietiesMockDbSet);
            _nyssContextMock.AlertNotHandledNotificationRecipients.Returns(alertNotHandledRecipientsDbSet);
            _nyssContextMock.ProjectOrganizations.Returns(projectOrganizationsMockDbSet);
            _authorizationServiceMock.GetCurrentUser().Returns(users[0]);
        }

        [Fact]
        public async Task Create_WhenDoesNotExist_ShouldReturnSuccess()
        {
            // Arrange
            var userId = 2;
            var projectId = 1;

            // Act
            var res = await _projectAlertNotHandledRecipientService.Create(projectId, userId);

            // Assert
            res.IsSuccess.ShouldBeTrue();
            res.Message.Key.ShouldBe(ResultKey.AlertNotHandledNotificationRecipient.CreateSuccess);
        }

        [Fact]
        public async Task Create_WhenDoesExist_ShouldFail()
        {
            // Arrange
            var userId = 1;
            var projectId = 1;

            // Act
            var res = await _projectAlertNotHandledRecipientService.Create(projectId, userId);

            // Assert
            res.IsSuccess.ShouldBeFalse();
            res.Message.Key.ShouldBe(ResultKey.AlertNotHandledNotificationRecipient.AlreadyExists);
        }

        [Fact]
        public async Task Edit_WhenRecipientExists_ShouldReturnSuccess()
        {
            // Arrange
            var projectId = 1;
            var dto = new ProjectAlertNotHandledRecipientRequestDto
            {
                UserId = 2,
                OrganizationId = 1
            };

            // Act
            var res = await _projectAlertNotHandledRecipientService.Edit(projectId, dto);

            // Assert
            res.IsSuccess.ShouldBeTrue();
            res.Message.Key.ShouldBe(ResultKey.AlertNotHandledNotificationRecipient.EditSuccess);
        }

        [Fact]
        public async Task Edit_WhenNoRecipientExists_ShouldFail()
        {
            // Arrange
            var projectId = 1;
            var dto = new ProjectAlertNotHandledRecipientRequestDto
            {
                UserId = 2,
                OrganizationId = 2
            };

            // Act
            var res = await _projectAlertNotHandledRecipientService.Edit(projectId, dto);

            // Assert
            res.IsSuccess.ShouldBeFalse();
            res.Message.Key.ShouldBe(ResultKey.AlertNotHandledNotificationRecipient.NotFound);
        }

        [Fact]
        public async Task List_ShouldReturnAllRecipientsInProject()
        {
            // Arrange
            var projectId = 1;

            // Act
            var res = await _projectAlertNotHandledRecipientService.List(projectId);

            // Assert
            res.Value.Count.ShouldBe(1);
        }

        [Fact]
        public async Task GetFormData_ShouldReturnAllAppropriateUsers()
        {
            // Arrange
            var projectId = 1;

            // Act
            var res = await _projectAlertNotHandledRecipientService.GetFormData(projectId);

            // Assert
            res.Value.Count.ShouldBe(2);
        }
    }
}
