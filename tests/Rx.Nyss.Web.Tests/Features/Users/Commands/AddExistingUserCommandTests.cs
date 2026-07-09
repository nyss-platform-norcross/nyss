using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Users.Commands;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Users.Commands;

public class AddExistingUserCommandTests
{
    private readonly INyssContext _mockNyssContext;

    private readonly IAuthorizationService _mockAuthorizationService;

    private readonly AddExistingUserCommand.Handler _handler;

    public AddExistingUserCommandTests()
    {
        _mockNyssContext = Substitute.For<INyssContext>();
        _mockAuthorizationService = Substitute.For<IAuthorizationService>();

        _handler = new AddExistingUserCommand.Handler(
            _mockNyssContext,
            _mockAuthorizationService);
    }

    [Fact]
    public async Task WhenEmailDoesntExist_ShouldReturnError()
    {
        // arrange
        var users = Enumerable.Empty<User>().AsQueryable().BuildMockDbSet();
        var organizations = Enumerable.Empty<Organization>().AsQueryable().BuildMockDbSet();

        _mockNyssContext.Users.Returns(users);
        _mockNyssContext.Organizations.Returns(organizations);

        _mockAuthorizationService.GetCurrentUser().Returns(new AdministratorUser());
        _mockAuthorizationService.IsCurrentUserInRole(Role.Manager).Returns(false);

        var cmd = new AddExistingUserCommand
        {
            Email = "bla@ble.com",
        };

        //act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        //assert
        result.IsSuccess.ShouldBeFalse();
        result.Message.Key.ShouldBe("user.registration.userNotFound");
    }

    [Fact]
    public async Task WhenEmailExistsButIsNotAssignable_ShouldReturnError()
    {
        // arrange
        const string email = "manager7@domain.com";
        const int organizationId = 1;

        var users = new User[]
        {
            new ManagerUser
            {
                EmailAddress = email,
                Role = Role.Manager,
            }
        }.AsQueryable().BuildMockDbSet();
        var organizations = new[]
        {
            new Organization { Id = organizationId },
        }.AsQueryable().BuildMockDbSet();

        _mockNyssContext.Users.Returns(users);
        _mockNyssContext.Organizations.Returns(organizations);

        _mockAuthorizationService.GetCurrentUser().Returns(new AdministratorUser());
        _mockAuthorizationService.IsCurrentUserInRole(Role.Manager).Returns(false);

        var cmd = new AddExistingUserCommand
        {
            OrganizationId = organizationId,
            Email = email,
        };

        //act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        //assert
        result.IsSuccess.ShouldBeFalse();
        result.Message.Key.ShouldBe("user.registration.noAssignableUserWithThisEmailFound");
    }

    [Fact]
    public async Task WhenEmailExistsButUserAlreadyIsInThisNationalSociety_ShouldReturnError()
    {
        // arrange
        const string email = "manager7@domain.com";
        const int nationalSocietyId = 666;
        const int organizationId = 123;
        const int userId = 66;

        var users = new User[]
        {
            new TechnicalAdvisorUser
            {
                Id = userId,
                EmailAddress = email,
                Role = Role.TechnicalAdvisor,
            }
        }.AsQueryable().BuildMockDbSet();

        var organizations = new[]
        {
            new Organization { Id = organizationId },
        }.AsQueryable().BuildMockDbSet();

        var userNationalSocieties = new[]
        {
            new UserNationalSociety
            {
                UserId = userId,
                NationalSocietyId = nationalSocietyId,
                User = users.First(),
            },
        }.AsQueryable().BuildMockDbSet();

        _mockNyssContext.Users.Returns(users);
        _mockNyssContext.Organizations.Returns(organizations);
        _mockNyssContext.UserNationalSocieties.Returns(userNationalSocieties);

        _mockAuthorizationService.GetCurrentUser().Returns(new AdministratorUser());
        _mockAuthorizationService.IsCurrentUserInRole(Role.Manager).Returns(false);
        _mockAuthorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor)
            .Returns(true);

        var cmd = new AddExistingUserCommand
        {
            NationalSocietyId = nationalSocietyId,
            OrganizationId = organizationId,
            Email = email,
        };

