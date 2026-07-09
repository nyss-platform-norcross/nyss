using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Users;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Users;

public class UserServiceTest
{
    private const string NationalSociety1Tag = "NationalSociety1";
    private const string NationalSociety2Tag = "NationalSociety2";
    private const string NationalSociety1And2Tag = "NationalSociety1And2";
    private readonly IUserService _userService;
    private readonly INyssContext _nyssContext;
    private readonly IAuthorizationService _authorizationService;

    public UserServiceTest()
    {
        _nyssContext = Substitute.For<INyssContext>();
        _authorizationService = Substitute.For<IAuthorizationService>();
        _authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Manager, Role.TechnicalAdvisor).Returns(true);
        _authorizationService.GetCurrentUser().Returns(new AdministratorUser());
        _userService = new UserService(_nyssContext, _authorizationService);
        ArrangeNationalSocieties();
    }

    private void ArrangeNationalSocieties()
    {
        var organizations = new List<Organization>
        {
            new Organization
            {
                Id = 1,
                Name = "first org"
            },
            new Organization
            {
                Id = 2,
                Name = "another org"
            }
        };

        var nationalSocieties = new List<NationalSociety>
        {
            new NationalSociety
            {
                Id = 1,
                Name = "National society 1",
                DefaultOrganization = organizations[0]
            },
            new NationalSociety
            {
                Id = 2,
                Name = "National society 2",
                DefaultOrganization = organizations[1]
            }
        };

        SetupNationalSocietiesFrom(nationalSocieties);
        SetupOrgsFrom(organizations);
        ArrangeUsers(nationalSocieties, organizations);
    }

    private void SetupNationalSocietiesFrom(List<NationalSociety> nationalSocieties)
    {
        var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
        _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);
    }


    private void SetupOrgsFrom(List<Organization> orgs)
    {
        var orgsDbSet = orgs.AsQueryable().BuildMockDbSet();
        _nyssContext.Organizations.Returns(orgsDbSet);
    }


    [Fact]
    public async Task GetUsersInNationalSociety_ShouldReturnOnlyUsersFromSpecifiedNationalSociety()
    {
        var users = await _userService.List(1);

        users.Value.Count.ShouldBe(5);
        users.Value.ShouldAllBe(u => u.Name == NationalSociety1Tag || u.Name == NationalSociety1And2Tag);
    }

    [Fact]
    public async Task GetUsersInNationalSociety_ShouldReturnOnlyUsersWithSpecificRoles()
    {
        var users = await _userService.List(1);

        var allowedRoles = new List<Role>
        {
            Role.DataConsumer,
            Role.Manager,
            Role.TechnicalAdvisor,
            Role.Supervisor
        }.Select(x => x.ToString());
        users.Value.Count.ShouldBe(5);
        users.Value.ShouldAllBe(u => allowedRoles.Contains(u.Role));
    }

    [Fact]
    public async Task GetUsersInNationalSociety_WhenCallingRoleIsOtherThanGlobalCoordinator_ShouldReturnAllUsers()
    {
        var users = await _userService.List(1);

        var allowedRoles = new List<Role>
        {
            Role.DataConsumer,
            Role.Manager,
            Role.TechnicalAdvisor,
            Role.Supervisor
        }.Select(x => x.ToString());
        users.Value.Count.ShouldBe(5);
        users.Value.ShouldAllBe(u => allowedRoles.Contains(u.Role));
    }

    [Fact]
    public async Task GetUsersInNationalSociety_WhenCallingRoleIsGlobalCoordinator_ShouldNotReturnSupervisors()
    {
        //arrange
        _authorizationService.GetCurrentUser().Returns(new GlobalCoordinatorUser());

        //act
        var users = await _userService.List(1);

        //assert
        users.Value.Count.ShouldBe(0);
        users.Value.ShouldAllBe(u => u.Role != Role.Supervisor.ToString());
    }

    private void ArrangeUsers(List<NationalSociety> nationalSocieties, List<Organization> organizations)
    {
        var users = new List<User>
        {
            new AdministratorUser
            {
                Id = 1,
                Role = Role.Administrator,
                EmailAddress = "admin1@domain.com"
            },
            new GlobalCoordinatorUser
            {
                Id = 2,
                Role = Role.GlobalCoordinator,
                EmailAddress = "globalAdministrator2@domain.com"
            },
            new ManagerUser
            {
                Id = 3,
                Role = Role.Manager,
                Name = NationalSociety1Tag,
                EmailAddress = "manager3@domain.com"
            },
            new DataConsumerUser
            {
                Id = 4,
                Role = Role.DataConsumer,
                Name = NationalSociety1Tag,
                EmailAddress = "dataConsumer4@domain.com",
                IsFirstLogin = false
            },
            new TechnicalAdvisorUser
            {
                Id = 5,
                Role = Role.TechnicalAdvisor,
                Name = NationalSociety1Tag,
                EmailAddress = "technicalAdvisor5@domain.com"
            },
            new SupervisorUser
            {
                Id = 6,
                Role = Role.Supervisor,
                Name = NationalSociety1Tag,
                EmailAddress = "supervisor6@domain.com"
            },
            new ManagerUser
            {
                Id = 7,
                Role = Role.Manager,
                Name = NationalSociety2Tag,
                EmailAddress = "manager7@domain.com"
            },
            new DataConsumerUser
            {
                Id = 8,
                Role = Role.DataConsumer,
                Name = NationalSociety2Tag,
                EmailAddress = "dataConsumer8@domain.com",
                IsFirstLogin = false
            },
            new TechnicalAdvisorUser
            {
                Id = 9,
                Role = Role.TechnicalAdvisor,
                Name = NationalSociety2Tag,
                EmailAddress = "technicalAdvisor9@domain.com"
            },
            new SupervisorUser
            {
                Id = 10,
                Role = Role.Supervisor,
                Name = NationalSociety2Tag,
                EmailAddress = "supervisor10@domain.com"
            },
            new TechnicalAdvisorUser
            {
                Id = 13,
                Role = Role.TechnicalAdvisor,
                Name = NationalSociety1And2Tag,
                EmailAddress = "technicalAdvisor11@domain.com"
            }
        };

        var userNationalSocieties1 = users.Where(u => u.Name == NationalSociety1Tag).Select(u => new UserNationalSociety
        {
            User = u,
            UserId = u.Id,
            NationalSocietyId = 1,
            NationalSociety = nationalSocieties[0],
            OrganizationId = 1,
            Organization = organizations[0]
        });

        var userNationalSocieties2 = users.Where(u => u.Name == NationalSociety2Tag).Select(u => new UserNationalSociety
        {
            User = u,
            UserId = u.Id,
            NationalSocietyId = 2,
            NationalSociety = nationalSocieties[1],
            OrganizationId = 2,
            Organization = organizations[1]
        });

        var userNationalSocieties1And2 = users.Where(u => u.Name == NationalSociety1And2Tag).Select(u => new UserNationalSociety
        {
            User = u,
            UserId = u.Id,
            NationalSocietyId = 1,
            NationalSociety = nationalSocieties[0],
            OrganizationId = 1,
            Organization = organizations[0]
        })
            .Concat(users.Where(u => u.Name == NationalSociety1And2Tag).Select(u => new UserNationalSociety
            {
                User = u,
                UserId = u.Id,
                NationalSocietyId = 2,
                NationalSociety = nationalSocieties[1],
                OrganizationId = 2,
                Organization = organizations[1]
            }));

        ArrangeUsersFrom(users);
        ArrangeUserNationalSocietiesFrom(userNationalSocieties1.Concat(userNationalSocieties2).Concat(userNationalSocieties1And2));

        ArrangeProjects();
        ArrangeSupervisorUserProjects();

        _authorizationService.GetCurrentUser().Returns(users[2]);
        _authorizationService.GetCurrentUser().Returns(users[2]);
    }

    private void ArrangeSupervisorUserProjects()
    {
        var supervisor = _nyssContext.Users.OfType<SupervisorUser>().Single(x => x.Id == 6);
        var project = _nyssContext.Projects.Single(x => x.Id == 1);

        var supervisorUserProjects = new List<SupervisorUserProject>
            {
                new SupervisorUserProject
                {
                    SupervisorUserId = 6,
                    SupervisorUser = supervisor,
                    ProjectId = 1,
                    Project = project
                }
            }
            .AsQueryable().BuildMockDbSet();

        supervisor.SupervisorUserProjects = supervisorUserProjects.Where(x => x.SupervisorUserId == 6).ToList();
        project.SupervisorUserProjects = supervisorUserProjects.Where(x => x.ProjectId == 1).ToList();

        _nyssContext.SupervisorUserProjects.Returns(supervisorUserProjects);
    }

    private void ArrangeProjects()
    {
        var projectsDbSet = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    NationalSociety = _nyssContext.NationalSocieties.Single(ns => ns.Id == 1),
                    Name = "awd in somalia",
                    State = ProjectState.Open
                }
            }
            .AsQueryable().BuildMockDbSet();
        _nyssContext.Projects.Returns(projectsDbSet);
    }

    private void ArrangeUsersFrom(IEnumerable<User> existingUsers)
    {
        var usersDbSet = existingUsers.AsQueryable().BuildMockDbSet();
        _nyssContext.Users.Returns(usersDbSet);
    }

    private void ArrangeUserNationalSocietiesFrom(IEnumerable<UserNationalSociety> userNationalSocieties)
    {
        var userNationalSocietyDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
        _nyssContext.UserNationalSocieties.Returns(userNationalSocietyDbSet);
    }

    [Fact]
    public async Task GetUsers_ReturnsOnlyUsersInSpecifiedNationalSociety()
    {
        //act
        var nationalSocietyId = 1;
        var result = await _userService.List(nationalSocietyId);

        //assert
        result.Value.Count.ShouldBe(5);
        result.Value.ShouldAllBe(u => u.Name == NationalSociety1Tag || u.Name == NationalSociety1And2Tag);
    }

    [Fact]
    public async Task GetUsers_ReturnsOnlyNotDeletedUsers()
    {
        //act
        var nationalSocietyId = 1;
        var result = await _userService.List(nationalSocietyId);

        //assert
        var users = _nyssContext.Users.Where(u => result.Value.Select(x => x.Id).Contains(u.Id)).ToList();
        result.Value.Count.ShouldBe(5);
        users.ShouldAllBe(u => !u.DeletedAt.HasValue);
    }
}
