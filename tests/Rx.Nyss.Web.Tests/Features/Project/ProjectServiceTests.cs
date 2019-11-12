using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Alert.Dto;
using RX.Nyss.Web.Features.Project;
using RX.Nyss.Web.Features.Project.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using Shouldly;
using Xunit;

namespace Rx.Nyss.Web.Tests.Features.Project
{
    public class ProjectServiceTests
    {
        private readonly IProjectService _projectService;
        private readonly INyssContext _nyssContextMock;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ProjectServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            var loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _dateTimeProvider = Substitute.For<IDateTimeProvider>();
            _projectService = new ProjectService(_nyssContextMock, loggerAdapterMock, _dateTimeProvider);
        }

        [Fact]
        public async Task GetProjects_WhenNationalSocietyIsProvided_ShouldFilterResults()
        {
            // Arrange
            const int nationalSocietyId = 1;

            var project = GenerateExemplaryProjects(nationalSocietyId);

            var projectMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectMockDbSet);

            // Act
            var result = await _projectService.GetProjects(nationalSocietyId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count.ShouldBe(2);
            result.Value.Select(x => x.Id).ShouldBe(new[] { 1, 3 });

            result.Value[0].Id.ShouldBe(1);
            result.Value[0].Name.ShouldBe("1");
            result.Value[0].StartDate.ShouldBe(new DateTime(2019, 1, 1));
            result.Value[0].EndDate.ShouldBeNull();
            result.Value[0].State.ShouldBe(ProjectState.Open);
            result.Value[0].TotalReportCount.ShouldBe(1);
            result.Value[0].EscalatedAlertCount.ShouldBe(0);
            result.Value[0].ActiveDataCollectorCount.ShouldBe(1);
            //result.Value[0].SupervisorCount.ShouldBe(0);

            result.Value[1].Id.ShouldBe(3);
            result.Value[1].Name.ShouldBe("3");
            result.Value[1].StartDate.ShouldBe(new DateTime(2019, 1, 1));
            result.Value[1].EndDate.ShouldBeNull();
            result.Value[1].State.ShouldBe(ProjectState.Open);
            result.Value[1].TotalReportCount.ShouldBe(3);
            result.Value[1].EscalatedAlertCount.ShouldBe(2);
            result.Value[1].ActiveDataCollectorCount.ShouldBe(2);
            //result.Value[1].SupervisorCount.ShouldBe(0);
        }

        [Fact]
        public async Task GetProject_WhenProjectExists_ShouldReturnSuccess()
        {
            // Arrange
            const int existingProjectId = 2;

            var project = new[]
            {
                new RX.Nyss.Data.Models.Project
                {
                    Id = 1,
                    NationalSocietyId = 1,
                    ProjectHealthRisks = new List<ProjectHealthRisk>(),
                    AlertRecipients = new List<AlertRecipient>()
                },
                new RX.Nyss.Data.Models.Project
                {
                    Id = existingProjectId,
                    Name = "Name",
                    TimeZone = "Time Zone",
                    NationalSocietyId = 2,
                    State = ProjectState.Open,
                    ProjectHealthRisks = new[]
                    {
                        new ProjectHealthRisk
                        {
                            Id = 1,
                            HealthRisk = new RX.Nyss.Data.Models.HealthRisk
                            {
                                HealthRiskCode = 1,
                                HealthRiskType = HealthRiskType.Human
                            },
                            CaseDefinition = "CaseDefinition",
                            FeedbackMessage = "FeedbackMessage",
                            AlertRule = new AlertRule
                            {
                                CountThreshold = 1,
                                HoursThreshold = 48,
                                MetersThreshold = 3000
                            }
                        }
                    },
                    AlertRecipients = new[]
                    {
                        new AlertRecipient
                        {
                            Id = 1,
                            EmailAddress = "user@domain.com"
                        }
                    }
                }
            };

            var projectMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectMockDbSet);

