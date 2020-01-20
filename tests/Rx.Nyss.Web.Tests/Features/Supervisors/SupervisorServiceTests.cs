using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Supervisors;
using RX.Nyss.Web.Features.Supervisors.Dto;
using RX.Nyss.Web.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Supervisors
{
    public class SupervisorServiceTests
    {
        private readonly SupervisorService _supervisorService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationServiceMock;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailServiceMock;
        private IDeleteUserService _deleteUserService;

        private readonly int _nationalSocietyId1 = 1;
        private readonly int _nationalSocietyId2 = 2;
        private readonly int _projectId1 = 1;
        private readonly int _projectId2 = 2;
        private readonly int _projectId3 = 3;
        private readonly int _projectId4 = 4;
        private readonly int _projectId5 = 5;
        private readonly int _administratorId = 1;
        private readonly int _supervisorWithDataCollectorsId = 2;
        private readonly int _supervisorWithoutDataCollectorsId = 3;
        private readonly int _supervisorWithDeletedDataCollectorsId = 4;
        private readonly int _dataCollectorId = 1;
        private readonly int _deletedDataCollectorId = 2;


        public SupervisorServiceTests()
        {
            _loggerAdapter = Substitute.For<ILoggerAdapter>();
            _nyssContext = Substitute.For<INyssContext>();
            _identityUserRegistrationServiceMock = Substitute.For<IIdentityUserRegistrationService>();
            _verificationEmailServiceMock = Substitute.For<IVerificationEmailService>();
            _nationalSocietyUserService = Substitute.For<INationalSocietyUserService>();
            _deleteUserService = Substitute.For<IDeleteUserService>();

            var applicationLanguages = new List<ApplicationLanguage>();
            var applicationLanguagesDbSet = applicationLanguages.AsQueryable().BuildMockDbSet();
            _nyssContext.ApplicationLanguages.Returns(applicationLanguagesDbSet);

            _supervisorService = new SupervisorService(_identityUserRegistrationServiceMock, _nationalSocietyUserService, _nyssContext, _loggerAdapter, _verificationEmailServiceMock, _deleteUserService);

            _identityUserRegistrationServiceMock.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()).Returns(ci => new IdentityUser { Id = "123", Email = (string)ci[0] });


            var nationalSocieties = new List<NationalSociety>
            {
                new NationalSociety { Id = _nationalSocietyId1, Name = "Test national society 1" },
                new NationalSociety { Id = _nationalSocietyId2, Name = "Test national society 2" }
            };

            var projects = new List<Project>
            {
                new Project
                {
                    Id = _projectId1, NationalSociety = nationalSocieties[0], Name = "project 1", State = ProjectState.Open
                },
                new Project
                {
                    Id = _projectId2, NationalSociety = nationalSocieties[0], Name = "project 2", State = ProjectState.Open
                },
                new Project
                {
                    Id = _projectId3, NationalSociety = nationalSocieties[0], Name = "project 3", State = ProjectState.Closed
                },
                new Project
                {
                    Id = _projectId4, NationalSociety = nationalSocieties[1], Name = "project 4", State = ProjectState.Open
                },
                new Project
                {
                    Id = _projectId5, NationalSociety = nationalSocieties[1], Name = "project 5", State = ProjectState.Open
                }
            };

            var users = new List<User>
            {
                new AdministratorUser { Id = _administratorId, Role = Role.Administrator },
                new SupervisorUser
                {
                    Id = _supervisorWithDataCollectorsId,
                    Role = Role.Supervisor,
                    EmailAddress = "emailTest1@domain.com",
                    Name = "emailTest1@domain.com",
                    PhoneNumber = "123",
                    AdditionalPhoneNumber = "321",
                    Sex = Sex.Male,
                    DecadeOfBirth = 1990
                },
                new SupervisorUser
                {
                    Id = _supervisorWithoutDataCollectorsId,
                    Role = Role.Supervisor,
                    EmailAddress = "emailTest2@domain.com",
                    Name = "emailTest1@domain.com",
                    PhoneNumber = "123456",
                    AdditionalPhoneNumber = "321",
                    Sex = Sex.Male,
                    DecadeOfBirth = 1990
                },
                new SupervisorUser
                {
                    Id = _supervisorWithDeletedDataCollectorsId,
                    Role = Role.Supervisor,
                    EmailAddress = "emailTest2@domain.com",
                    Name = "emailTest1@domain.com",
                    PhoneNumber = "123456",
                    AdditionalPhoneNumber = "321",
                    Sex = Sex.Male,
                    DecadeOfBirth = 1990
                }
            };

            var supervisorWithDataCollectors = (SupervisorUser)users[1];
            var supervisorWithoutDataCollectors = (SupervisorUser)users[2];
            var supervisorWithDeletedDataCollectors = (SupervisorUser)users[3];
            var supervisorUserProjects = new List<SupervisorUserProject>
            {
                new SupervisorUserProject { Project = projects[0], ProjectId = _projectId1, SupervisorUser = supervisorWithDataCollectors, SupervisorUserId = _supervisorWithDataCollectorsId },
                new SupervisorUserProject { Project = projects[2], ProjectId = _projectId3, SupervisorUser = supervisorWithDataCollectors, SupervisorUserId = _supervisorWithDataCollectorsId },
                new SupervisorUserProject { Project = projects[2], ProjectId = _projectId3, SupervisorUser = supervisorWithoutDataCollectors, SupervisorUserId = _supervisorWithoutDataCollectorsId },
                new SupervisorUserProject { Project = projects[0], ProjectId = _projectId1, SupervisorUser = supervisorWithDeletedDataCollectors, SupervisorUserId = _supervisorWithDeletedDataCollectorsId }
            };
            supervisorWithDataCollectors.SupervisorUserProjects = new List<SupervisorUserProject> { supervisorUserProjects[0], supervisorUserProjects[1] };
            supervisorWithoutDataCollectors.SupervisorUserProjects = new List<SupervisorUserProject> { supervisorUserProjects[2] };
            supervisorWithDeletedDataCollectors.SupervisorUserProjects = new List<SupervisorUserProject> { supervisorUserProjects[3] };

            var userNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety { NationalSociety = nationalSocieties[0], NationalSocietyId = _nationalSocietyId1, User =  supervisorWithDataCollectors, UserId = _supervisorWithDataCollectorsId},
                new UserNationalSociety { NationalSociety = nationalSocieties[0], NationalSocietyId = _nationalSocietyId1, User =  supervisorWithoutDataCollectors, UserId = _supervisorWithoutDataCollectorsId},
                new UserNationalSociety { NationalSociety = nationalSocieties[0], NationalSocietyId = _nationalSocietyId1, User =  supervisorWithDeletedDataCollectors, UserId = _supervisorWithDeletedDataCollectorsId}
            };
            supervisorWithDataCollectors.UserNationalSocieties = new List<UserNationalSociety> { userNationalSocieties[0] };
            supervisorWithoutDataCollectors.UserNationalSocieties = new List<UserNationalSociety> { userNationalSocieties[1] };
            supervisorWithDeletedDataCollectors.UserNationalSocieties = new List<UserNationalSociety> { userNationalSocieties[2] };

            var dataCollectors = new List<DataCollector>
            {
                new DataCollector
                {
                    Id = _dataCollectorId,
                    Supervisor = supervisorWithDataCollectors
                },
                new DataCollector
                {
                    Id = _deletedDataCollectorId,
                    Supervisor = supervisorWithDeletedDataCollectors,
                    DeletedAt = new DateTime(2020,01,01)
                }
            };



            var supervisorUserProjectsDbSet = supervisorUserProjects.AsQueryable().BuildMockDbSet();
            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            var projectsDbSet = projects.AsQueryable().BuildMockDbSet();
            var usersDbSet = users.AsQueryable().BuildMockDbSet();
            var userNationalSocietiesDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            var dataCollectorsDbSet = dataCollectors.AsQueryable().BuildMockDbSet();

            _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);
            _nyssContext.Projects.Returns(projectsDbSet);
            _nyssContext.Users.Returns(usersDbSet);
            _nyssContext.SupervisorUserProjects.Returns(supervisorUserProjectsDbSet);
            _nyssContext.UserNationalSocieties.Returns(userNationalSocietiesDbSet);
            _nyssContext.DataCollectors.Returns(dataCollectorsDbSet);


            _nyssContext.NationalSocieties.FindAsync(1).Returns(nationalSocieties[0]);
            _nyssContext.NationalSocieties.FindAsync(2).Returns(nationalSocieties[1]);


            _nationalSocietyUserService.GetNationalSocietyUser<SupervisorUser>(Arg.Any<int>()).Returns(ci =>
            {
                var user = users.OfType<SupervisorUser>().FirstOrDefault(x => x.Id == (int)ci[0]);
                if (user == null)
                {
                    throw new ResultException(ResultKey.User.Registration.UserNotFound);
                }
                return user;
            });
        }


        [Fact]
        public async Task Create_WhenIdentityUserCreationSuccessful_ShouldReturnSuccessResult()
        {
            var userEmail = "emailTest1@domain.com";
            var userName = "Mickey Mouse";
            var registerSupervisorRequestDto = new CreateSupervisorRequestDto { Name = userName, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _supervisorService.Create(nationalSocietyId, registerSupervisorRequestDto);

            await _identityUserRegistrationServiceMock.Received(1).GenerateEmailVerification(userEmail);
            await _verificationEmailServiceMock.Received(1).SendVerificationEmail(Arg.Is<User>(u => u.EmailAddress == userEmail), Arg.Any<string>());
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task Create_WhenIdentityUserCreationSuccessful_NyssContextAddIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerSupervisorRequestDto = new CreateSupervisorRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _supervisorService.Create(nationalSocietyId, registerSupervisorRequestDto);


            await _nyssContext.Received().AddAsync(Arg.Any<UserNationalSociety>());
        }

        [Fact]
        public async Task Create_WhenIdentityUserCreationSuccessful_NyssContextSaveChangesIsCalledOnce()
        {
            var userEmail = "emailTest1@domain.com";
            var registerSupervisorRequestDto = new CreateSupervisorRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _supervisorService.Create(nationalSocietyId, registerSupervisorRequestDto);


            await _nyssContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task Create_WhenIdentityUserServiceThrowsResultException_ShouldReturnErrorResultWithAppropriateKey()
        {
            var exception = new ResultException(ResultKey.User.Registration.UserAlreadyExists);
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw exception);

            var userEmail = "emailTest1@domain.com";
            var registerSupervisorRequestDto = new CreateSupervisorRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            var result = await _supervisorService.Create(nationalSocietyId, registerSupervisorRequestDto);


            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(exception.Result.Message.Key);
        }

        [Fact]
        public void Create_WhenNonResultExceptionIsThrown_ShouldPassThroughWithoutBeingCaught()
        {
            _identityUserRegistrationServiceMock.When(ius => ius.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()))
                .Do(x => throw new Exception());

            var userEmail = "emailTest1@domain.com";
            var registerSupervisorRequestDto = new CreateSupervisorRequestDto { Name = userEmail, Email = userEmail };

            var nationalSocietyId = 1;
            _supervisorService.Create(nationalSocietyId, registerSupervisorRequestDto).ShouldThrowAsync<Exception>();
        }

        [Fact]
        public async Task Create_WhenCreatingInNonExistentNationalSociety_ShouldReturnError()
        {
            //Arrange
            var userEmail = "emailTest1@domain.com";
            var registerSupervisorRequestDto = new CreateSupervisorRequestDto { Name = userEmail, Email = userEmail };

            //Act
            var nationalSocietyId = 666;
            var result = await _supervisorService.Create(nationalSocietyId, registerSupervisorRequestDto);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Registration.NationalSocietyDoesNotExist);
        }

        [Fact]
        public async Task Create_WhenCreatingSupervisorWithProjectSpecified_AddAsyncForProjectReferenceShouldBeCalledOnce()
        {
            //Arrange
            var userEmail = "emailTest1@domain.com";
            var registerSupervisorRequestDto = new CreateSupervisorRequestDto { Name = userEmail, Email = userEmail, ProjectId = 1 };

            //Act
            var nationalSocietyId = 1;
            var result = await _supervisorService.Create(nationalSocietyId, registerSupervisorRequestDto);

            //Assert
            await _nyssContext.Received(1).AddAsync(Arg.Any<SupervisorUserProject>());
        }

        [Fact]
        public async Task Create_WhenCreatingSupervisorWithNoProjectSpecified_AddAsyncForProjectReferenceShouldNotBeCalled()
        {
            //Arrange
            var userEmail = "emailTest1@domain.com";
            var registerSupervisorRequestDto = new CreateSupervisorRequestDto { Name = userEmail, Email = userEmail };

            //Act
            var result = await _supervisorService.Create(_nationalSocietyId1, registerSupervisorRequestDto);

            //Assert
            await _nyssContext.Received(0).AddAsync(Arg.Any<SupervisorUserProject>());
        }

        [Fact]
        public async Task Create_WhenCreatingSupervisorWithProjectThatDoesntExist_ShouldReturnError()
        {
            //Arrange
            var userEmail = "emailTest1@domain.com";
            var registerSupervisorRequestDto = new CreateSupervisorRequestDto { Name = userEmail, Email = userEmail, ProjectId = 666};

            //Act
            var result = await _supervisorService.Create(_nationalSocietyId1, registerSupervisorRequestDto);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Supervisor.ProjectDoesNotExistOrNoAccess);
        }

        [Fact]
        public async Task Create_WhenCreatingSupervisorWithProjectInAnotherNationalSociety_ShouldReturnError()
        {
            //Arrange
            var userEmail = "emailTest1@domain.com";
            var registerSupervisorRequestDto = new CreateSupervisorRequestDto { Name = userEmail, Email = userEmail, ProjectId = _projectId5 };

            //Act
            var result = await _supervisorService.Create(_nationalSocietyId1, registerSupervisorRequestDto);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Supervisor.ProjectDoesNotExistOrNoAccess);
        }


        [Fact]
        public async Task Edit_WhenEditingNonExistingUser_ReturnsErrorResult()
        {
            var result = await _supervisorService.Edit(999, new EditSupervisorRequestDto());

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task Edit_WhenEditingUserThatIsNotSupervisor_ReturnsErrorResult()
        {
            var result = await _supervisorService.Edit(_administratorId, new EditSupervisorRequestDto());

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task Edit_WhenEditingUserThatIsNotSupervisor_SaveChangesShouldNotBeCalled()
        {
            await _supervisorService.Edit(_administratorId, new EditSupervisorRequestDto());

            await _nyssContext.DidNotReceive().SaveChangesAsync();
        }


        [Fact]
        public async Task Edit_WhenEditingExistingSupervisor_ReturnsSuccess()
        {
            var result = await _supervisorService.Edit(_supervisorWithDataCollectorsId, new EditSupervisorRequestDto());

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task Edit_WhenEditingExistingSupervisor_SaveChangesAsyncIsCalled()
        {
            await _supervisorService.Edit(_supervisorWithDataCollectorsId, new EditSupervisorRequestDto());

            await _nyssContext.Received().SaveChangesAsync();
        }


        [Fact]
        public async Task Edit_WhenEditingExistingUser_ExpectedFieldsGetEdited()
        {
            var existingUserEmail = _nyssContext.Users.Single(u => u.Id == _supervisorWithDataCollectorsId)?.EmailAddress;

            var editRequest = new EditSupervisorRequestDto
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123"
            };

            await _supervisorService.Edit(_supervisorWithDataCollectorsId, editRequest);

            var editedUser = _nyssContext.Users.Single(u => u.Id == _supervisorWithDataCollectorsId) as SupervisorUser;

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
            var editRequest = new EditSupervisorRequestDto
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123",
                ProjectId = null
            };

            //Act
            await _supervisorService.Edit(_supervisorWithDataCollectorsId, editRequest);

            //Assert
            _nyssContext.SupervisorUserProjects.Received(1).Remove(Arg.Any<SupervisorUserProject>());
        }

        [Fact]
        public async Task Edit_WhenSwitchingProject_NyssContextRemoveAndAddProjectEachAreCalledOnce()
        {
            //Arrange
            var editRequest = new EditSupervisorRequestDto
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123",
                ProjectId = 2
            };
            var projectReferenceToBeRemoved = _nyssContext.SupervisorUserProjects.Single(x => x.ProjectId == _projectId1 && x.SupervisorUserId == _supervisorWithDataCollectorsId);


            //Act
            await _supervisorService.Edit(_supervisorWithDataCollectorsId, editRequest);

            //Assert
            _nyssContext.SupervisorUserProjects.Received(1).Remove(projectReferenceToBeRemoved);
            await _nyssContext.Received(1).AddAsync(Arg.Any<SupervisorUserProject>());
        }


        [Fact]
        public async Task Edit_WhenSwitchingProjectToTheSameProject_NyssContextRemoveAndAddProjectShouldNotBeCalled()
        {
            //Arrange
            var editRequest = new EditSupervisorRequestDto
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123",
                ProjectId = 1
            };

            //Act
            await _supervisorService.Edit(_supervisorWithDataCollectorsId, editRequest);

            //Assert
            _nyssContext.SupervisorUserProjects.Received(0).Remove(Arg.Any<SupervisorUserProject>());
            await _nyssContext.Received(0).AddAsync(Arg.Any<SupervisorUserProject>());
        }


        [Fact]
        public async Task Edit_WhenSwitchingProjectToOneFromOtherNationalSociety_ShouldReturnError()
        {
            //Arrange
            var editRequest = new EditSupervisorRequestDto
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123",
                ProjectId = 4
            };

            //Act
            var result = await _supervisorService.Edit(_supervisorWithDataCollectorsId, editRequest);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Supervisor.ProjectDoesNotExistOrNoAccess);
        }


        [Fact]
        public async Task Remove_WhenSupervisorHasSomeProjectReferencesAndNoDataCollectors_RemoveRangeIsCalled()
        {
            //Act
            var result = await _supervisorService.Remove(_supervisorWithoutDataCollectorsId);

            //Assert
            _nyssContext.SupervisorUserProjects.Received(1).RemoveRange(Arg.Any<List<SupervisorUserProject>>());
        }

        [Fact]
        public async Task Remove_WhenRemovingNonExistentSupervisor_ReturnsError()
        {
            //Act
            var result = await _supervisorService.Remove(999);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Common.UserNotFound);
        }


        [Fact]
        public async Task Get_IfSupervisorExists_ReturnSupervisor()
        {
            //Act
            var result = await _supervisorService.Get(_supervisorWithDataCollectorsId);

            //Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldBe(_supervisorWithDataCollectorsId);
            result.Value.Role.ShouldBe(Role.Supervisor);
            result.Value.Email.ShouldBe("emailTest1@domain.com");
            result.Value.Name.ShouldBe("emailTest1@domain.com");
            result.Value.PhoneNumber.ShouldBe("123");
            result.Value.AdditionalPhoneNumber.ShouldBe("321");
            result.Value.Sex.ShouldBe(Sex.Male);
            result.Value.DecadeOfBirth.ShouldBe(1990);
        }


        [Fact]
        public async Task Get_IfSupervisorDoesntExists_ReturnError()
        {
            //Act
            var result = await _supervisorService.Get(999);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Common.UserNotFound);
        }

        [Fact]
        public async Task Remove_WhenDeleting_EnsureCanDeleteUserIsCalled()
        {
            //act
            await _supervisorService.Remove(_supervisorWithoutDataCollectorsId);

            //assert
            await _deleteUserService.Received().EnsureCanDeleteUser(_supervisorWithoutDataCollectorsId, Role.Supervisor);
        }

        [Fact]
        public async Task Remove_WhenDeletingSupervisorWithDataCollectors_ReturnsError()
        {
            //act
            var result = await _supervisorService.Remove(_supervisorWithDataCollectorsId);

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Deletion.CannotDeleteSupervisorWithDataCollectors);
        }


        [Fact]
        public async Task Remove_WhenDeletingSupervisorWithOnlyDeletedDataCollectors_ReturnsSuccess()
        {
            //act
            var result = await _supervisorService.Remove(_supervisorWithDeletedDataCollectorsId);

            //assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task Remove_WhenDeletingSupervisorWithOnlyDeletedDataCollectors_DetachDataCollectorsFromSupervisorCommandShouldBeCalled()
        {
            //arrange
            FormattableString expectedSqlCommand = $"UPDATE Nyss.DataCollectors SET SupervisorId = null WHERE SupervisorId = {_supervisorWithDeletedDataCollectorsId}";

            // Act
            var result = await _supervisorService.Remove(_supervisorWithDeletedDataCollectorsId);

            //Assert
            result.IsSuccess.ShouldBeTrue();
            await _nyssContext.Received().ExecuteSqlInterpolatedAsync(Arg.Is<FormattableString>(arg => arg.ToString() == expectedSqlCommand.ToString()));
        }
    }
}
