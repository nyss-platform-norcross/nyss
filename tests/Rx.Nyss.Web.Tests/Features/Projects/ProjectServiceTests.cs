using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataCollectors;
using RX.Nyss.Web.Features.Projects;
using RX.Nyss.Web.Features.Projects.Dto;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Projects
{
    public class ProjectServiceTests
    {
        private readonly IProjectService _projectService;
        private readonly INyssContext _nyssContextMock;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IDataCollectorService _dataCollectorService;
        private readonly IAuthorizationService _authorizationService;

        public ProjectServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            var loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _dateTimeProvider = Substitute.For<IDateTimeProvider>();
            _authorizationService = Substitute.For<IAuthorizationService>();
            _dataCollectorService = Substitute.For<IDataCollectorService>();
            _projectService = new ProjectService(_nyssContextMock, loggerAdapterMock, _dateTimeProvider, _authorizationService, _dataCollectorService);
        }

        [Fact]
        public async Task ListProjects_WhenNationalSocietyIsProvided_ShouldFilterResults()
        {
            // Arrange
            const int nationalSocietyId = 1;

            var project = GenerateExemplaryProjects(nationalSocietyId);

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);

            var currentUser = new AdministratorUser();
            _authorizationService.GetCurrentUser().Returns(currentUser);

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = nationalSocietyId,
                    NationalSocietyUsers = new List<UserNationalSociety> { new UserNationalSociety { User = currentUser } }
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var currentDate = new DateTime(2019, 1, 1);
            _dateTimeProvider.UtcNow.Returns(currentDate);

            // Act
            var result = await _projectService.List(nationalSocietyId, 0);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count.ShouldBe(2);
            result.Value.Select(x => x.Id).ShouldBe(new[] { 1, 3 });

            result.Value[0].Id.ShouldBe(1);
            result.Value[0].Name.ShouldBe("1");
            result.Value[0].StartDate.ShouldBe(new DateTime(2019, 1, 1));
            result.Value[0].EndDate.ShouldBeNull();
            result.Value[0].TotalReportCount.ShouldBe(1);
            result.Value[0].EscalatedAlertCount.ShouldBe(0);
            result.Value[0].TotalDataCollectorCount.ShouldBe(1);
            result.Value[0].SupervisorCount.ShouldBe(0);

            result.Value[1].Id.ShouldBe(3);
            result.Value[1].Name.ShouldBe("3");
            result.Value[1].StartDate.ShouldBe(new DateTime(2019, 1, 1));
            result.Value[1].EndDate.ShouldBeNull();
            result.Value[1].TotalReportCount.ShouldBe(3);
            result.Value[1].EscalatedAlertCount.ShouldBe(2);
            result.Value[1].TotalDataCollectorCount.ShouldBe(2);
            result.Value[1].SupervisorCount.ShouldBe(0);
        }

        [Fact]
        public async Task GetProject_WhenProjectExists_ShouldReturnSuccess()
        {
            // Arrange
            const int existingProjectId = 2;

            var nationalSociety1 = new NationalSociety
            {
                Id = 1,
                ContentLanguage = new ContentLanguage { Id = 2 },
                Organizations = new List<Organization>(),
                NationalSocietyUsers = new List<UserNationalSociety>()
            };

            var nationalSociety2 = new NationalSociety
            {
                Id = 2,
                ContentLanguage = new ContentLanguage { Id = 1 },
                Organizations = new List<Organization>(),
                NationalSocietyUsers = new List<UserNationalSociety>()
            };

            var nationalSocieties = new[] { nationalSociety1, nationalSociety2 };

            var project = new[]
            {
                new Project
                {
                    Id = 1,
                    NationalSocietyId = 1,
                    NationalSociety = nationalSociety1,
                    ProjectHealthRisks = new List<ProjectHealthRisk>()
                },
                new Project
                {
                    Id = existingProjectId,
                    Name = "Name",
                    State = ProjectState.Open,
                    NationalSocietyId = 2,
                    NationalSociety = nationalSociety2,
                    ProjectHealthRisks = new[]
                    {
                        new ProjectHealthRisk
                        {
                            Id = 1,
                            HealthRiskId = 10,
                            HealthRisk = new HealthRisk
                            {
                                HealthRiskCode = 100,
                                HealthRiskType = HealthRiskType.Human,
                                LanguageContents = new[]
                                {
                                    new HealthRiskLanguageContent
                                    {
                                        Name = "HealthRiskName",
                                        ContentLanguage = new ContentLanguage { Id = 1 }
                                    }
                                }
                            },
                            CaseDefinition = "CaseDefinition",
                            FeedbackMessage = "FeedbackMessage",
                            AlertRule = new AlertRule
                            {
                                CountThreshold = 1,
                                DaysThreshold = 2,
                                KilometersThreshold = 3
                            },
                            Reports = new[] { new Report() }
                        }
                    }
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);

            var healthRisks = Array.Empty<HealthRisk>();
            var healthRisksMockDbSet = healthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.HealthRisks.Returns(healthRisksMockDbSet);

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var userNationalSocieties = new List<UserNationalSociety>();
            var userNationalSocietiesMockDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.UserNationalSocieties.Returns(userNationalSocietiesMockDbSet);

            // Act
            var result = await _projectService.Get(existingProjectId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldBe(existingProjectId);
            result.Value.Name.ShouldBe("Name");
            result.Value.State.ShouldBe(ProjectState.Open);
            result.Value.ProjectHealthRisks.Count().ShouldBe(1);
            result.Value.ProjectHealthRisks.ElementAt(0).Id.ShouldBe(1);
            result.Value.ProjectHealthRisks.ElementAt(0).HealthRiskId.ShouldBe(10);
            result.Value.ProjectHealthRisks.ElementAt(0).HealthRiskCode.ShouldBe(100);
            result.Value.ProjectHealthRisks.ElementAt(0).HealthRiskName.ShouldBe("HealthRiskName");
            result.Value.ProjectHealthRisks.ElementAt(0).CaseDefinition.ShouldBe("CaseDefinition");
            result.Value.ProjectHealthRisks.ElementAt(0).FeedbackMessage.ShouldBe("FeedbackMessage");
            result.Value.ProjectHealthRisks.ElementAt(0).AlertRuleCountThreshold.ShouldBe(1);
            result.Value.ProjectHealthRisks.ElementAt(0).AlertRuleDaysThreshold.ShouldBe(2);
            result.Value.ProjectHealthRisks.ElementAt(0).AlertRuleKilometersThreshold.ShouldBe(3);
            result.Value.ProjectHealthRisks.ElementAt(0).ContainsReports.ShouldBe(true);
        }

        [Fact]
        public async Task GetProject_WhenProjectDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentProjectId = 0;

            var project = new[]
            {
                new Project
                {
                    Id = 1,
                    ProjectHealthRisks = new List<ProjectHealthRisk>(),
                    AlertNotificationRecipients = new List<AlertNotificationRecipient>(),
                    NationalSociety = new NationalSociety
                    {
                        ContentLanguage = new ContentLanguage { Id = 1 },
                        Organizations = new List<Organization>(),
                        NationalSocietyUsers = new List<UserNationalSociety>()
                    }
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);

            // Act
            var result = await _projectService.Get(nonExistentProjectId);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Project.ProjectDoesNotExist);
        }

        [Fact]
        public async Task AddProject_ForCorrectData_ShouldReturnSuccess()
        {
            // Arrange
            const int nationalSocietyId = 1;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = nationalSocietyId,
                    Name = "National Society",
                    Organizations = new List<Organization>
                    {
                        new Organization
                        {
                            Id = 1
                        }
                    },
                    NationalSocietyUsers = new List<UserNationalSociety>()
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            const int healthRiskId = 1;

            var healthRisks = new[] { new HealthRisk { Id = healthRiskId } };

            var healthRisksMockDbSet = healthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.HealthRisks.Returns(healthRisksMockDbSet);

            var users = new List<User>
            {
                new ManagerUser
                {
                    Id = 1,
                    UserNationalSocieties = new List<UserNationalSociety>
                    {
                        new UserNationalSociety
                        {
                            NationalSocietyId = nationalSocietyId,
                            UserId = 1,
                            OrganizationId = 1
                        }
                    }
                }
            };
            var usersMockDbSet = users.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Users.Returns(usersMockDbSet);

            var startDate = new DateTime(2019, 1, 1);
            _dateTimeProvider.UtcNow.Returns(startDate);

            var project = new[]
            {
                new Project
                {
                    Id = 1,
                    Name = "Name",
                    NationalSocietyId = nationalSocietyId,
                    State = ProjectState.Open
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);

            var projectRequestDto = new CreateProjectRequestDto
            {
                Name = "New Project",
                HealthRisks = new[]
                {
                    new ProjectHealthRiskRequestDto
                    {
                        HealthRiskId = healthRiskId,
                        AlertRuleCountThreshold = 1,
                        AlertRuleDaysThreshold = 2,
                        AlertRuleKilometersThreshold = 3,
                        CaseDefinition = "CaseDefinition",
                        FeedbackMessage = "FeedbackMessage"
                    }
                },
                AlertNotHandledNotificationRecipientId = 1
            };

            // Act
            var result = await _projectService.Create(nationalSocietyId, projectRequestDto);

            // Assert
            await _nyssContextMock.Projects.Received(1).AddAsync(
                Arg.Is<Project>(p =>
                    p.Name == "New Project" &&
                    p.StartDate == startDate &&
                    p.EndDate == null &&
                    p.ProjectHealthRisks.Any(phr =>
                        phr.HealthRiskId == healthRiskId &&
                        phr.CaseDefinition == "CaseDefinition" &&
                        phr.FeedbackMessage == "FeedbackMessage" &&
                        phr.AlertRule.CountThreshold == 1 &&
                        phr.AlertRule.DaysThreshold == 2 &&
                        phr.AlertRule.KilometersThreshold == 3)));
            await _nyssContextMock.Received(1).SaveChangesAsync();
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.Project.SuccessfullyAdded);
        }

        [Fact]
        public async Task AddProject_WhenNationalSocietyDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentNationalSocietyId = 0;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = 1,
                    Name = "National Society"
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var projectRequestDto = new CreateProjectRequestDto
            {
                Name = "New Project"
            };

            // Act
            var result = await _projectService.Create(nonExistentNationalSocietyId, projectRequestDto);

            // Assert
            await _nyssContextMock.Projects.DidNotReceive().AddAsync(Arg.Any<Project>());
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Project.NationalSocietyDoesNotExist);
        }

        [Fact]
        public async Task AddProject_WhenHealthRiskDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nationalSocietyId = 1;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = nationalSocietyId,
                    Name = "National Society",
                    Organizations = new List<Organization>(),
                    NationalSocietyUsers = new List<UserNationalSociety>()
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            const int nonExistentHealthRiskId = 0;

            var healthRisks = new[] { new HealthRisk { Id = 1 } };

            var healthRisksMockDbSet = healthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.HealthRisks.Returns(healthRisksMockDbSet);

            var projectRequestDto = new CreateProjectRequestDto
            {
                Name = "New Project",
                HealthRisks = new[] { new ProjectHealthRiskRequestDto { HealthRiskId = nonExistentHealthRiskId } }
            };

            // Act
            var result = await _projectService.Create(nationalSocietyId, projectRequestDto);

            // Assert
            await _nyssContextMock.Projects.DidNotReceive().AddAsync(Arg.Any<Project>());
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Project.HealthRiskDoesNotExist);
        }

        [Fact(Skip = "Currently, AddProject does not throw ResultException")]
        public async Task AddProject_WhenExceptionIsThrown_ShouldReturnError()
        {
            // Arrange
            const int nationalSocietyId = 1;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = nationalSocietyId,
                    Name = "National Society"
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var project = new[]
            {
                new Project
                {
                    Id = 1,
                    Name = "Name",
                    NationalSocietyId = nationalSocietyId,
                    State = ProjectState.Open
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);

            var projectRequestDto = new CreateProjectRequestDto
            {
                Name = "New Project",
            };

            // Act
            var result = await _projectService.Create(nationalSocietyId, projectRequestDto);

            // Assert
            await _nyssContextMock.Projects.Received(1).AddAsync(
                Arg.Is<Project>(p => p.Name == "New Project"));
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }

        [Fact]
        public async Task UpdateProject_WhenProjectExists_ShouldReturnSuccess()
        {
            // Arrange
            const int projectId = 1;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = 1,
                    Name = "National Society",
                    Organizations = new List<Organization>(),
                    NationalSocietyUsers = new List<UserNationalSociety>()
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var project = new[]
            {
                new Project
                {
                    Id = projectId,
                    Name = "Name",
                    NationalSocietyId = 1,
                    State = ProjectState.Open,
                    ProjectHealthRisks = new List<ProjectHealthRisk>
                    {
                        new ProjectHealthRisk
                        {
                            Project = new Project(),
                            Id = 1,
                            CaseDefinition = "Case Definition 1",
                            FeedbackMessage = "Feedback Message 2",
                            AlertRule = new AlertRule
                            {
                                CountThreshold = 1,
                                DaysThreshold = 2,
                                KilometersThreshold = 3
                            },
                            Reports = new List<Report>()
                        },
                        new ProjectHealthRisk
                        {
                            Id = 2,
                            Project = new Project(),
                            CaseDefinition = "Case Definition 2",
                            FeedbackMessage = "Feedback Message 2",
                            AlertRule = new AlertRule
                            {
                                CountThreshold = 1,
                                DaysThreshold = 2,
                                KilometersThreshold = 3
                            },
                            Reports = new List<Report>()
                        }
                    }
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            _nyssContextMock.Projects.FindAsync(projectId).Returns(project[0]);

            var projectHealthRisksMockDbSet = project.SelectMany(p => p.ProjectHealthRisks).AsQueryable().BuildMockDbSet();
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksMockDbSet);

            var projectRequestDto = new EditProjectRequestDto
            {
                Name = "Updated Project",
                HealthRisks = new List<ProjectHealthRiskRequestDto>
                {
                    new ProjectHealthRiskRequestDto
                    {
                        Id = 2,
                        HealthRiskId = 1,
                        CaseDefinition = "Updated Case Definition 2",
                        FeedbackMessage = "Updated Feedback Message 2",
                        AlertRuleCountThreshold = 2,
                        AlertRuleDaysThreshold = 3,
                        AlertRuleKilometersThreshold = 4
                    },
                    new ProjectHealthRiskRequestDto
                    {
                        Id = null,
                        HealthRiskId = 2,
                        CaseDefinition = "Case Definition 3",
                        FeedbackMessage = "Feedback Message 3",
                        AlertRuleCountThreshold = 3,
                        AlertRuleDaysThreshold = 4,
                        AlertRuleKilometersThreshold = 5
                    }
                }
            };

            // Act
            var result = await _projectService.Edit(projectId, projectRequestDto);

            // Assert
            await _nyssContextMock.Received(1).SaveChangesAsync();
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.Project.SuccessfullyUpdated);
        }

        [Fact]
        public async Task UpdateProject_WhenProjectDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentProjectId = 0;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = 1,
                    Name = "National Society"
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var project = new[]
            {
                new Project
                {
                    Id = 1,
                    Name = "Name",
                    NationalSocietyId = 1,
                    State = ProjectState.Open
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            _nyssContextMock.Projects.FindAsync(nonExistentProjectId).ReturnsNull();

            var projectRequestDto = new EditProjectRequestDto
            {
                Name = "Updated Project"
            };

            // Act
            var result = await _projectService.Edit(nonExistentProjectId, projectRequestDto);

            // Assert
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Project.ProjectDoesNotExist);
        }

        [Fact]
        public async Task UpdateProject_WhenRemovedHealthRiskContainsReports_ShouldReturnError()
        {
            // Arrange
            const int projectId = 1;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = 1,
                    Name = "National Society",
                    Organizations = new List<Organization>(),
                    NationalSocietyUsers = new List<UserNationalSociety>()
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var project = new[]
            {
                new Project
                {
                    Id = projectId,
                    Name = "Name",
                    NationalSocietyId = 1,
                    State = ProjectState.Open,
                    ProjectHealthRisks = new List<ProjectHealthRisk>
                    {
                        new ProjectHealthRisk
                        {
                            Id = 1,
                            Project = new Project { Id = projectId },
                            CaseDefinition = "Case Definition",
                            FeedbackMessage = "Feedback Message",
                            AlertRule = new AlertRule
                            {
                                CountThreshold = 1,
                                DaysThreshold = 2,
                                KilometersThreshold = 3
                            },
                            Reports = new List<Report> { new Report() }
                        }
                    }
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            _nyssContextMock.Projects.FindAsync(projectId).Returns(project[0]);

            var projectHealthRisksMockDbSet = project.SelectMany(p => p.ProjectHealthRisks).AsQueryable().BuildMockDbSet();
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksMockDbSet);

            var projectRequestDto = new EditProjectRequestDto
            {
                Name = "Updated Project",
                HealthRisks = new List<ProjectHealthRiskRequestDto>()
            };

            // Act
            var result = await _projectService.Edit(projectId, projectRequestDto);

            // Assert
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Project.HealthRiskContainsReports);
        }

        [Fact(Skip = "Currently, UpdateProject does not throw ResultException")]
        public async Task UpdateProject_WhenExceptionIsThrown_ShouldReturnError()
        {
            // Arrange
            const int projectId = 1;

            var nationalSocieties = new[]
            {
                new NationalSociety
                {
                    Id = 1,
                    Name = "National Society"
                }
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var project = new[]
            {
                new Project
                {
                    Id = projectId,
                    Name = "Name",
                    NationalSocietyId = 1,
                    State = ProjectState.Open
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            _nyssContextMock.Projects.FindAsync(projectId).Returns(project[0]);

            var projectRequestDto = new EditProjectRequestDto
            {
                Name = "Updated Project"
            };

            // Act
            var result = await _projectService.Edit(projectId, projectRequestDto);

            // Assert
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }

        [Fact]
        public async Task CloseProject_WhenProjectExists_ShouldReturnSuccess()
        {
            // Arrange
            const int existingProjectId = 1;

            var project = new[]
            {
                new Project
                {
                    Id = existingProjectId,
                    Name = "Name",
                    NationalSocietyId = 1,
                    State = ProjectState.Open,
                    ProjectHealthRisks = new List<ProjectHealthRisk>(),
                    ProjectOrganizations = new List<ProjectOrganization>()
                }
            };

            _authorizationService.IsCurrentUserInAnyRole(Role.Coordinator, Role.Administrator).Returns(true);

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            var dcMockDbSet = new List<DataCollector>().AsQueryable().BuildMockDbSet();
            _nyssContextMock.DataCollectors.Returns(dcMockDbSet);
            var nationalSocietiesMockDbSet = new[] { new NationalSociety() }.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            // Act
            var result = await _projectService.Close(existingProjectId);

            // Assert
            _nyssContextMock.Projects.First().State.ShouldBe(ProjectState.Closed);
            await _dataCollectorService.Received(1).AnonymizeDataCollectorsWithReports(existingProjectId);
            await _nyssContextMock.Received(1).SaveChangesAsync();
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.Project.SuccessfullyClosed);
        }


        [Fact]
        public async Task CloseProject_WhenAlreadyClosed_ShouldReturnError()
        {
            // Arrange
            const int existingProjectId = 1;

            var project = new[]
            {
                new Project
                {
                    Id = existingProjectId,
                    Name = "Name",
                    NationalSocietyId = 1,
                    State = ProjectState.Closed,
                    ProjectHealthRisks = new List<ProjectHealthRisk>(),
                    ProjectOrganizations = new List<ProjectOrganization>()
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            var dcMockDbSet = new List<DataCollector>().AsQueryable().BuildMockDbSet();
            _nyssContextMock.DataCollectors.Returns(dcMockDbSet);

            // Act
            var result = await _projectService.Close(existingProjectId);

            // Assert
            await _dataCollectorService.DidNotReceive().AnonymizeDataCollectorsWithReports(existingProjectId);
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Project.ProjectAlreadyClosed);
        }


        [Theory]
        [InlineData(AlertStatus.Escalated)]
        [InlineData(AlertStatus.Pending)]
        public async Task CloseProject_WhenOpenOrPendingAlerts_ShouldReturnError(AlertStatus alertStatus)
        {
            // Arrange
            const int existingProjectId = 1;

            var project = new[]
            {
                new Project
                {
                    Id = existingProjectId,
                    Name = "Name",
                    NationalSocietyId = 1,
                    State = ProjectState.Open,
                    ProjectHealthRisks = new[] { new ProjectHealthRisk { Alerts = new[] { new Alert { Status = alertStatus } } } },
                    ProjectOrganizations = new List<ProjectOrganization>()
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            var dcMockDbSet = new List<DataCollector>().AsQueryable().BuildMockDbSet();
            _nyssContextMock.DataCollectors.Returns(dcMockDbSet);

            // Act
            var result = await _projectService.Close(existingProjectId);

            // Assert
            await _dataCollectorService.DidNotReceive().AnonymizeDataCollectorsWithReports(existingProjectId);
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Project.ProjectHasOpenOrEscalatedAlerts);
        }

        [Fact]
        public async Task CloseProject_WhenDataCollectorsWithReports_ShouldAnonymize()
        {
            // Arrange
            const int existingProjectId = 1;

            _authorizationService.IsCurrentUserInAnyRole(Role.Coordinator, Role.Administrator).Returns(true);

            var project = new[]
            {
                new Project
                {
                    Id = existingProjectId,
                    Name = "Name",
                    NationalSocietyId = 1,
                    State = ProjectState.Open,
                    ProjectHealthRisks = new List<ProjectHealthRisk>(),
                    ProjectOrganizations = new List<ProjectOrganization>()
                }
            };
            var dataCollectorsToAnonymize = new List<DataCollector>
            {
                new DataCollector
                {
                    Project = project[0],
                    RawReports = new[] { new RawReport() }
                },
                new DataCollector
                {
                    Project = project[0],
                    RawReports = new[] { new RawReport() }
                }
            };
            var dataCollectorsToDelete = new List<DataCollector>
            {
                new DataCollector
                {
                    Id = 123,
                    Project = project[0]
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            var dcMockDbSet = dataCollectorsToAnonymize.Union(dataCollectorsToDelete).AsQueryable().BuildMockDbSet();
            _nyssContextMock.DataCollectors.Returns(dcMockDbSet);
            var nationalSocietiesMockDbSet = new[] { new NationalSociety() }.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            // Act
            var result = await _projectService.Close(existingProjectId);

            // Assert
            await _dataCollectorService.Received(1).AnonymizeDataCollectorsWithReports(existingProjectId);
            _nyssContextMock.DataCollectors.Received(1).RemoveRange(Arg.Any<IQueryable<DataCollector>>());
            await _nyssContextMock.Received(1).SaveChangesAsync();
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.Project.SuccessfullyClosed);
        }

        [Fact]
        public async Task CloseProject_WhenProjectDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentProjectId = 0;

            var project = new[]
            {
                new Project
                {
                    Id = 1,
                    Name = "Name",
                    NationalSocietyId = 1,
                    State = ProjectState.Open,
                    ProjectHealthRisks = new List<ProjectHealthRisk>(),
                    ProjectOrganizations = new List<ProjectOrganization>()
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            _nyssContextMock.Projects.FindAsync(nonExistentProjectId).ReturnsNull();

            // Act
            var result = await _projectService.Close(nonExistentProjectId);

            // Assert
            _nyssContextMock.Projects.DidNotReceive().Remove(Arg.Any<Project>());
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Project.ProjectDoesNotExist);
        }

        [Fact(Skip = "Currently, CloseProject does not throw ResultException")]
        public async Task CloseProject_WhenExceptionIsThrown_ShouldReturnError()
        {
            // Arrange
            const int projectId = 1;

            var project = new[]
            {
                new Project
                {
                    Id = projectId,
                    Name = "Name",
                    NationalSocietyId = 1,
                    State = ProjectState.Open
                }
            };

            var projectsMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectsMockDbSet);
            _nyssContextMock.Projects.FindAsync(projectId).Returns(project[0]);

            // Act
            var result = await _projectService.Close(projectId);

            // Assert
            _nyssContextMock.Projects.DidNotReceive()
                .Remove(Arg.Is<Project>(p => p.Id == projectId));
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }

        private IEnumerable<Project> GenerateExemplaryProjects(int nationalSocietyId)
        {
            var projectHealthRisk = new ProjectHealthRisk { HealthRisk = new HealthRisk() };

            return new[]
            {
                new Project
                {
                    Id = 1,
                    Name = "1",
                    State = ProjectState.Open,
                    StartDate = new DateTime(2019, 1, 1),
                    EndDate = null,
                    NationalSocietyId = nationalSocietyId,
                    DataCollectors = new[]
                    {
                        new DataCollector
                        {
                            DataCollectorType = DataCollectorType.Human,
                            Reports = new[]
                            {
                                new Report
                                {
                                    IsTraining = false,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                },
                                new Report
                                {
                                    IsTraining = true,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                }
                            }
                        }
                    },
                    SupervisorUserProjects = new List<SupervisorUserProject>(),
                    ProjectHealthRisks = new[]
                    {
                        new ProjectHealthRisk
                        {
                            Reports = new[]
                            {
                                new Report
                                {
                                    IsTraining = false,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                },
                                new Report
                                {
                                    IsTraining = true,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                }
                            },
                            Alerts = new[]
                            {
                                new Alert
                                {
                                    Id = 10,
                                    Status = AlertStatus.Pending
                                }
                            }
                        }
                    }
                },
                new Project
                {
                    Id = 2,
                    Name = "2",
                    State = ProjectState.Open,
                    NationalSocietyId = 2
                },
                new Project
                {
                    Id = 3,
                    Name = "3",
                    State = ProjectState.Open,
                    StartDate = new DateTime(2019, 1, 1),
                    EndDate = null,
                    NationalSocietyId = nationalSocietyId,
                    DataCollectors = new[]
                    {
                        new DataCollector
                        {
                            DataCollectorType = DataCollectorType.Human,
                            Reports = new[]
                            {
                                new Report
                                {
                                    IsTraining = false,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                },
                                new Report
                                {
                                    IsTraining = true,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                }
                            }
                        },
                        new DataCollector
                        {
                            DataCollectorType = DataCollectorType.Human,
                            Reports = new[]
                            {
                                new Report
                                {
                                    IsTraining = false,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                },
                                new Report
                                {
                                    IsTraining = false,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                },
                                new Report
                                {
                                    IsTraining = true,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                }
                            }
                        }
                    },
                    SupervisorUserProjects = new List<SupervisorUserProject>(),
                    ProjectHealthRisks = new[]
                    {
                        new ProjectHealthRisk
                        {
                            Reports = new[]
                            {
                                new Report
                                {
                                    IsTraining = false,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                },
                                new Report
                                {
                                    IsTraining = true,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                }
                            },
                            Alerts = new[]
                            {
                                new Alert
                                {
                                    Id = 1,
                                    Status = AlertStatus.Pending
                                },
                                new Alert
                                {
                                    Id = 3,
                                    Status = AlertStatus.Escalated,
                                    EscalatedAt = new DateTime(2019, 1, 1)
                                }
                            }
                        },
                        new ProjectHealthRisk
                        {
                            Reports = new[]
                            {
                                new Report
                                {
                                    IsTraining = false,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                },
                                new Report
                                {
                                    IsTraining = false,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                },
                                new Report
                                {
                                    IsTraining = true,
                                    ProjectHealthRisk = projectHealthRisk,
                                    ReceivedAt = new DateTime(2019, 1, 1),
                                    ReportedCase = new ReportCase
                                    {
                                        CountFemalesAtLeastFive = 1,
                                        CountFemalesBelowFive = 0,
                                        CountMalesAtLeastFive = 0,
                                        CountMalesBelowFive = 0
                                    },
                                    ReportedCaseCount = 1
                                }
                            },
                            Alerts = new[]
                            {
                                new Alert
                                {
                                    Id = 4,
                                    Status = AlertStatus.Pending
                                },
                                new Alert
                                {
                                    Id = 5,
                                    Status = AlertStatus.Dismissed
                                },
                                new Alert
                                {
                                    Id = 6,
                                    Status = AlertStatus.Escalated,
                                    EscalatedAt = new DateTime(2019, 1, 1)
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
