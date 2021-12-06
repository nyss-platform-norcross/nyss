using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataConsumers.Commands;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataConsumers.Commands
{
    public class EditDataConsumerCommandTests : DataConsumerTestsBase
    {
        private readonly EditDataConsumerCommand.Handler _handler;

        public EditDataConsumerCommandTests()
        {
            _handler = new EditDataConsumerCommand.Handler(
                _mockNyssContext,
                _mockNationalSocietyUserService,
                Substitute.For<ILoggerAdapter>());
        }

        [Fact]
        public async Task WhenEditingNonExistingUser_ReturnsErrorResult()
        {
            // Arrange
            ArrangeUsersFrom(new List<User>());
            var cmd = new EditDataConsumerCommand(0, new EditDataConsumerCommand.RequestBody());

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task WhenEditingUserThatIsNotDataConsumer_ReturnsErrorResult()
        {
            // Arrange
            ArrangeUsersWithOneAdministratorUser();
            var cmd = new EditDataConsumerCommand(123, new EditDataConsumerCommand.RequestBody());

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task WhenEditingUserThatIsNotDataConsumer_SaveChangesShouldNotBeCalled()
        {
            // Arrange
            ArrangeUsersWithOneAdministratorUser();
            var cmd = new EditDataConsumerCommand(123, new EditDataConsumerCommand.RequestBody());

            // Act
            await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            await _mockNyssContext.DidNotReceive().SaveChangesAsync(CancellationToken.None);
        }

        [Fact]
        public async Task WhenEditingExistingDataConsumer_ReturnsSuccess()
        {
            // Arrange
            ArrangeUsersDbSetWithOneDataConsumer();

            var cmd = new EditDataConsumerCommand(123, new EditDataConsumerCommand.RequestBody());

            // Act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task WhenEditingExistingDataConsumer_SaveChangesAsyncIsCalled()
        {
            // Arrange
            ArrangeUsersDbSetWithOneDataConsumer();

            var cmd = new EditDataConsumerCommand(123, new EditDataConsumerCommand.RequestBody());

            // Act
            await _handler.Handle(cmd, CancellationToken.None);

            // Assert
            await _mockNyssContext.Received().SaveChangesAsync(CancellationToken.None);
        }

        [Fact]
        public async Task WhenEditingExistingUser_ExpectedFieldsGetEdited()
        {
            // Arrange
            ArrangeUsersDbSetWithOneDataConsumer();

            var existingUserEmail = _mockNyssContext.Users.Single(u => u.Id == 123)?.EmailAddress;

            var cmd = new EditDataConsumerCommand(123, new EditDataConsumerCommand.RequestBody
            {
                Name = "New name",
                Organization = "New organization",
                PhoneNumber = "456",
                AdditionalPhoneNumber = "654"
            });

            // Act
            await _handler.Handle(cmd, CancellationToken.None);

            var editedUser = _mockNyssContext.Users.Single(u => u.Id == 123) as DataConsumerUser;

            editedUser.ShouldNotBeNull();
            editedUser.Name.ShouldBe(cmd.Body.Name);
            editedUser.Organization.ShouldBe(cmd.Body.Organization);
            editedUser.PhoneNumber.ShouldBe(cmd.Body.PhoneNumber);
            editedUser.EmailAddress.ShouldBe(existingUserEmail);
            editedUser.AdditionalPhoneNumber.ShouldBe(cmd.Body.AdditionalPhoneNumber);
        }
    }
}
