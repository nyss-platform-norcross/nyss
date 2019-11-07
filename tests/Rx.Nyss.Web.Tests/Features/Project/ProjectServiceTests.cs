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
using RX.Nyss.Web.Features.Project;
using RX.Nyss.Web.Features.Project.Dto;
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

        public ProjectServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            var loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _projectService = new ProjectService(_nyssContextMock, loggerAdapterMock);
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
            result.Value.Select(x => x.Id).ShouldBe(new[] {1, 3});

            result.Value[0].Id.ShouldBe(1);
            result.Value[0].Name.ShouldBe("1");
            //result.Value[0].StartDate.ShouldBe(DateTime.MinValue);
            result.Value[0].EndDate.ShouldBeNull();
            result.Value[0].State.ShouldBe(ProjectState.Open);
            result.Value[0].ReportCount.ShouldBe(0);
            result.Value[0].EscalatedAlertCount.ShouldBe(0);
            result.Value[0].ActiveDataCollectorCount.ShouldBe(1);
            //result.Value[0].SupervisorCount.ShouldBe(0);

            result.Value[1].Id.ShouldBe(3);
            result.Value[1].Name.ShouldBe("3");
            //result.Value[1].StartDate.ShouldBe(DateTime.MinValue);
            result.Value[1].EndDate.ShouldBeNull();
            result.Value[1].State.ShouldBe(ProjectState.Open);
            result.Value[1].ReportCount.ShouldBe(5);
            result.Value[1].EscalatedAlertCount.ShouldBe(2);
            result.Value[1].ActiveDataCollectorCount.ShouldBe(2);
            //result.Value[1].SupervisorCount.ShouldBe(0);
        }

        [Fact]
        public async Task GetProject_WhenProjectExists_ShouldReturnSuccess()
        {
            // Arrange
            const int existingNationalSocietyId = 3;

            var project = new[]
            {
                new RX.Nyss.Data.Models.Project {Id = 1, NationalSocietyId = 1},
                new RX.Nyss.Data.Models.Project {Id = 2, NationalSocietyId = 2},
                new RX.Nyss.Data.Models.Project
                {
                    Id = existingNationalSocietyId,
                    Name = "Name",
                    TimeZone = "Time Zone",
                    NationalSocietyId = 1,
                    State = ProjectState.Open
                }
            };

            var projectMockDbSet = project.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Projects.Returns(projectMockDbSet);

            // Act
            var result = await _projectService.GetProject(existingNationalSocietyId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldBe(3);
            result.Value.Name.ShouldBe("Name");
            result.Value.TimeZone.ShouldBe("Time Zone");
            result.Value.State.ShouldBe(ProjectState.Open);
        }

        [Fact]
        public async Task GetProject_WhenProjectDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentProjectId = 0;

            var project = new[] {new RX.Nyss.Data.Models.Project {Id = 1}};

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

            var projectRequestDto = new ProjectRequestDto {Name = "New Project", TimeZone = "Time Zone"};

            // Act
            var result = await _projectService.AddProject(nationalSocietyId, projectRequestDto);

            // Assert
            await _nyssContextMock.Projects.Received(1).AddAsync(
                Arg.Is<RX.Nyss.Data.Models.Project>(p =>
                    p.Name == "New Project" &&
                    p.TimeZone == "Time Zone"));
            await _nyssContextMock.Received(1).SaveChangesAsync();
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.Project.SuccessfullyAdded);
        }

        [Fact]
        public async Task AddProject_WhenNationalSocietyDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentNationalSocietyId = 0;

            var nationalSocieties = new[] {new RX.Nyss.Data.Models.NationalSociety {Id = 1, Name = "National Society"}};

            var nationalSocietiesMockDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContextMock.NationalSocieties.Returns(nationalSocietiesMockDbSet);

            var projectRequestDto = new ProjectRequestDto {Name = "New Project", TimeZone = "Time Zone"};

            // Act
            var result = await _projectService.AddProject(nonExistentNationalSocietyId, projectRequestDto);

            // Assert
            await _nyssContextMock.Projects.DidNotReceive().AddAsync(Arg.Any<RX.Nyss.Data.Models.Project>());
            await _nyssContextMock.DidNotReceive().SaveChangesAsync();
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.Project.NationalSocietyDoesNotExist);
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

            var projectRequestDto = new ProjectRequestDto {Name = "New Project", TimeZone = "Time Zone"};

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

            var nationalSocieties = new[] {new RX.Nyss.Data.Models.NationalSociety {Id = 1, Name = "National Society"}};

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

            var projectRequestDto = new ProjectRequestDto {Name = "Updated Project", TimeZone = "Updated Time Zone"};

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

            var nationalSocieties = new[] {new RX.Nyss.Data.Models.NationalSociety {Id = 1, Name = "National Society"}};

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

            var projectRequestDto = new ProjectRequestDto {Name = "Updated Project", TimeZone = "Updated Time Zone"};

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

            var nationalSocieties = new[] {new RX.Nyss.Data.Models.NationalSociety {Id = 1, Name = "National Society"}};

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

            var projectRequestDto = new ProjectRequestDto {Name = "Updated Project", TimeZone = "Updated Time Zone"};

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
                    NationalSocietyId = nationalSocietyId,
                    DataCollectors = new[] {new DataCollector()},
                    ProjectHealthRisks = Array.Empty<ProjectHealthRisk>()
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
                    NationalSocietyId = nationalSocietyId,
                    DataCollectors = new[] {new DataCollector(), new DataCollector()},
                    ProjectHealthRisks = new[]
                    {
                        new ProjectHealthRisk
                        {
                            Reports = new[]
                            {
                                new Report
                                {
                                    ReportAlerts = new[]
                                    {
                                        new AlertReport
                                        {
                                            Alert = new Alert
                                            {
                                                Id = 1, Status = AlertStatus.Pending
                                            }
                                        }
                                    }
                                },
                                new Report
                                {
                                    ReportAlerts = new[]
                                    {
                                        new AlertReport
                                        {
                                            Alert = new Alert
                                            {
                                                Id = 2,
                                                Status = AlertStatus.Dismissed
                                            }
                                        },
                                        new AlertReport
                                        {
                                            Alert = new Alert
                                            {
                                                Id = 3,
                                                Status = AlertStatus.Escalated
                                            }
                                        }
                                    }
                                },
                            }
                        },
                        new ProjectHealthRisk
                        {
                            Reports = new[]
                            {
                                new Report
                                {
                                    ReportAlerts = new[]
                                    {
                                        new AlertReport
                                        {
                                            Alert = new Alert
                                            {
                                                Id = 4, Status = AlertStatus.Pending
                                            }
                                        }
                                    }
                                },
                                new Report
                                {
                                    ReportAlerts = new[]
                                    {
                                        new AlertReport
                                        {
                                            Alert = new Alert
                                            {
                                                Id = 6,
                                                Status = AlertStatus.Escalated
                                            }
                                        }
                                    }
                                },
                                new Report
                                {
                                    ReportAlerts = new[]
                                    {
                                        new AlertReport
                                        {
                                            Alert = new Alert
                                            {
                                                Id = 5,
                                                Status = AlertStatus.Dismissed
                                            }
                                        },
                                        new AlertReport
                                        {
                                            Alert = new Alert
                                            {
                                                Id = 6,
                                                Status = AlertStatus.Escalated
                                            }
                                        }
                                    }
                                },
                            }
                        }
                    }
                },
            };
    }
}