            // Act
            var result = await _projectService.GetProject(existingProjectId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldBe(existingProjectId);
            result.Value.Name.ShouldBe("Name");
            result.Value.TimeZone.ShouldBe("Time Zone");
            result.Value.State.ShouldBe(ProjectState.Open);
            result.Value.HealthRisks.Count().ShouldBe(1);
            result.Value.HealthRisks.ElementAt(0).Id.ShouldBe(1);
            result.Value.HealthRisks.ElementAt(0).HealthRiskCode.ShouldBe(1);
            result.Value.HealthRisks.ElementAt(0).HealthRiskType.ShouldBe(HealthRiskType.Human);
            result.Value.HealthRisks.ElementAt(0).CaseDefinition.ShouldBe("CaseDefinition");
            result.Value.HealthRisks.ElementAt(0).FeedbackMessage.ShouldBe("FeedbackMessage");
            result.Value.HealthRisks.ElementAt(0).AlertRuleCountThreshold.ShouldBe(1);
            result.Value.HealthRisks.ElementAt(0).AlertRuleDaysThreshold.ShouldBe(2);
            result.Value.HealthRisks.ElementAt(0).AlertRuleMetersThreshold.ShouldBe(3000);
            result.Value.AlertRecipients.Count().ShouldBe(1);
            result.Value.AlertRecipients.ElementAt(0).Id.ShouldBe(1);
            result.Value.AlertRecipients.ElementAt(0).EmailAddress.ShouldBe("user@domain.com");
        }

        [Fact]
        public async Task GetProject_WhenProjectDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentProjectId = 0;

            var project = new[] {
                new RX.Nyss.Data.Models.Project
                {
                    Id = 1,
                    ProjectHealthRisks = new List<ProjectHealthRisk>(),
                    AlertRecipients = new List<AlertRecipient>()
                }
            };

