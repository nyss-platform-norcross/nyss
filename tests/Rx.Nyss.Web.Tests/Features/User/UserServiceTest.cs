using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.User;
using Shouldly;
using Xunit;

namespace Rx.Nyss.Web.Tests.Features.Users
{
    public class UserServiceTest
    {
        private IUserService _userService;
        private readonly INyssContext _nyssContext;

        const string NationalSociety1Tag = "NationalSociety1";
        const string NationalSociety2Tag = "NationalSociety2";
        const string NationalSociety1And2Tag = "NationalSociety1And2";

        public UserServiceTest()
        {

            _nyssContext = Substitute.For<INyssContext>();
            _userService = new UserService(_nyssContext);
            ArrangeNationalSocieties();
        }

        private void ArrangeNationalSocieties()
        {
            var nationalSocieties = new List<RX.Nyss.Data.Models.NationalSociety>
            {
                new RX.Nyss.Data.Models.NationalSociety {Id = 1, Name = "National society 1"},
                new RX.Nyss.Data.Models.NationalSociety {Id = 2, Name = "National society 2"}
            };

            SetupNationalSocietiesFrom(nationalSocieties);
        }


        private void SetupNationalSocietiesFrom(List<RX.Nyss.Data.Models.NationalSociety> nationalSocieties)
        {
            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);
        }


        [Fact]
        public async Task GetUsersInNationalSociety_ShouldReturOnlyUsersFromSpecifiedNationalSociety()
        {
            ArrangeUsers();

            var users = await _userService.GetUsersInNationalSociety(1);

            users.Value.Count.ShouldBe(5);
            users.Value.ShouldAllBe(u => u.Name == NationalSociety1Tag || u.Name == NationalSociety1And2Tag);
        }

        [Fact]
        public async Task GetUsersInNationalSociety_ShouldReturnOnlyUsersWithSpecificRoles()
        {
            ArrangeUsers();

            var users = await _userService.GetUsersInNationalSociety(1);

            var allowedRoles = new List<Role> {Role.DataConsumer, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor}.Select(x => x.ToString());
            users.Value.Count.ShouldBe(5);
            users.Value.ShouldAllBe(u => allowedRoles.Contains(u.Role));
        }


        private void ArrangeUsers()
        {
            var users = new List<User>
            {
                new AdministratorUser {Id = 1, Role = Role.Administrator},
                new GlobalCoordinatorUser {Id = 2, Role = Role.GlobalCoordinator},
                new ManagerUser {Id = 3, Role = Role.Manager, Name = NationalSociety1Tag},
                new DataConsumerUser {Id = 4, Role = Role.DataConsumer, Name = NationalSociety1Tag},
                new TechnicalAdvisorUser {Id = 5, Role = Role.TechnicalAdvisor, Name = NationalSociety1Tag},
                new SupervisorUser {Id = 6, Role = Role.Supervisor, Name = NationalSociety1Tag},
                new ManagerUser {Id = 7, Role = Role.Manager, Name = NationalSociety2Tag},
                new DataConsumerUser {Id = 8, Role = Role.DataConsumer, Name = NationalSociety2Tag},
                new TechnicalAdvisorUser {Id = 9, Role = Role.TechnicalAdvisor, Name = NationalSociety2Tag},
                new SupervisorUser {Id = 10, Role = Role.Supervisor, Name = NationalSociety2Tag},
                new TechnicalAdvisorUser {Id = 13, Role = Role.TechnicalAdvisor, Name = NationalSociety1And2Tag},
            };

            var userNationalSocieties1 = users.Where(u => u.Name == NationalSociety1Tag).Select(u => new UserNationalSociety {User = u, UserId = u.Id, NationalSocietyId = 1});
            var userNationalSocieties2 = users.Where(u => u.Name == NationalSociety2Tag).Select(u => new UserNationalSociety {User = u, UserId = u.Id, NationalSocietyId = 2});
            var userNationalSocieties1And2 = users.Where(u => u.Name == NationalSociety1And2Tag).Select(u => new UserNationalSociety {User = u, UserId = u.Id, NationalSocietyId = 1})
                     .Concat(users.Where(u => u.Name == NationalSociety1And2Tag).Select(u => new UserNationalSociety {User = u, UserId = u.Id, NationalSocietyId = 2}));

            ArrangeUsersFrom(users);
            ArrangeUserNationalSocietiesFrom(userNationalSocieties1.Concat(userNationalSocieties2).Concat(userNationalSocieties1And2));
        }

        private void ArrangeUsersFrom(IEnumerable<RX.Nyss.Data.Models.User> existingUsers)
        {
            var usersDbSet = existingUsers.AsQueryable().BuildMockDbSet();
            _nyssContext.Users.Returns(usersDbSet);
        }

        private void ArrangeUserNationalSocietiesFrom(IEnumerable<UserNationalSociety> userNationalSocieties)
        {
            var userNationalSocietyDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContext.UserNationalSocieties.Returns(userNationalSocietyDbSet);
        }
    }
}
