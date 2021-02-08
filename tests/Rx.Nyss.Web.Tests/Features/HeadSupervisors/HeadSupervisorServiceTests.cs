using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.HeadSupervisors;
using RX.Nyss.Web.Features.HeadSupervisors.Dto;
using RX.Nyss.Web.Features.Organizations;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.HeadSupervisors
{
    public class HeadSupervisorServiceTests
    {
        private readonly HeadSupervisorService _headSupervisorService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationServiceMock;
        private readonly IVerificationEmailService _verificationEmailServiceMock;
        private readonly IOrganizationService _organizationServiceMock;
        private readonly IAuthorizationService _authorizationServiceMock;
        private readonly int _nationalSocietyId1 = 1;
        private readonly int _nationalSocietyId2 = 2;
        private readonly int _projectId1 = 1;
        private readonly int _projectId2 = 2;
        private readonly int _projectId3 = 3;
        private readonly int _projectId4 = 4;
        private readonly int _projectId5 = 5;
        private readonly int _administratorId = 1;
        private readonly int _headSupervisorWithSupervisors = 2;
        private readonly int _headSupervisorWithoutSupervisors = 3;
        private readonly int _supervisorId = 4;
        private readonly int _dataCollectorId = 1;
        private readonly int _deletedDataCollectorId = 2;
        private readonly IDeleteUserService _deleteUserService;


        public HeadSupervisorServiceTests()
        {
            _loggerAdapter = Substitute.For<ILoggerAdapter>();
            _nyssContext = Substitute.For<INyssContext>();
            _identityUserRegistrationServiceMock = Substitute.For<IIdentityUserRegistrationService>();
            _verificationEmailServiceMock = Substitute.For<IVerificationEmailService>();
            _deleteUserService = Substitute.For<IDeleteUserService>();
            _organizationServiceMock = Substitute.For<IOrganizationService>();
            _authorizationServiceMock = Substitute.For<IAuthorizationService>();
            var dateTimeProvider = Substitute.For<IDateTimeProvider>();
            var applicationLanguages = new List<ApplicationLanguage>();
            var applicationLanguagesDbSet = applicationLanguages.AsQueryable().BuildMockDbSet();
            _nyssContext.ApplicationLanguages.Returns(applicationLanguagesDbSet);

            _headSupervisorService = new HeadSupervisorService(_identityUserRegistrationServiceMock, _nyssContext, _organizationServiceMock, _loggerAdapter, _verificationEmailServiceMock,
                _deleteUserService, dateTimeProvider, _authorizationServiceMock);

            _identityUserRegistrationServiceMock.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()).Returns(ci => new IdentityUser
            {
                Id = "123",
                Email = (string)ci[0]
            });


            var nationalSocieties = new List<NationalSociety>
            {
                new NationalSociety
                {
                    Id = _nationalSocietyId1,
                    Name = "Test national society 1"
                },
                new NationalSociety
                {
                    Id = _nationalSocietyId2,
                    Name = "Test national society 2"
                }
            };

            var organizations = new List<Organization>
            {
                new Organization
                {
                    Id = 1,
                    Name = "RC",
                    NationalSociety = nationalSocieties[0],
                    NationalSocietyId = _nationalSocietyId1
                }
            };

            var projects = new List<Project>
            {
                new Project
                {
                    Id = _projectId1,
                    NationalSociety = nationalSocieties[0],
                    Name = "project 1",
                    State = ProjectState.Open,
                    AlertNotificationRecipients = new List<AlertNotificationRecipient>()
                },
                new Project
                {
                    Id = _projectId2,
                    NationalSociety = nationalSocieties[0],
                    Name = "project 2",
                    State = ProjectState.Open,
                    AlertNotificationRecipients = new List<AlertNotificationRecipient>()
                },
                new Project
                {
                    Id = _projectId3,
                    NationalSociety = nationalSocieties[0],
                    Name = "project 3",
                    State = ProjectState.Closed,
                    AlertNotificationRecipients = new List<AlertNotificationRecipient>()
                },
                new Project
                {
                    Id = _projectId4,
                    NationalSociety = nationalSocieties[1],
                    Name = "project 4",
                    State = ProjectState.Open,
                    AlertNotificationRecipients = new List<AlertNotificationRecipient>()
                },
                new Project
                {
                    Id = _projectId5,
                    NationalSociety = nationalSocieties[1],
                    Name = "project 5",
                    State = ProjectState.Open,
                    AlertNotificationRecipients = new List<AlertNotificationRecipient>()
                }
            };

            var users = new List<User>
            {
                new AdministratorUser
                {
                    Id = _administratorId,
                    Role = Role.Administrator
                },
                new HeadSupervisorUser
                {
                    Id = _headSupervisorWithSupervisors,
                    Role = Role.HeadSupervisor,
                    EmailAddress = "emailTest1@domain.com",
                    Name = "emailTest1@domain.com",
                    PhoneNumber = "123",
                    AdditionalPhoneNumber = "321",
                    Sex = Sex.Male,
                    DecadeOfBirth = 1990
                },
                new HeadSupervisorUser
                {
                    Id = _headSupervisorWithoutSupervisors,
                    Role = Role.HeadSupervisor,
                    EmailAddress = "emailTest2@domain.com",
                    Name = "emailTest1@domain.com",
                    PhoneNumber = "123456",
                    AdditionalPhoneNumber = "321",
                    Sex = Sex.Male,
                    DecadeOfBirth = 1990
                },
                new SupervisorUser
                {
                    Id = _supervisorId,
                    Role = Role.Supervisor,
                    Sex = Sex.Female,
                    DecadeOfBirth = 1990
                }
            };

            var headSupervisorWithSupervisors = (HeadSupervisorUser)users[1];
            var headSupervisorWithoutSupervisors = (HeadSupervisorUser)users[2];
            var supervisor = (SupervisorUser)users[3];
            supervisor.HeadSupervisor = headSupervisorWithSupervisors;

            var headSupervisorUserProjects = new List<HeadSupervisorUserProject>
            {
                new HeadSupervisorUserProject
                {
                    Project = projects[0],
                    ProjectId = _projectId1,
                    HeadSupervisorUser = headSupervisorWithSupervisors,
                    HeadSupervisorUserId = _headSupervisorWithSupervisors
                },
                new HeadSupervisorUserProject
                {
                    Project = projects[2],
                    ProjectId = _projectId3,
                    HeadSupervisorUser = headSupervisorWithSupervisors,
                    HeadSupervisorUserId = _headSupervisorWithSupervisors
                },
                new HeadSupervisorUserProject
                {
                    Project = projects[1],
                    ProjectId = _projectId3,
                    HeadSupervisorUser = headSupervisorWithoutSupervisors,
                    HeadSupervisorUserId = _headSupervisorWithoutSupervisors
                }
            };
            headSupervisorWithSupervisors.HeadSupervisorUserProjects = new List<HeadSupervisorUserProject>
            {
                headSupervisorUserProjects[0],
                headSupervisorUserProjects[1]
            };
            headSupervisorWithoutSupervisors.HeadSupervisorUserProjects = new List<HeadSupervisorUserProject> { headSupervisorUserProjects[2] };

            headSupervisorWithSupervisors.CurrentProject = headSupervisorWithSupervisors.HeadSupervisorUserProjects.Single(p => p.Project.State == ProjectState.Open).Project;
            headSupervisorWithoutSupervisors.CurrentProject = headSupervisorWithoutSupervisors.HeadSupervisorUserProjects.Single(p => p.Project.State == ProjectState.Open).Project;

            var userNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety
                {
                    NationalSociety = nationalSocieties[0],
                    NationalSocietyId = _nationalSocietyId1,
                    User = headSupervisorWithSupervisors,
                    UserId = _headSupervisorWithSupervisors,
                    Organization = organizations[0],
                    OrganizationId = organizations[0].Id
                },
                new UserNationalSociety
                {
                    NationalSociety = nationalSocieties[0],
                    NationalSocietyId = _nationalSocietyId1,
                    User = headSupervisorWithoutSupervisors,
                    UserId = _headSupervisorWithoutSupervisors,
                    Organization = organizations[0],
                    OrganizationId = organizations[0].Id
                },
                new UserNationalSociety
                {
                    NationalSociety = nationalSocieties[0],
                    NationalSocietyId = _nationalSocietyId1,
                    User = supervisor,
                    UserId = _supervisorId,
                    Organization = organizations[0],
                    OrganizationId = organizations[0].Id
                }
            };

            nationalSocieties[0].NationalSocietyUsers = userNationalSocieties;
            headSupervisorWithSupervisors.HeadSupervisorUserAlertRecipients = new List<HeadSupervisorUserAlertRecipient>();
            headSupervisorWithSupervisors.UserNationalSocieties = new List<UserNationalSociety> { userNationalSocieties[0] };
            headSupervisorWithoutSupervisors.UserNationalSocieties = new List<UserNationalSociety> { userNationalSocieties[1] };
            headSupervisorWithoutSupervisors.HeadSupervisorUserAlertRecipients = new List<HeadSupervisorUserAlertRecipient>();

            var dataCollectors = new List<DataCollector>
            {
                new DataCollector
                {
                    Id = _dataCollectorId,
                    Supervisor = supervisor
                },
                new DataCollector
                {
                    Id = _deletedDataCollectorId,
                    Supervisor = supervisor,
                    DeletedAt = new DateTime(2020, 01, 01)
                }
            };

            var headSupervisorUserAlertRecipients = new List<HeadSupervisorUserAlertRecipient>();
            var gatewayModems = new List<GatewayModem>();


            var headSupervisorUserProjectsDbSet = headSupervisorUserProjects.AsQueryable().BuildMockDbSet();
            var headSupervisorAlertRecipientsDbSet = headSupervisorUserAlertRecipients.AsQueryable().BuildMockDbSet();
            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            var projectsDbSet = projects.AsQueryable().BuildMockDbSet();
            var usersDbSet = users.AsQueryable().BuildMockDbSet();
            var userNationalSocietiesDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            var dataCollectorsDbSet = dataCollectors.AsQueryable().BuildMockDbSet();
            var organizationsDbSet = organizations.AsQueryable().BuildMockDbSet();
            var gatewayModemsDbSet = gatewayModems.AsQueryable().BuildMockDbSet();

            _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);
            _nyssContext.Projects.Returns(projectsDbSet);
            _nyssContext.Users.Returns(usersDbSet);
            _nyssContext.HeadSupervisorUserProjects.Returns(headSupervisorUserProjectsDbSet);
            _nyssContext.UserNationalSocieties.Returns(userNationalSocietiesDbSet);
            _nyssContext.DataCollectors.Returns(dataCollectorsDbSet);
            _nyssContext.HeadSupervisorUserAlertRecipients.Returns(headSupervisorAlertRecipientsDbSet);
            _nyssContext.Organizations.Returns(organizationsDbSet);
            _nyssContext.GatewayModems.Returns(gatewayModemsDbSet);


            _nyssContext.NationalSocieties.FindAsync(1).Returns(nationalSocieties[0]);
            _nyssContext.NationalSocieties.FindAsync(2).Returns(nationalSocieties[1]);
        }


        [Fact]
        public async Task Create_WhenIdentityUserCreationSuccessful_ShouldReturnSuccessResult()
        {
            var userEmail = "emailTest1@domain.com";
            var userName = "Mickey Mouse";
            var createHeadSupervisorRequestDto = new CreateHeadSupervisorRequestDto
            {
                Name = userName,
                Email = userEmail,
                OrganizationId = 1
            };

            var nationalSocietyId = 1;
            var result = await _headSupervisorService.Create(nationalSocietyId, createHeadSupervisorRequestDto);

            await _identityUserRegistrationServiceMock.Received(1).GenerateEmailVerification(userEmail);
            await _verificationEmailServiceMock.Received(1).SendVerificationEmail(Arg.Is<User>(u => u.EmailAddress == userEmail), Arg.Any<string>());
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task Create_WhenIdentityUserCreationSuccessful_NyssContextAddIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var createHeadSupervisorRequestDto = new CreateHeadSupervisorRequestDto
            {
                Name = userEmail,
                Email = userEmail,
                OrganizationId = 1
            };

            var nationalSocietyId = 1;
            var result = await _headSupervisorService.Create(nationalSocietyId, createHeadSupervisorRequestDto);


            await _nyssContext.Received().AddAsync(Arg.Any<UserNationalSociety>());
        }

        [Fact]
        public async Task Create_WhenIdentityUserCreationSuccessful_NyssContextSaveChangesIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var createHeadSupervisorRequestDto = new CreateHeadSupervisorRequestDto
            {
                Name = userEmail,
                Email = userEmail,
                OrganizationId = 1
            };

            var nationalSocietyId = 1;
            var result = await _headSupervisorService.Create(nationalSocietyId, createHeadSupervisorRequestDto);


            await _nyssContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task Create_WhenIdentityUserServiceThrowsResultException_ShouldReturnErrorResultWithAppropriateKey()
        {
            var exception = new ResultException(ResultKey.User.Registration.UserAlreadyExists);
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw exception);

            var userEmail = "emailTest1@domain.com";
            var createHeadSupervisorRequestDto = new CreateHeadSupervisorRequestDto
            {
                Name = userEmail,
                Email = userEmail,
                OrganizationId = 1
            };

            var nationalSocietyId = 1;
            var result = await _headSupervisorService.Create(nationalSocietyId, createHeadSupervisorRequestDto);


            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(exception.Result.Message.Key);
        }

        [Fact]
        public void Create_WhenNonResultExceptionIsThrown_ShouldPassThroughWithoutBeingCaught()
        {
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw new Exception());

            var userEmail = "emailTest1@domain.com";
            var createHeadSupervisorRequestDto = new CreateHeadSupervisorRequestDto
            {
                Name = userEmail,
                Email = userEmail,
                OrganizationId = 1
            };

            var nationalSocietyId = 1;
            _headSupervisorService.Create(nationalSocietyId, createHeadSupervisorRequestDto).ShouldThrowAsync<Exception>();
        }

        [Fact]
        public async Task Create_WhenCreatingInNonExistentNationalSociety_ShouldReturnError()
        {
            //Arrange
            var userEmail = "emailTest1@domain.com";
            var createHeadSupervisorRequestDto = new CreateHeadSupervisorRequestDto
            {
                Name = userEmail,
                Email = userEmail
            };

            //Act
            var nationalSocietyId = 666;
            var result = await _headSupervisorService.Create(nationalSocietyId, createHeadSupervisorRequestDto);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Registration.NationalSocietyDoesNotExist);
        }

        [Fact]
        public async Task Create_WhenCreatingHeadSupervisorWithProjectSpecified_AddAsyncForProjectReferenceShouldBeCalledOnce()
        {
            //Arrange
            var userEmail = "emailTest1@domain.com";
            var createHeadSupervisorRequestDto = new CreateHeadSupervisorRequestDto
            {
                Name = userEmail,
                Email = userEmail,
                ProjectId = 1,
                OrganizationId = 1
            };

            //Act
            var nationalSocietyId = 1;
            var result = await _headSupervisorService.Create(nationalSocietyId, createHeadSupervisorRequestDto);

            //Assert
            await _nyssContext.Received(1).AddAsync(Arg.Any<HeadSupervisorUserProject>());
        }

        [Fact]
        public async Task Create_WhenCreatingHeadSupervisorWithNoProjectSpecified_AddAsyncForProjectReferenceShouldNotBeCalled()
        {
            //Arrange
            var userEmail = "emailTest1@domain.com";
            var createHeadSupervisorRequestDto = new CreateHeadSupervisorRequestDto
            {
                Name = userEmail,
                Email = userEmail,
                OrganizationId = 1
            };

            //Act
            var result = await _headSupervisorService.Create(_nationalSocietyId1, createHeadSupervisorRequestDto);

            //Assert
            await _nyssContext.Received(0).AddAsync(Arg.Any<HeadSupervisorUserProject>());
        }

        [Fact]
        public async Task Create_WhenCreatingHeadSupervisorWithProjectThatDoesntExist_ShouldReturnError()
        {
            //Arrange
            var userEmail = "emailTest1@domain.com";
            var createHeadSupervisorRequestDto = new CreateHeadSupervisorRequestDto
            {
                Name = userEmail,
                Email = userEmail,
                ProjectId = 666,
            };

            //Act
            var result = await _headSupervisorService.Create(_nationalSocietyId1, createHeadSupervisorRequestDto);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Supervisor.ProjectDoesNotExistOrNoAccess);
        }

        [Fact]
        public async Task Create_WhenCreatingHeadSupervisorWithProjectInAnotherNationalSociety_ShouldReturnError()
        {
            //Arrange
            var userEmail = "emailTest1@domain.com";
            var createHeadSupervisorRequestDto = new CreateHeadSupervisorRequestDto
            {
                Name = userEmail,
                Email = userEmail,
                ProjectId = _projectId5,
            };

            //Act
            var result = await _headSupervisorService.Create(_nationalSocietyId1, createHeadSupervisorRequestDto);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Supervisor.ProjectDoesNotExistOrNoAccess);
        }


        [Fact]
        public async Task Edit_WhenEditingNonExistingUser_ReturnsErrorResult()
        {
            var result = await _headSupervisorService.Edit(999, new EditHeadSupervisorRequestDto());

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task Edit_WhenEditingUserThatIsNotHeadSupervisor_ReturnsErrorResult()
        {
            var result = await _headSupervisorService.Edit(_administratorId, new EditHeadSupervisorRequestDto());

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task Edit_WhenEditingUserThatIsNotHeadSupervisor_SaveChangesShouldNotBeCalled()
        {
            await _headSupervisorService.Edit(_administratorId, new EditHeadSupervisorRequestDto());

            await _nyssContext.DidNotReceive().SaveChangesAsync();
        }


        [Fact]
        public async Task Edit_WhenEditingExistingHeadSupervisor_ReturnsSuccess()
        {
            var result = await _headSupervisorService.Edit(_headSupervisorWithSupervisors, new EditHeadSupervisorRequestDto());

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task Edit_WhenEditingExistingHeadSupervisor_SaveChangesAsyncIsCalled()
        {
            await _headSupervisorService.Edit(_headSupervisorWithSupervisors, new EditHeadSupervisorRequestDto());

            await _nyssContext.Received().SaveChangesAsync();
        }


        [Fact]
        public async Task Edit_WhenEditingExistingUser_ExpectedFieldsGetEdited()
        {
            var existingUserEmail = _nyssContext.Users.Single(u => u.Id == _headSupervisorWithSupervisors)?.EmailAddress;

            var editRequest = new EditHeadSupervisorRequestDto
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123"
            };

            await _headSupervisorService.Edit(_headSupervisorWithSupervisors, editRequest);

            var editedUser = _nyssContext.Users.Single(u => u.Id == _headSupervisorWithSupervisors) as HeadSupervisorUser;

            editedUser.ShouldNotBeNull();
            editedUser.Name.ShouldBe(editRequest.Name);
            editedUser.PhoneNumber.ShouldBe(editRequest.PhoneNumber);
            editedUser.EmailAddress.ShouldBe(existingUserEmail);
            editedUser.DecadeOfBirth.ShouldBe(editRequest.DecadeOfBirth);
            editedUser.Sex.ShouldBe(editRequest.Sex);
        }

        [Fact]
        public async Task Edit_WhenRemovingProjectReference_NyssContextRemoveProjectIsCalledOnce()
        {
            //Arrange
            var editRequest = new EditHeadSupervisorRequestDto
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123",
                ProjectId = null
            };

            //Act
            await _headSupervisorService.Edit(_headSupervisorWithSupervisors, editRequest);

            //Assert
            _nyssContext.HeadSupervisorUserProjects.Received(1).Remove(Arg.Any<HeadSupervisorUserProject>());
        }

        [Fact]
        public async Task Edit_WhenSwitchingProject_NyssContextRemoveAndAddProjectEachAreCalledOnce()
        {
            //Arrange
            var editRequest = new EditHeadSupervisorRequestDto
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123",
                ProjectId = 2
            };
            var projectReferenceToBeRemoved = _nyssContext.HeadSupervisorUserProjects.Single(x => x.ProjectId == _projectId1 && x.HeadSupervisorUserId == _headSupervisorWithSupervisors);


            //Act
            await _headSupervisorService.Edit(_headSupervisorWithSupervisors, editRequest);

            //Assert
            _nyssContext.HeadSupervisorUserProjects.Received(1).Remove(projectReferenceToBeRemoved);
            await _nyssContext.Received(1).AddAsync(Arg.Any<HeadSupervisorUserProject>());
        }


        [Fact]
        public async Task Edit_WhenSwitchingProjectToTheSameProject_NyssContextRemoveAndAddProjectShouldNotBeCalled()
        {
            //Arrange
            var editRequest = new EditHeadSupervisorRequestDto
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123",
                ProjectId = 1
            };

            //Act
            await _headSupervisorService.Edit(_headSupervisorWithSupervisors, editRequest);

            //Assert
            _nyssContext.HeadSupervisorUserProjects.Received(0).Remove(Arg.Any<HeadSupervisorUserProject>());
            await _nyssContext.Received(0).AddAsync(Arg.Any<HeadSupervisorUserProject>());
        }


        [Fact]
        public async Task Edit_WhenSwitchingProjectToOneFromOtherNationalSociety_ShouldReturnError()
        {
            //Arrange
            var editRequest = new EditHeadSupervisorRequestDto
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123",
                ProjectId = 4,
            };

            //Act
            var result = await _headSupervisorService.Edit(_headSupervisorWithSupervisors, editRequest);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Supervisor.ProjectDoesNotExistOrNoAccess);
        }

        [Fact]
        public async Task Delete_WhenRemovingNonExistentHeadSupervisor_ReturnsError()
        {
            //Act
            var result = await _headSupervisorService.Delete(999);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Common.UserNotFound);
        }


        [Fact]
        public async Task Get_IfHeadSupervisorExists_ReturnSupervisor()
        {
            _authorizationServiceMock.GetCurrentUser().Returns(new AdministratorUser());

            //Act
            var result = await _headSupervisorService.Get(_headSupervisorWithSupervisors, _nationalSocietyId1);

            //Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldBe(_headSupervisorWithSupervisors);
            result.Value.Role.ShouldBe(Role.HeadSupervisor);
            result.Value.Email.ShouldBe("emailTest1@domain.com");
            result.Value.Name.ShouldBe("emailTest1@domain.com");
            result.Value.PhoneNumber.ShouldBe("123");
            result.Value.AdditionalPhoneNumber.ShouldBe("321");
            result.Value.Sex.ShouldBe(Sex.Male);
            result.Value.DecadeOfBirth.ShouldBe(1990);
        }


        [Fact]
        public async Task Get_IfHeadSupervisorDoesntExists_ReturnError()
        {
            //Act
            var result = await _headSupervisorService.Get(999, _nationalSocietyId1);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Common.UserNotFound);
        }

        [Fact]
        public async Task Delete_WhenDeleting_EnsureCanDeleteUserIsCalled()
        {
            //act
            await _headSupervisorService.Delete(_headSupervisorWithoutSupervisors);

            //assert
            await _deleteUserService.Received().EnsureCanDeleteUser(_headSupervisorWithoutSupervisors, Role.HeadSupervisor);
        }

        [Fact]
        public async Task Delete_WhenDeletingHeadSupervisorWithSupervisors_ReturnsError()
        {
            //act
            var result = await _headSupervisorService.Delete(_headSupervisorWithSupervisors);

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Deletion.CannotDeleteHeadSupervisorHasSupervisors);
        }

        [Fact]
        public async Task Delete_WhenDeletingHeadSupervisor_ProjectReferenceShouldBeRemoved()
        {
            //arrange
            var supervisorBeingDeleted = _nyssContext.Users.FilterAvailable().Single(u => u.Id == _headSupervisorWithoutSupervisors);

            //act
            var result = await _headSupervisorService.Delete(_headSupervisorWithoutSupervisors);

            //assert
            _nyssContext.HeadSupervisorUserProjects.Received(1).Remove(Arg.Any<HeadSupervisorUserProject>());
            result.IsSuccess.ShouldBeTrue();
        }
    }
}
