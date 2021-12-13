using System.Collections.Generic;
using System.Linq;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Tests.Features.DataConsumers
{
    public abstract class DataConsumerTestsBase
    {
        protected readonly INyssContext _mockNyssContext;

        protected readonly INationalSocietyUserService _mockNationalSocietyUserService;

        protected DataConsumerTestsBase()
        {
            _mockNyssContext = Substitute.For<INyssContext>();
            _mockNationalSocietyUserService = Substitute.For<INationalSocietyUserService>();
        }

        protected void ArrangeUsersFrom(IEnumerable<User> existingUsers)
        {
            var usersDbSet = existingUsers.AsQueryable().BuildMockDbSet();
            _mockNyssContext.Users.Returns(usersDbSet);

            _mockNationalSocietyUserService.GetNationalSocietyUser<DataConsumerUser>(Arg.Any<int>()).Returns(ci =>
            {
                var user = existingUsers.OfType<DataConsumerUser>().FirstOrDefault(x => x.Id == (int)ci[0]);
                if (user == null)
                {
                    throw new ResultException(ResultKey.User.Registration.UserNotFound);
                }

                return user;
            });

            _mockNationalSocietyUserService.GetNationalSocietyUserIncludingNationalSocieties<DataConsumerUser>(Arg.Any<int>())
                .Returns(ci => _mockNationalSocietyUserService.GetNationalSocietyUser<DataConsumerUser>((int)ci[0]));
        }

        protected void ArrangeUsersWithOneAdministratorUser() =>
            ArrangeUsersFrom(new List<User>
            {
                new AdministratorUser
                {
                    Id = 123,
                    Role = Role.Administrator
                }
            });

        protected User ArrangeUsersDbSetWithOneDataConsumer()
        {
            var dataConsumer = new DataConsumerUser
            {
                Id = 123,
                Role = Role.DataConsumer,
                EmailAddress = "emailTest1@domain.com",
                Name = "emailTest1@domain.com",
                Organization = "org org",
                PhoneNumber = "123",
                AdditionalPhoneNumber = "321"
            };
            ArrangeUsersFrom(new List<User> { dataConsumer });
            return dataConsumer;
        }
    }
}