            var projectMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectMockDbSet);

            // Act
            var result = await _projectService.GetProject(nonExistentProjectId);

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
                new RX.Nyss.Data.Models.NationalSociety {Id = nationalSocietyId, Name = "National Society"}
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            const int healthRiskId = 1;

            var healthRisks = new[]
            {
                new RX.Nyss.Data.Models.HealthRisk {Id = healthRiskId}
            };

            var healthRisksMockDbSet = healthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.HealthRisks.Returns(healthRisksMockDbSet);

            var startDate = new DateTime(2019, 1, 1);
            _dateTimeProvider.UtcNow.Returns(startDate);

            var project = new[]
            {
                new RX.Nyss.Data.Models.Project
                {
                    Id = 1,
                    Name = "Name",
                    TimeZone = "Time Zone",
                    NationalSocietyId = nationalSocietyId,
                    State = ProjectState.Open
                }
            };

            var projectMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectMockDbSet);

            var projectRequestDto = new ProjectRequestDto
            {
                Name = "New Project",
                TimeZone = "Time Zone",
                HealthRisks = new[]
                {
                    new ProjectHealthRiskRequestDto
                    {
                        HealthRiskId = healthRiskId,
                        AlertRuleCountThreshold = 1,
                        AlertRuleDaysThreshold = 2,
                        AlertRuleMetersThreshold = 3,
                        CaseDefinition = "CaseDefinition",
                        FeedbackMessage = "FeedbackMessage"
                    }
                },
                AlertRecipients = new[]
                {
                    new AlertRecipientDto
                    {
                        EmailAddress = "user@domain.com"
                    }
                }
            };

            // Act
            var result = await _projectService.AddProject(nationalSocietyId, projectRequestDto);

            // Assert
            await _nyssContextMock.Projects.Received(1).AddAsync(
                Arg.Is<RX.Nyss.Data.Models.Project>(p =>
                    p.Name == "New Project" &&
                    p.TimeZone == "Time Zone" &&
                    p.StartDate == startDate &&
                    p.EndDate == null &&
                    p.ProjectHealthRisks.Any(phr =>
                        phr.HealthRiskId == healthRiskId &&
                        phr.CaseDefinition == "CaseDefinition" &&
                        phr.FeedbackMessage == "FeedbackMessage" &&
                        phr.AlertRule.CountThreshold == 1 &&
                        phr.AlertRule.HoursThreshold == 48 &&
                        phr.AlertRule.MetersThreshold == 3) &&
                    p.AlertRecipients.Any(phr => phr.EmailAddress == "user@domain.com")));
            await _nyssContextMock.Received(1).SaveChangesAsync();
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.Project.SuccessfullyAdded);
        }

        [Fact]
        public async Task AddProject_WhenNationalSocietyDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentNationalSocietyId = 0;

            var nationalSocieties = new[] { new RX.Nyss.Data.Models.NationalSociety { Id = 1, Name = "National Society" } };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);
            
            var projectRequestDto = new ProjectRequestDto { Name = "New Project", TimeZone = "Time Zone" };

            // Act
            var result = await _projectService.AddProject(nonExistentNationalSocietyId, projectRequestDto);

            // Assert
            await _nyssContextMock.Projects.DidNotReceive().AddAsync(Arg.Any<RX.Nyss.Data.Models.Project>());
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
                new RX.Nyss.Data.Models.NationalSociety {Id = nationalSocietyId, Name = "National Society"}
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            const int nonExistentHealthRiskId = 0;

            var healthRisks = new[]
            {
                new RX.Nyss.Data.Models.HealthRisk {Id = 1}
            };

            var healthRisksMockDbSet = healthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.HealthRisks.Returns(healthRisksMockDbSet);

            var projectRequestDto = new ProjectRequestDto
            {
                Name = "New Project",
                TimeZone = "Time Zone",
                HealthRisks = new[]
                {
                    new ProjectHealthRiskRequestDto
                    {
                        HealthRiskId = nonExistentHealthRiskId
                    }
                }
            };

            // Act
            var result = await _projectService.AddProject(nationalSocietyId, projectRequestDto);

            // Assert
            await _nyssContextMock.Projects.DidNotReceive().AddAsync(Arg.Any<RX.Nyss.Data.Models.Project>());
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
                new RX.Nyss.Data.Models.NationalSociety {Id = nationalSocietyId, Name = "National Society"}
            };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var project = new[]
            {
                new RX.Nyss.Data.Models.Project
                {
                    Id = 1,
                    Name = "Name",
                    TimeZone = "Time Zone",
                    NationalSocietyId = nationalSocietyId,
                    State = ProjectState.Open
                }
            };

            var projectMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectMockDbSet);

            var projectRequestDto = new ProjectRequestDto { Name = "New Project", TimeZone = "Time Zone" };

            // Act
            var result = await _projectService.AddProject(nationalSocietyId, projectRequestDto);

            // Assert
            await _nyssContextMock.Projects.Received(1).AddAsync(
                Arg.Is<RX.Nyss.Data.Models.Project>(p =>
                    p.Name == "New Project" &&
                    p.TimeZone == "Time Zone"));
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }

        [Fact]
        public async Task UpdateProject_WhenProjectExists_ShouldReturnSuccess()
        {
            // Arrange
            const int projectId = 1;

            var nationalSocieties = new[] { new RX.Nyss.Data.Models.NationalSociety { Id = 1, Name = "National Society" } };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var project = new[]
            {
                new RX.Nyss.Data.Models.Project
                {
                    Id = projectId,
                    Name = "Name",
                    TimeZone = "Time Zone",
                    NationalSocietyId = 1,
                    State = ProjectState.Open
                }
            };

            var projectMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectMockDbSet);
            _nyssContextMock.Projects.FindAsync(projectId).Returns(project[0]);

            var projectRequestDto = new ProjectRequestDto { Name = "Updated Project", TimeZone = "Updated Time Zone" };

            // Act
            var result = await _projectService.UpdateProject(projectId, projectRequestDto);

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

            var nationalSocieties = new[] { new RX.Nyss.Data.Models.NationalSociety { Id = 1, Name = "National Society" } };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var project = new[]
            {
                new RX.Nyss.Data.Models.Project
                {
                    Id = 1,
                    Name = "Name",
                    TimeZone = "Time Zone",
                    NationalSocietyId = 1,
                    State = ProjectState.Open
                }
            };

            var projectMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectMockDbSet);
            _nyssContextMock.Projects.FindAsync(nonExistentProjectId).ReturnsNull();

            var projectRequestDto = new ProjectRequestDto { Name = "Updated Project", TimeZone = "Updated Time Zone" };

            // Act
            var result = await _projectService.UpdateProject(nonExistentProjectId, projectRequestDto);

            // Assert
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Project.ProjectDoesNotExist);
        }

        [Fact(Skip = "Currently, UpdateProject does not throw ResultException")]
        public async Task UpdateProject_WhenExceptionIsThrown_ShouldReturnError()
        {
            // Arrange
            const int projectId = 1;

            var nationalSocieties = new[] { new RX.Nyss.Data.Models.NationalSociety { Id = 1, Name = "National Society" } };

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var project = new[]
            {
                new RX.Nyss.Data.Models.Project
                {
                    Id = projectId,
                    Name = "Name",
                    TimeZone = "Time Zone",
                    NationalSocietyId = 1,
                    State = ProjectState.Open
                }
            };

            var projectMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectMockDbSet);
            _nyssContextMock.Projects.FindAsync(projectId).Returns(project[0]);

            var projectRequestDto = new ProjectRequestDto { Name = "Updated Project", TimeZone = "Updated Time Zone" };

            // Act
            var result = await _projectService.UpdateProject(projectId, projectRequestDto);

            // Assert
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }

        [Fact]
        public async Task DeleteProject_WhenProjectExists_ShouldReturnSuccess()
        {
            // Arrange
            const int existingProjectId = 1;

            var project = new[]
            {
                new RX.Nyss.Data.Models.Project
                {
                    Id = existingProjectId,
                    Name = "Name",
                    TimeZone = "Time Zone",
                    NationalSocietyId = 1,
                    State = ProjectState.Open
                }
            };

            var projectMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectMockDbSet);
            _nyssContextMock.Projects.FindAsync(existingProjectId).Returns(project[0]);

            // Act
            var result = await _projectService.DeleteProject(existingProjectId);

            // Assert
            _nyssContextMock.Projects.Received(1)
                .Remove(Arg.Is<RX.Nyss.Data.Models.Project>(p => p.Id == existingProjectId));
            await _nyssContextMock.Received(1).SaveChangesAsync();
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.Project.SuccessfullyDeleted);
        }

        [Fact]
        public async Task DeleteProject_WhenProjectDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentProjectId = 0;

            var project = new[]
            {
                new RX.Nyss.Data.Models.Project
                {
                    Id = 1,
                    Name = "Name",
                    TimeZone = "Time Zone",
                    NationalSocietyId = 1,
                    State = ProjectState.Open
                }
            };

            var projectMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectMockDbSet);
            _nyssContextMock.Projects.FindAsync(nonExistentProjectId).ReturnsNull();

            // Act
            var result = await _projectService.DeleteProject(nonExistentProjectId);

            // Assert
            _nyssContextMock.Projects.DidNotReceive().Remove(Arg.Any<RX.Nyss.Data.Models.Project>());
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Project.ProjectDoesNotExist);
        }

        [Fact(Skip = "Currently, DeleteProject does not throw ResultException")]
        public async Task DeleteProject_WhenExceptionIsThrown_ShouldReturnError()
        {
            // Arrange
            const int projectId = 1;

            var project = new[]
            {
                new RX.Nyss.Data.Models.Project
                {
                    Id = projectId,
                    Name = "Name",
                    TimeZone = "Time Zone",
                    NationalSocietyId = 1,
                    State = ProjectState.Open
                }
            };

            var projectMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectMockDbSet);
            _nyssContextMock.Projects.FindAsync(projectId).Returns(project[0]);

            // Act
            var result = await _projectService.DeleteProject(projectId);

            // Assert
            _nyssContextMock.Projects.DidNotReceive()
                .Remove(Arg.Is<RX.Nyss.Data.Models.Project>(p => p.Id == projectId));
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.UnexpectedError);
        }

        private IEnumerable<RX.Nyss.Data.Models.Project> GenerateExemplaryProjects(int nationalSocietyId) =>
            new[]
            {
                new RX.Nyss.Data.Models.Project
                {
                    Id = 1,
                    Name = "1",
                    State = ProjectState.Open,
                    StartDate = new DateTime(2019, 1, 1),
                    EndDate = null,
                    NationalSocietyId = nationalSocietyId,
                    DataCollectors = new[] { new DataCollector() },
                    ProjectHealthRisks = new[] { new ProjectHealthRisk { Reports = new[] { new Report() }, Alerts = new[] { new Alert() } } }
                },
                new RX.Nyss.Data.Models.Project
                {
                    Id = 2, Name = "2", State = ProjectState.Open, NationalSocietyId = 2
                },
                new RX.Nyss.Data.Models.Project
                {
                    Id = 3,
                    Name = "3",
                    State = ProjectState.Open,
                    StartDate = new DateTime(2019, 1, 1),
                    EndDate = null,
                    NationalSocietyId = nationalSocietyId,
                    DataCollectors = new[] { new DataCollector(), new DataCollector() },
                    ProjectHealthRisks = new[]
                    {
                        new ProjectHealthRisk
                        {
                            Reports = new[] { new Report() },
                            Alerts = new[]
                            {
                                new Alert { Id = 1, Status = AlertStatus.Pending },
                                new Alert { Id = 3, Status = AlertStatus.Escalated }
                            }
                        },
                        new ProjectHealthRisk
                        {
                            Reports = new[] { new Report(), new Report() },
                            Alerts = new[]
                            {
                                new Alert { Id = 4, Status = AlertStatus.Pending },
                                new Alert { Id = 5, Status = AlertStatus.Dismissed },
                                new Alert { Id = 6, Status = AlertStatus.Escalated }
                            }
                        }
                    }
                }
            };
    }
}
