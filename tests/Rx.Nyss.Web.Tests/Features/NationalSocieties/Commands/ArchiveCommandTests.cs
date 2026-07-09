using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Managers;
using RX.Nyss.Web.Features.NationalSocieties.Commands;
using RX.Nyss.Web.Features.SmsGateways;
using RX.Nyss.Web.Features.TechnicalAdvisors;
using RX.Nyss.Web.Tests.Features.NationalSocieties.TestData;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.NationalSocieties.Commands
{
    public class ArchiveCommandTests
    {
        private readonly INyssContext _mockNyssContext;

        private readonly IManagerService _mockManagerService;

        private readonly ITechnicalAdvisorService _mockTechnicalAdvisorService;

        private readonly ISmsGatewayService _mockSmsGatewayService;

        private readonly NationalSocietyServiceTestData _testData;

        private readonly ArchiveCommand.Handler _handler;

        public ArchiveCommandTests()
        {
            _mockNyssContext = Substitute.For<INyssContext>();
            _mockManagerService = Substitute.For<IManagerService>();
            _mockTechnicalAdvisorService = Substitute.For<ITechnicalAdvisorService>();
            _mockSmsGatewayService = Substitute.For<ISmsGatewayService>();

            _testData = new NationalSocietyServiceTestData(_mockNyssContext, _mockSmsGatewayService);

            _handler = new ArchiveCommand.Handler(
                _mockNyssContext,
                _mockManagerService,
                _mockTechnicalAdvisorService,
                _mockSmsGatewayService);
        }

        [Fact]
        public async Task WhenHasNoProjectsAndNoUsersExceptHeadManager_ReturnsSuccess()
        {
            //arrange
            _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchived = _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.AdditionalData.NationalSocietyBeingArchived;
            var nationalSocietyBeingArchivedId = nationalSocietyBeingArchived.Id;

            var cmd = new ArchiveCommand(nationalSocietyBeingArchivedId);

            //act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            //assert
            result.IsSuccess.ShouldBeTrue();
            nationalSocietyBeingArchived.IsArchived.ShouldBeTrue();
        }

        [Fact]
        public async Task WhenHasNoProjectsAndNoUsersExceptHeadManager_CallsSaveChanges()
        {
            //arrange
            _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchived = _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.AdditionalData.NationalSocietyBeingArchived;
            var nationalSocietyBeingArchivedId = nationalSocietyBeingArchived.Id;

            var cmd = new ArchiveCommand(nationalSocietyBeingArchivedId);

            //act
            await _handler.Handle(cmd, CancellationToken.None);

            //assert
            await _mockNyssContext.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager_DeletesHeadManager()
        {
            //arrange
            _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchivedId = _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.AdditionalData.NationalSocietyBeingArchived.Id;
            var headManagerId = _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.AdditionalData.HeadManager.Id;

            var cmd = new ArchiveCommand(nationalSocietyBeingArchivedId);

            //act
            await _handler.Handle(cmd, CancellationToken.None);

            //assert
            await _mockManagerService.Received(1).DeleteIncludingHeadManagerFlag(headManagerId);
        }

        [Fact]
        public async Task ArchiveNationalSociety_WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleTechnicalAdvisor_DeletesHeadManager()
        {
            //arrange
            var testCase = _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleTechnicalAdvisor;
            testCase.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchivedId = testCase.AdditionalData.NationalSocietyBeingArchived.Id;
            var headManagerId = testCase.AdditionalData.HeadManager.Id;

            var cmd = new ArchiveCommand(nationalSocietyBeingArchivedId);

            //act
            await _handler.Handle(cmd, CancellationToken.None);

            //assert
            await _mockTechnicalAdvisorService.Received(1).DeleteIncludingHeadManagerFlag(nationalSocietyBeingArchivedId, headManagerId);
        }

        [Fact]
        public async Task ArchiveNationalSociety_WhenSuccessfullyArchiving_ShouldRemoveSmsGateways()
        {
            //arrange
            _testData.WhenSuccessfullyArchivingNationalSocietyWith2SmsGateways.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchivedId = _testData.WhenSuccessfullyArchivingNationalSocietyWith2SmsGateways.AdditionalData.NationalSocietyBeingArchived.Id;
            var smsGatewaysInNationalSocietyIds = _testData.WhenSuccessfullyArchivingNationalSocietyWith2SmsGateways.AdditionalData.SmsGatewaysIds;

            var cmd = new ArchiveCommand(nationalSocietyBeingArchivedId);

            //act
            await _handler.Handle(cmd, CancellationToken.None);

            //assert
            await _mockSmsGatewayService.Received(1).Delete(smsGatewaysInNationalSocietyIds[0]);
            await _mockSmsGatewayService.Received(1).Delete(smsGatewaysInNationalSocietyIds[1]);
        }

        [Fact]
        public async Task ArchiveNationalSociety_WhenHasNoProjectsAndHasSomeUsersExceptHeadManager_ReturnsError()
        {
            //arrange
            _testData.WhenHasNoProjectsAndSomeUsersExceptHeadManager.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchivedId = _testData.WhenHasNoProjectsAndSomeUsersExceptHeadManager.AdditionalData.NationalSocietyBeingArchived.Id;

            var cmd = new ArchiveCommand(nationalSocietyBeingArchivedId);

            //act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe("nationalSociety.archive.errorHasRegisteredUsers");
        }

        [Fact]
        public async Task ArchiveNationalSociety_WhenHasProjects_ReturnsError()
        {
            //arrange
            _testData.ArchiveWhenHasProjects.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchivedId = _testData.ArchiveWhenHasProjects.AdditionalData.NationalSocietyBeingArchived.Id;

            var cmd = new ArchiveCommand(nationalSocietyBeingArchivedId);

            //act
            var result = await _handler.Handle(cmd, CancellationToken.None);

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe("nationalSociety.archive.errorHasOpenedProjects");
        }
    }
}
