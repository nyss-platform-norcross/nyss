using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Supervisor;
using RX.Nyss.Web.Features.Supervisor.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using Shouldly;
using Xunit;

namespace Rx.Nyss.Web.Tests.Features.Supervisor
{
    public class SupervisorServiceTests
    {
        private readonly SupervisorService _supervisorService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationServiceMock;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailServiceMock;

        public SupervisorServiceTests()
        {
            _loggerAdapter = Substitute.For<ILoggerAdapter>();
            _nyssContext = Substitute.For<INyssContext>();
            _identityUserRegistrationServiceMock = Substitute.For<IIdentityUserRegistrationService>();
            _verificationEmailServiceMock = Substitute.For<IVerificationEmailService>();
            _nationalSocietyUserService = Substitute.For<INationalSocietyUserService>();

            var applicationLanguages = new List<ApplicationLanguage>();
            var applicationLanguagesDbSet = applicationLanguages.AsQueryable().BuildMockDbSet();
            _nyssContext.ApplicationLanguages.Returns(applicationLanguagesDbSet);

            _supervisorService = new SupervisorService(_identityUserRegistrationServiceMock, _nationalSocietyUserService, _nyssContext, _loggerAdapter, _verificationEmailServiceMock);

            _identityUserRegistrationServiceMock.CreateIdentityUser(Arg.Any<string>(), Arg.Any<Role>()).Returns(ci => new IdentityUser { Id = "123", Email = (string)ci[0] });

            SetupTestNationalSocieties();
            SetupTestProjects();
        }

        private void SetupTestNationalSocieties()
        {
            var nationalSociety1 = new RX.Nyss.Data.Models.NationalSociety { Id = 1, Name = "Test national society 1"};
            var nationalSociety2 = new RX.Nyss.Data.Models.NationalSociety { Id = 2, Name = "Test national society 2" };

            var nationalSocieties = new List<RX.Nyss.Data.Models.NationalSociety> { nationalSociety1, nationalSociety2 }; 
            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);

            _nyssContext.NationalSocieties.FindAsync(1).Returns(nationalSociety1);
            _nyssContext.NationalSocieties.FindAsync(2).Returns(nationalSociety2);
        }

