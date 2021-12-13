using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataConsumers.Commands;
using RX.Nyss.Web.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataConsumers.Commands
{
    public class DeleteDataConsumerCommandTests : DataConsumerTestsBase
    {
        private readonly IDeleteUserService _mockDeleteUserService;

        private readonly DeleteDataConsumerCommand.Handler _handler;

        public DeleteDataConsumerCommandTests()
        {
            _mockDeleteUserService = Substitute.For<IDeleteUserService>();

            _handler = new DeleteDataConsumerCommand.Handler(
                _mockNyssContext,
                Substitute.For<ILoggerAdapter>(),
                Substitute.For<IIdentityUserRegistrationService>(),
                _mockNationalSocietyUserService,
                _mockDeleteUserService);
        }

        [Fact]
        public async Task WhenSuccess_SaveChangesIsCalledOnce()
        {
            //arrange
            ArrangeUsersDbSetWithOneDataConsumerInOneNationalSociety();

            var cmd = new DeleteDataConsumerCommand(123, 1);

            //act
            await _handler.Handle(cmd, CancellationToken.None);

            //assert
            await _mockNyssContext.Received(1).SaveChangesAsync(CancellationToken.None);
        }

        [Fact]
        public async Task WhenDeletingFromNationalSocietyTheUserIsNotIn_ShouldReturnError()
        {
            //arrange
            ArrangeUsersDbSetWithOneDataConsumerInOneNationalSociety();
            var cmd = new DeleteDataConsumerCommand(123, 2);

            //act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Registration.UserIsNotAssignedToThisNationalSociety);
        }

        [Fact]
        public async Task WhenDeletingAUserThatDoesNotExist_ShouldReturnError()
        {
            //arrange
            ArrangeUsersDbSetWithOneDataConsumerInOneNationalSociety();
            var cmd = new DeleteDataConsumerCommand(321, 2);

            //act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Registration.UserNotFound);
        }

        [Fact]
        public async Task WhenDeletingFromLastNationalSociety_RemoveOnNyssUserIsCalledOnce()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneDataConsumerInOneNationalSociety();
            var cmd = new DeleteDataConsumerCommand(123, 1);

            //act
            await _handler.Handle(cmd, CancellationToken.None);

            //assert
            _mockNationalSocietyUserService.Received(1).DeleteNationalSocietyUser(user);
        }

        [Fact]
        public async Task WhenDeletingFromLastNationalSociety_RemoveOnNationalSocietyReferenceIsCalledOnce()
        {
            //arrange
            ArrangeUsersDbSetWithOneDataConsumerInOneNationalSociety();
            var cmd = new DeleteDataConsumerCommand(123, 1);

            //act
            await _handler.Handle(cmd, CancellationToken.None);

            //assert
            _mockNyssContext.UserNationalSocieties
                .Received(1)
                .Remove(Arg.Is<UserNationalSociety>(uns => uns.NationalSocietyId == 1 && uns.UserId == 123));
        }

        [Fact]
        public async Task WhenDeletingFromNotLastNationalSociety_RemoveOnNyssUserIsNotCalled()
        {
            //arrange
            var user = ArrangeUsersDbSetWithOneDataConsumerInTwoNationalSocieties();
            var cmd = new DeleteDataConsumerCommand(123, 1);

            //act
            await _handler.Handle(cmd, CancellationToken.None);

            //assert
            _mockNationalSocietyUserService.Received(0).DeleteNationalSocietyUser(user);
        }

        [Fact]
        public async Task WhenDeletingFromNotLastNationalSociety_RemoveOnNationalSocietyReferenceIsCalledOnce()
        {
            //arrange
            ArrangeUsersDbSetWithOneDataConsumerInTwoNationalSocieties();
            var cmd = new DeleteDataConsumerCommand(123, 1);

            //act
            await _handler.Handle(cmd, CancellationToken.None);

            //assert
            _mockNyssContext.UserNationalSocieties
                .Received(1)
                .Remove(Arg.Is<UserNationalSociety>(uns => uns.NationalSocietyId == 1 && uns.UserId == 123));
        }

        [Fact]
        public async Task WhenDeleting_EnsureCanDeleteUserIsCalled()
        {
            //arrange
            ArrangeUsersDbSetWithOneDataConsumerInTwoNationalSocieties();
            var cmd = new DeleteDataConsumerCommand(123, 1);

            //act
            await _handler.Handle(cmd, CancellationToken.None);

            //assert
            await _mockDeleteUserService.Received().EnsureCanDeleteUser(123, Role.DataConsumer);
        }

        private User ArrangeUsersDbSetWithOneDataConsumerInOneNationalSociety()
        {
            var dataConsumer = ArrangeUsersDbSetWithOneDataConsumer();

            var userNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety
                {
                    User = dataConsumer,
                    UserId = dataConsumer.Id,
                    NationalSocietyId = 1,
                    NationalSociety = _mockNyssContext.NationalSocieties.Find(1)
                }
            };

            ArrangeUserNationalSocietiesFrom(userNationalSocieties);
            dataConsumer.UserNationalSocieties = userNationalSocieties;

            return dataConsumer;
        }

        private void ArrangeUserNationalSocietiesFrom(IEnumerable<UserNationalSociety> userNationalSocieties)
        {
            var userNationalSocietiesDbSet = userNationalSocieties.AsQueryable().BuildMockDbSet();
            _mockNyssContext.UserNationalSocieties.Returns(userNationalSocietiesDbSet);
        }

        private User ArrangeUsersDbSetWithOneDataConsumerInTwoNationalSocieties()
        {
            var dataConsumer = ArrangeUsersDbSetWithOneDataConsumer();

            var userNationalSocieties = new List<UserNationalSociety>
            {
                new UserNationalSociety
                {
                    User = dataConsumer,
                    UserId = dataConsumer.Id,
                    NationalSocietyId = 1,
                    NationalSociety = _mockNyssContext.NationalSocieties.Find(1)
                },
                new UserNationalSociety
                {
                    User = dataConsumer,
                    UserId = dataConsumer.Id,
                    NationalSocietyId = 2,
                    NationalSociety = _mockNyssContext.NationalSocieties.Find(2)
                }
            };

            ArrangeUserNationalSocietiesFrom(userNationalSocieties);
            dataConsumer.UserNationalSocieties = userNationalSocieties;

            return dataConsumer;
        }
    }
}