        //act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        //assert
        result.IsSuccess.ShouldBeFalse();
        result.Message.Key.ShouldBe(ResultKey.User.Registration.UserIsAlreadyInThisNationalSociety);
    }

    [Fact]
    public async Task WhenEmailExistsAndIsAssignable_ShouldReturnSuccess()
    {
        // arrange
        const string email = "technicalAdvisor5@domain.com";
        const int nationalSocietyId = 666;
        const int organizationId = 123;
        const int userId = 66;

        var users = new User[]
        {
            new TechnicalAdvisorUser
            {
                Id = userId,
                EmailAddress = email,
                Role = Role.TechnicalAdvisor,
            }
        }.AsQueryable().BuildMockDbSet();

        var organizations = new[]
        {
            new Organization { Id = organizationId },
        }.AsQueryable().BuildMockDbSet();

        var userNationalSocieties = Enumerable.Empty<UserNationalSociety>().AsQueryable().BuildMockDbSet();

        var nationalSocieties = new[]
        {
            new NationalSociety
            {
                Id = nationalSocietyId,
            },
        }.AsQueryable().BuildMockDbSet();

        _mockNyssContext.Users.Returns(users);
        _mockNyssContext.Organizations.Returns(organizations);
        _mockNyssContext.UserNationalSocieties.Returns(userNationalSocieties);
        _mockNyssContext.NationalSocieties.Returns(nationalSocieties);

        _mockAuthorizationService.GetCurrentUser().Returns(new AdministratorUser());
        _mockAuthorizationService.IsCurrentUserInRole(Role.Manager).Returns(false);
        _mockAuthorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor)
            .Returns(true);

        var cmd = new AddExistingUserCommand
        {
            NationalSocietyId = nationalSocietyId,
            OrganizationId = organizationId,
            Email = email,
        };

        //act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        //assert
        result.IsSuccess.ShouldBeTrue();
        await _mockNyssContext.Received(1).SaveChangesAsync();
        _mockNyssContext.UserNationalSocieties
            .Received(1)
            .Add(Arg.Is<UserNationalSociety>(u => u.UserId == userId && u.NationalSocietyId == nationalSocietyId));
    }

    [Fact]
    public async Task WhenEmailExistsAndIsAssignableAndCurrentUserIsManager_ShouldUseManagerOrganization()
    {
        // arrange
        const string email = "technicalAdvisor5@domain.com";
        const int nationalSocietyId = 666;
        const int organizationId = 123;
        const int userId = 66;
        const int currentUserId = 321;

        var users = new User[]
        {
            new TechnicalAdvisorUser
            {
                Id = userId,
                EmailAddress = email,
                Role = Role.TechnicalAdvisor,
            }
        }.AsQueryable().BuildMockDbSet();

        var organizations = new[]
        {
            new Organization { Id = organizationId },
        }.AsQueryable().BuildMockDbSet();

        var currentUser = new ManagerUser
        {
            Id = currentUserId,
        };
        var userNationalSocieties = new[]
        {
            new UserNationalSociety
            {
                UserId = currentUserId,
                NationalSocietyId = nationalSocietyId,
                OrganizationId = organizationId,
                Organization = new Organization { Id = organizationId },
                User = currentUser,
            },
        }.AsQueryable().BuildMockDbSet();

        var nationalSocieties = new[]
        {
            new NationalSociety
            {
                Id = nationalSocietyId,
            },
        }.AsQueryable().BuildMockDbSet();

        _mockNyssContext.Users.Returns(users);
        _mockNyssContext.Organizations.Returns(organizations);
        _mockNyssContext.UserNationalSocieties.Returns(userNationalSocieties);
        _mockNyssContext.NationalSocieties.Returns(nationalSocieties);

        _mockAuthorizationService.GetCurrentUser().Returns(currentUser);
        _mockAuthorizationService.IsCurrentUserInRole(Role.Manager).Returns(true);
        _mockAuthorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor)
            .Returns(true);

        var cmd = new AddExistingUserCommand
        {
            OrganizationId = organizationId,
            NationalSocietyId = nationalSocietyId,
            Email = email,
        };

        //act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        //assert
        result.IsSuccess.ShouldBeTrue();

        _mockNyssContext.UserNationalSocieties
            .Received(1)
            .Add(Arg.Is<UserNationalSociety>(u => u.Organization.Id == organizationId));
    }

    [Fact]
    public async Task WhenEmailExistsAndIsAssignableAndCurrentUserIsManagerWithInvalidOrganization_ShouldReturnError()
    {
        // arrange
        const string email = "technicalAdvisor5@domain.com";
        const int nationalSocietyId = 666;
        const int organizationId = 123;
        const int userId = 66;
        const int currentUserId = 321;

        var users = new User[]
        {
            new TechnicalAdvisorUser
            {
                Id = userId,
                EmailAddress = email,
                Role = Role.TechnicalAdvisor,
            }
        }.AsQueryable().BuildMockDbSet();

        var organizations = new[]
        {
            new Organization { Id = organizationId },
        }.AsQueryable().BuildMockDbSet();

        var currentUser = new ManagerUser
        {
            Id = currentUserId,
        };
        var userNationalSocieties = Enumerable.Empty<UserNationalSociety>().AsQueryable().BuildMockDbSet();

        var nationalSocieties = new[]
        {
            new NationalSociety
            {
                Id = nationalSocietyId,
            },
        }.AsQueryable().BuildMockDbSet();

        _mockNyssContext.Users.Returns(users);
        _mockNyssContext.Organizations.Returns(organizations);
        _mockNyssContext.UserNationalSocieties.Returns(userNationalSocieties);
        _mockNyssContext.NationalSocieties.Returns(nationalSocieties);

        _mockAuthorizationService.GetCurrentUser().Returns(currentUser);
        _mockAuthorizationService.IsCurrentUserInRole(Role.Manager).Returns(true);
        _mockAuthorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor)
            .Returns(true);

        var cmd = new AddExistingUserCommand
        {
            OrganizationId = organizationId,
            NationalSocietyId = nationalSocietyId,
            Email = email,
        };

        //act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        //assert
        result.IsSuccess.ShouldBeFalse();
        result.Message.Key.ShouldBe("user.registration.invalidUserOrganization");
    }
}