        private void SetupTestProjects()
        {
            var project1 = new Project{ Id = 1, NationalSociety = _nyssContext.NationalSocieties.Single(x => x.Id == 1), Name = "project 1", State = ProjectState.Open };
            var project2 = new Project{ Id = 2, NationalSociety = _nyssContext.NationalSocieties.Single(x => x.Id == 1), Name = "project 2", State = ProjectState.Open };
            var project3 = new Project{ Id = 3, NationalSociety = _nyssContext.NationalSocieties.Single(x => x.Id == 1), Name = "project 3", State = ProjectState.Closed };
            var project4 = new Project{ Id = 4, NationalSociety = _nyssContext.NationalSocieties.Single(x => x.Id == 2), Name = "project 4", State = ProjectState.Open };
            var project5 = new Project{ Id = 5, NationalSociety = _nyssContext.NationalSocieties.Single(x => x.Id == 2), Name = "project 5", State = ProjectState.Open };

            var projects = new List<Project> { project1, project2, project3, project4 };
            var projectsDbSet = projects.AsQueryable().BuildMockDbSet();

            _nyssContext.Projects.Returns(projectsDbSet);
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
            var nationalSocietyId = 1;
            var result = await _supervisorService.Create(nationalSocietyId, registerSupervisorRequestDto);

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
            var nationalSocietyId = 1;
            var result = await _supervisorService.Create(nationalSocietyId, registerSupervisorRequestDto);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Supervisor.ProjectDoesNotExistOrNoAccess);
        }

        [Fact]
        public async Task Create_WhenCreatingSupervisorWithProjectInAnotherNationalSociety_ShouldReturnError()
        {
            //Arrange
            var userEmail = "emailTest1@domain.com";
            var registerSupervisorRequestDto = new CreateSupervisorRequestDto { Name = userEmail, Email = userEmail, ProjectId = 5 };

            //Act
            var nationalSocietyId = 1;
            var result = await _supervisorService.Create(nationalSocietyId, registerSupervisorRequestDto);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Supervisor.ProjectDoesNotExistOrNoAccess);
        }


        [Fact]
        public async Task Edit_WhenEditingNonExistingUser_ReturnsErrorResult()
        {
            ArrangeUsersFrom(new List<User> { });

            var result = await _supervisorService.Edit(123, new EditSupervisorRequestDto() {});

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task Edit_WhenEditingUserThatIsNotSupervisor_ReturnsErrorResult()
        {
            ArrangeUsersWithOneAdministratorUser();

            var result = await _supervisorService.Edit(123, new EditSupervisorRequestDto() { });

            result.IsSuccess.ShouldBeFalse();
        }

        private void ArrangeUsersWithOneAdministratorUser() =>
            ArrangeUsersFrom(new List<User> { new AdministratorUser() { Id = 123, Role = Role.Administrator } });

        private void ArrangeUsersFrom(IEnumerable<User> existingUsers)
        {
            var usersDbSet = existingUsers.AsQueryable().BuildMockDbSet();
            _nyssContext.Users.Returns(usersDbSet);

            _nationalSocietyUserService.GetNationalSocietyUser<SupervisorUser>(Arg.Any<int>()).Returns(ci =>
            {
                var user = existingUsers.OfType<SupervisorUser>().FirstOrDefault(x => x.Id == (int)ci[0]);
                if (user == null)
                {
                    throw new ResultException(ResultKey.User.Registration.UserNotFound);
                }
                return user;
            });
        }
        
        [Fact]
        public async Task Edit_WhenEditingUserThatIsNotSupervisor_SaveChangesShouldNotBeCalled()
        {
            ArrangeUsersWithOneAdministratorUser();

            await _supervisorService.Edit(123, new EditSupervisorRequestDto() { });

            await _nyssContext.DidNotReceive().SaveChangesAsync();
        }


        [Fact]
        public async Task Edit_WhenEditingExistingSupervisor_ReturnsSuccess()
        {
            ArrangeUsersDbSetWithOneSupervisor();

            var result = await _supervisorService.Edit(123, new EditSupervisorRequestDto() {  });

            result.IsSuccess.ShouldBeTrue();
        }

        private void ArrangeUsersDbSetWithOneSupervisor()
        {
            var supervisor = new SupervisorUser
            {
                Id = 123,
                Role = Role.Supervisor,
                EmailAddress = "emailTest1@domain.com",
                Name = "emailTest1@domain.com",
                PhoneNumber = "123",
                AdditionalPhoneNumber = "321",
                Sex = Sex.Male,
                DecadeOfBirth = 1990,
            };
            ArrangeUsersFrom(new List<User> { supervisor });


            var supervisorUserProjects = new List<SupervisorUserProject>
            {
                new SupervisorUserProject { Project = _nyssContext.Projects.Single(x => x.Id == 1), ProjectId = 1, SupervisorUser = supervisor, SupervisorUserId = 123 },
                new SupervisorUserProject { Project = _nyssContext.Projects.Single(x => x.Id == 3), ProjectId = 3, SupervisorUser = supervisor, SupervisorUserId = 123 }
            };
            var supervisorUserProjectsDbSet = supervisorUserProjects.AsQueryable().BuildMockDbSet();
            supervisor.SupervisorUserProjects = supervisorUserProjects;
            _nyssContext.SupervisorUserProjects.Returns(supervisorUserProjectsDbSet);


            var userNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety { NationalSociety = _nyssContext.NationalSocieties.Single(x => x.Id == 1), NationalSocietyId = 1, User =  supervisor, UserId = 1}
            };
            var userNationalSocietiesDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            supervisor.UserNationalSocieties = userNationalSocieties;
            _nyssContext.UserNationalSocieties.Returns(userNationalSocietiesDbSet);
        }

    
        [Fact]
        public async Task Edit_WhenEditingExistingSupervisor_SaveChangesAsyncIsCalled()
        {
            ArrangeUsersDbSetWithOneSupervisor();

            await _supervisorService.Edit(123, new EditSupervisorRequestDto() {  });

            await _nyssContext.Received().SaveChangesAsync();
        }


        [Fact]
        public async Task Edit_WhenEditingExistingUser_ExpectedFieldsGetEdited()
        {
            ArrangeUsersDbSetWithOneSupervisor();

            var existingUserEmail = _nyssContext.Users.Single(u => u.Id == 123)?.EmailAddress;

            var editRequest = new EditSupervisorRequestDto()
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123"
            };

            await _supervisorService.Edit(123, editRequest);

            var editedUser = _nyssContext.Users.Single(u => u.Id == 123) as SupervisorUser;

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
            ArrangeUsersDbSetWithOneSupervisor();
            var editRequest = new EditSupervisorRequestDto()
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123",
                ProjectId = null,
            };

            //Act
            await _supervisorService.Edit(123, editRequest);

            //Assert
            _nyssContext.SupervisorUserProjects.Received(1).Remove(Arg.Any<SupervisorUserProject>());
        }

        [Fact]
        public async Task Edit_WhenSwitchingProject_NyssContextRemoveAndAddProjectEachAreCalledOnce()
        {
            //Arrange
            ArrangeUsersDbSetWithOneSupervisor();
            var editRequest = new EditSupervisorRequestDto()
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123",
                ProjectId = 2,
            };

            //Act
            await _supervisorService.Edit(123, editRequest);

            //Assert
            var removedReference = _nyssContext.SupervisorUserProjects.Single(x => x.ProjectId == 1);
            _nyssContext.SupervisorUserProjects.Received(1).Remove(removedReference);
            await _nyssContext.Received(1).AddAsync(Arg.Any<SupervisorUserProject>());
        }


        [Fact]
        public async Task Edit_WhenSwitchingProjectToTheSameProject_NyssContextRemoveAndAddProjectShouldNotBeCalled()
        {
            //Arrange
            ArrangeUsersDbSetWithOneSupervisor();
            var editRequest = new EditSupervisorRequestDto()
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123",
                ProjectId = 1,
            };

            //Act
            await _supervisorService.Edit(123, editRequest);

            //Assert
            _nyssContext.SupervisorUserProjects.Received(0).Remove(Arg.Any<SupervisorUserProject>());
            await _nyssContext.Received(0).AddAsync(Arg.Any<SupervisorUserProject>());
        }


        [Fact]
        public async Task Edit_WhenSwitchingProjectToOneFromOtherNationalSociety_ShouldReturnError()
        {
            //Arrange
            ArrangeUsersDbSetWithOneSupervisor();
            var editRequest = new EditSupervisorRequestDto()
            {
                Name = "New name",
                PhoneNumber = "432432",
                Sex = Sex.Female,
                DecadeOfBirth = 1980,
                AdditionalPhoneNumber = "123123",
                ProjectId = 4,
            };

            //Act
            var result = await _supervisorService.Edit(123, editRequest);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Supervisor.ProjectDoesNotExistOrNoAccess);
        }


        [Fact]
        public async Task Remove_WhenSupervisorHasSomeProjectReferences_RemoveRangeIsCalled()
        {
            //Arrange
            ArrangeUsersDbSetWithOneSupervisor();
            
            //Act
            var result = await _supervisorService.Remove(123);

            //Assert
            _nyssContext.SupervisorUserProjects.Received(1).RemoveRange(Arg.Any<List<SupervisorUserProject>>());
        }

        [Fact]
        public async Task Remove_WhenRemovingNonExistantSupervisot_ReturnsError()
        {
            //Arrange
            ArrangeUsersDbSetWithOneSupervisor();

            //Act
            var result = await _supervisorService.Remove(666);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Common.UserNotFound);
        }


        [Fact]
        public async Task Get_IfSupervisorExists_ReturnSupervisor()
        {
            //Arrange
            ArrangeUsersDbSetWithOneSupervisor();

            //Act
            var result = await _supervisorService.Get(123);

            //Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldBe(123);
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
            //Arrange
            ArrangeUsersDbSetWithOneSupervisor();

            //Act
            var result = await _supervisorService.Get(666);

            //Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Common.UserNotFound);
        }
    }
}
