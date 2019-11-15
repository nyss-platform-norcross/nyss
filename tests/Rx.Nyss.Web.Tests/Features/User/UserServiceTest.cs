﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.DataContract;
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
                new RX.Nyss.Data.Models.NationalSociety {Id = 1, Name = "National society 1", PendingHeadManager = null, HeadManager = null},
                new RX.Nyss.Data.Models.NationalSociety {Id = 2, Name = "National society 2", PendingHeadManager = null, HeadManager = null}
            };

            SetupNationalSocietiesFrom(nationalSocieties);
            ArrangeUsers(nationalSocieties);
        }


        private void SetupNationalSocietiesFrom(List<RX.Nyss.Data.Models.NationalSociety> nationalSocieties)
        {
            var nationalSocietiesDbSet = nationalSocieties.AsQueryable().BuildMockDbSet();
            _nyssContext.NationalSocieties.Returns(nationalSocietiesDbSet);
        }


        [Fact]
        public async Task GetUsersInNationalSociety_ShouldReturOnlyUsersFromSpecifiedNationalSociety()
        {
            var users = await _userService.GetUsersInNationalSociety(1);

            users.Value.Count.ShouldBe(5);
            users.Value.ShouldAllBe(u => u.Name == NationalSociety1Tag || u.Name == NationalSociety1And2Tag);
        }

        private void ArrangeUsers(List<RX.Nyss.Data.Models.NationalSociety> nationalSocieties)
        {
            var users = new List<User>
            {
                new AdministratorUser {Id = 1, Role = Role.Administrator, EmailAddress = "admin1@domain.com"},
                new GlobalCoordinatorUser {Id = 2, Role = Role.GlobalCoordinator, EmailAddress = "globalAdministrator2@domain.com"},
                new ManagerUser {Id = 3, Role = Role.Manager, Name = NationalSociety1Tag, EmailAddress = "manager3@domain.com"},
                new DataConsumerUser {Id = 4, Role = Role.DataConsumer, Name = NationalSociety1Tag, EmailAddress = "dataConsumer4@domain.com"},
                new TechnicalAdvisorUser {Id = 5, Role = Role.TechnicalAdvisor, Name = NationalSociety1Tag, EmailAddress = "technicalAdvisor5@domain.com"},
                new SupervisorUser {Id = 6, Role = Role.Supervisor, Name = NationalSociety1Tag, EmailAddress = "supervisor6@domain.com"},
                new ManagerUser {Id = 7, Role = Role.Manager, Name = NationalSociety2Tag, EmailAddress = "manager7@domain.com"},
                new DataConsumerUser {Id = 8, Role = Role.DataConsumer, Name = NationalSociety2Tag, EmailAddress = "dataConsumer8@domain.com"},
                new TechnicalAdvisorUser {Id = 9, Role = Role.TechnicalAdvisor, Name = NationalSociety2Tag, EmailAddress = "technicalAdvisor9@domain.com"},
                new SupervisorUser {Id = 10, Role = Role.Supervisor, Name = NationalSociety2Tag, EmailAddress = "supervisor10@domain.com"},
                new TechnicalAdvisorUser {Id = 13, Role = Role.TechnicalAdvisor, Name = NationalSociety1And2Tag, EmailAddress = "technicalAdvisor11@domain.com"},
            };

            var userNationalSocieties1 = users.Where(u => u.Name == NationalSociety1Tag).Select(u => new UserNationalSociety {User = u, UserId = u.Id, NationalSocietyId = 1, NationalSociety = nationalSocieties[0]});
            var userNationalSocieties2 = users.Where(u => u.Name == NationalSociety2Tag).Select(u => new UserNationalSociety {User = u, UserId = u.Id, NationalSocietyId = 2, NationalSociety = nationalSocieties[1]});
            var userNationalSocieties1And2 = users.Where(u => u.Name == NationalSociety1And2Tag).Select(u => new UserNationalSociety {User = u, UserId = u.Id, NationalSocietyId = 1, NationalSociety = nationalSocieties[0] })
                     .Concat(users.Where(u => u.Name == NationalSociety1And2Tag).Select(u => new UserNationalSociety {User = u, UserId = u.Id, NationalSocietyId = 2, NationalSociety = nationalSocieties[1] }));

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
        
        [Fact]
        public async Task GetUsersInNationalSociety_ShouldReturnOnlyUsersWithSpecificRoles()
        {
            var users = await _userService.GetUsersInNationalSociety(1);

            var allowedRoles = new List<Role> {Role.DataConsumer, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor}.Select(x => x.ToString());
            users.Value.Count.ShouldBe(5);
            users.Value.ShouldAllBe(u => allowedRoles.Contains(u.Role));
        }

        [Fact]
        public async Task AddExisting_WhenEmailDoesntExist_ShouldReturnError()
        {
            //act
            var result = await _userService.AddExisting(2, "bla@ble.com");

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Registration.UserNotFound);
        }


        [Fact]
        public async Task AddExisting_WhenEmailExistsButIsNotAssignable_ShouldReturnError()
        {
            //act
            var result = await _userService.AddExisting(2, "manager7@domain.com");

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Registration.NoAssignableUserWithThisEmailFound);
        }

        [Fact]
        public async Task AddExisting_WhenEmailExistsButUserAlreadyIsInThisNationalSociety_ShouldReturnError()
        {
            //arrange
            ArrangeUsers();

            //act
            var result = await _userService.AddExisting(1, "technical.advisor5@domain.com");

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Registration.NoAssignableUserWithThisEmailFound);
        }

        [Fact]
        public async Task AddExisting_WhenEmailExistsAndIsAssignable_ShouldReturnSuccess()
        {
            //act
            var result = await _userService.AddExisting(2, "technicalAdvisor5@domain.com");

            //assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task AddExisting_WhenEmailExistsAndIsAssignable_AddOnUserNationalSocietiesShouldBeCalledOnce()
        {
            //act
            var result = await _userService.AddExisting(2, "technicalAdvisor5@domain.com");

            //assert
            await _nyssContext.UserNationalSocieties.Received(1).AddAsync(Arg.Any<UserNationalSociety>());
        }


        [Fact]
        public async Task AddExisting_WhenEmailExistsAndIsAssignable_SaveChangesShouldBeCalledOnce()
        {
            //act
            var result = await _userService.AddExisting(2, "technicalAdvisor5@domain.com");

            //assert
            await _nyssContext.Received(1).SaveChangesAsync();
        }
    }
}
