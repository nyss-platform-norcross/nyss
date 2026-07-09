using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataConsumers.Commands;
using RX.Nyss.Web.Features.Users;
using RX.Nyss.Web.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.DataConsumers.Commands
{
    public class EditDataConsumerCommandTests : DataConsumerTestsBase
    {
        private readonly IVerificationEmailService _verificationEmailService;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IUserService _userService;

        private readonly EditDataConsumerCommand.Handler _handler;

        private readonly int _administratorId = 1;
        private readonly int _dataConsumerId = 123;

        public EditDataConsumerCommandTests()
        {
            _verificationEmailService = Substitute.For<IVerificationEmailService>();
            _identityUserRegistrationService = Substitute.For<IIdentityUserRegistrationService>();
            _loggerAdapter = Substitute.For<ILoggerAdapter>();
            _userService = Substitute.For<IUserService>();

            _handler = new EditDataConsumerCommand.Handler(
                _verificationEmailService,
                _identityUserRegistrationService,
                _mockNyssContext,
                _mockNationalSocietyUserService,
                _loggerAdapter,
                _userService);
        }

        [Fact]
        public async Task Edit_WhenEditingNonExistingUser_ReturnsErrorResult()
        {
            var request = new EditDataConsumerCommand(999, new EditDataConsumerCommand.RequestBody());
            var result = await _handler.Handle(request, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task Edit_WhenEditingUserThatIsNotDataConsumer_ReturnsErrorResult()
        {
            ArrangeUsersWithOneAdministratorUser();

            var request = new EditDataConsumerCommand(_administratorId, new EditDataConsumerCommand.RequestBody());
            var result = await _handler.Handle(request, CancellationToken.None);

            result.IsSuccess.ShouldBeFalse();
        }

        [Fact]
        public async Task Edit_WhenEditingUserThatIsNotDataConsumer_SaveChangesShouldNotBeCalled()
        {

            var request = new EditDataConsumerCommand(_administratorId, new EditDataConsumerCommand.RequestBody());
            var result = await _handler.Handle(request, CancellationToken.None);

            await _mockNyssContext.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task Edit_WhenEditingExistingDataConsumer_ReturnsSuccess()
        {
            ArrangeUsersDbSetWithOneDataConsumer();

            var request = new EditDataConsumerCommand(_dataConsumerId, new EditDataConsumerCommand.RequestBody());
            var result = await _handler.Handle(request, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task Edit_WhenEditingExistingDataConsumer_SaveChangesAsyncIsCalled()
        {
            ArrangeUsersDbSetWithOneDataConsumer();

            var request = new EditDataConsumerCommand(_dataConsumerId, new EditDataConsumerCommand.RequestBody());
            var result = await _handler.Handle(request, CancellationToken.None);

            await _mockNyssContext.Received().SaveChangesAsync();
        }

        [Fact]
        public async Task Edit_WhenEditingExistingUser_ExpectedFieldsGetEdited()
        {
            ArrangeUsersDbSetWithOneDataConsumer();

            var existingUserEmail = "emailTest1@domain.com"; //From mockNyssContext setup in TestsBase

            var request = new EditDataConsumerCommand(_dataConsumerId, new EditDataConsumerCommand.RequestBody
            {
                Name = "New Name",
                PhoneNumber = "432432",
                AdditionalPhoneNumber = "123123"
            });

            var result = await _handler.Handle(request, CancellationToken.None);

            var editedUser = _mockNyssContext.Users.Single(u => u.Id == _dataConsumerId) as DataConsumerUser;

            editedUser.ShouldNotBeNull();
            editedUser.Name.ShouldBe(request.Body.Name);
            editedUser.PhoneNumber.ShouldBe(request.Body.PhoneNumber);
            editedUser.AdditionalPhoneNumber.ShouldBe(request.Body.AdditionalPhoneNumber);
            editedUser.EmailAddress.ShouldBe(existingUserEmail);
        }
    }
}
