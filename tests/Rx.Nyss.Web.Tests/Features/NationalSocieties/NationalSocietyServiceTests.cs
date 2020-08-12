using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Managers;
using RX.Nyss.Web.Features.NationalSocieties;
using RX.Nyss.Web.Features.NationalSocieties.Access;
using RX.Nyss.Web.Features.NationalSocieties.Dto;
using RX.Nyss.Web.Features.Organizations;
using RX.Nyss.Web.Features.SmsGateways;
using RX.Nyss.Web.Features.TechnicalAdvisors;
using RX.Nyss.Web.Services.Authorization;
using RX.Nyss.Web.Tests.Features.NationalSocieties.TestData;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.NationalSocieties
{
    public class NationalSocietyServiceTests
    {
        private readonly INationalSocietyService _nationalSocietyService;
        private readonly NationalSocietyServiceTestData _testData;

        private readonly INyssContext _nyssContextMock;
        private readonly ISmsGatewayService _smsGatewayServiceMock;

        private readonly IManagerService _managerServiceMock;
        private readonly ITechnicalAdvisorService _technicalAdvisorServiceMock;
        private readonly IOrganizationService _organizationServiceMock;
        private readonly IGeneralBlobProvider _generalBlobProviderMock;
        private readonly IDataBlobService _dataBlobServiceMock;


        public NationalSocietyServiceTests()
        {
            _nyssContextMock = Substitute.For<INyssContext>();
            var loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            var authorizationService = Substitute.For<IAuthorizationService>();
            authorizationService.GetCurrentUserName().Returns("yo");
            _managerServiceMock = Substitute.For<IManagerService>();
            _technicalAdvisorServiceMock = Substitute.For<ITechnicalAdvisorService>();
            _smsGatewayServiceMock = Substitute.For<ISmsGatewayService>();
            _generalBlobProviderMock = Substitute.For<IGeneralBlobProvider>();
            _organizationServiceMock = Substitute.For<IOrganizationService>();
            _dataBlobServiceMock = Substitute.For<IDataBlobService>();

            _nationalSocietyService = new NationalSocietyService(
                _nyssContextMock,
                Substitute.For<INationalSocietyAccessService>(),
                loggerAdapterMock,
                authorizationService,
                _managerServiceMock,
                _technicalAdvisorServiceMock,
                _smsGatewayServiceMock,
                _generalBlobProviderMock,
                _organizationServiceMock,
                _dataBlobServiceMock);

            _testData = new NationalSocietyServiceTestData(_nyssContextMock, _smsGatewayServiceMock);
        }

        [Fact]
        public async Task ConsentToNationalSocietyAgreement_WhenUserNotFound_ShouldReturnNotFound()
        {
            // Arrange
            _testData.BasicData.Data.GenerateData().AddToDbContext();
            var users = new List<User> { new ManagerUser { EmailAddress = "no-yo" } };
            var mockDbSet = users.AsQueryable().BuildMockDbSet();
            _nyssContextMock.Users.Returns(mockDbSet);

            // Act
            var result = await _nationalSocietyService.ConsentToNationalSocietyAgreement("fr");

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.User.Common.UserNotFound);
        }

        [Fact]
        public async Task CreateNationalSociety_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            _testData.BasicData.Data.GenerateData().AddToDbContext();
            var nationalSocietyReq = new CreateNationalSocietyRequestDto
            {
                Name = BasicNationalSocietyServiceTestData.NationalSocietyName,
                ContentLanguageId = BasicNationalSocietyServiceTestData.ContentLanguageId,
                CountryId = BasicNationalSocietyServiceTestData.CountryId
            };

            // Act
            await _nationalSocietyService.Create(nationalSocietyReq);

            // Assert
            await _nyssContextMock.Received(1).AddAsync(Arg.Any<NationalSociety>());
        }

        [Fact]
        public async Task EditNationalSociety_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            _testData.BasicData.Data.GenerateData().AddToDbContext();
            var nationalSocietyReq = new EditNationalSocietyRequestDto
            {
                Name = BasicNationalSocietyServiceTestData.NationalSocietyName,
                CountryId = BasicNationalSocietyServiceTestData.CountryId,
                ContentLanguageId = BasicNationalSocietyServiceTestData.ContentLanguageId
            };

            // Act
            var result = await _nationalSocietyService.Edit(BasicNationalSocietyServiceTestData.NationalSocietyId, nationalSocietyReq);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Edit.Success);
        }

        [Fact]
        public async Task RemoveNationalSociety_WhenSuccessful_ShouldReturnSuccess()
        {
            // Act
            _testData.BasicData.Data.GenerateData().AddToDbContext();
            var result = await _nationalSocietyService.Delete(BasicNationalSocietyServiceTestData.NationalSocietyId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Remove.Success);
        }

        [Fact]
        public async Task ArchiveNationalSociety_WhenHasNoProjectsAndNoUsersExceptHeadManager_ReturnsSuccess()
        {
            //arrange
            _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchived = _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.AdditionalData.NationalSocietyBeingArchived;
            var nationalSocietyBeingArchivedId = nationalSocietyBeingArchived.Id;

            //act
            var result = await _nationalSocietyService.Archive(nationalSocietyBeingArchivedId);

            //assert
            result.IsSuccess.ShouldBeTrue();
            nationalSocietyBeingArchived.IsArchived.ShouldBeTrue();
        }

        [Fact]
        public async Task ArchiveNationalSociety_WhenHasNoProjectsAndNoUsersExceptHeadManager_CallsSaveChanges()
        {
            //arrange
            _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchived = _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.AdditionalData.NationalSocietyBeingArchived;
            var nationalSocietyBeingArchivedId = nationalSocietyBeingArchived.Id;

            //act
            var result = await _nationalSocietyService.Archive(nationalSocietyBeingArchivedId);

            //assert
            await _nyssContextMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ArchiveNationalSociety_WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager_DeletesHeadManager()
        {
            //arrange
            _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchivedId = _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.AdditionalData.NationalSocietyBeingArchived.Id;
            var headManagerId = _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleManager.AdditionalData.HeadManager.Id;

            //act
            var result = await _nationalSocietyService.Archive(nationalSocietyBeingArchivedId);

            //assert
            await _managerServiceMock.Received(1).DeleteIncludingHeadManagerFlag(headManagerId);
        }

        [Fact]
        public async Task ArchiveNationalSociety_WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleTechnicalAdvisor_DeletesHeadManager()
        {
            //arrange
            var testCase = _testData.WhenHasNoProjectsAndNoUsersExceptHeadManagerWithRoleTechnicalAdvisor;
            testCase.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchivedId = testCase.AdditionalData.NationalSocietyBeingArchived.Id;
            var headManagerId = testCase.AdditionalData.HeadManager.Id;

            //act
            var result = await _nationalSocietyService.Archive(nationalSocietyBeingArchivedId);

            //assert
            await _technicalAdvisorServiceMock.Received(1).DeleteIncludingHeadManagerFlag(nationalSocietyBeingArchivedId, headManagerId);
        }

        [Fact]
        public async Task ArchiveNationalSociety_WhenSuccessfullyArchiving_ShouldRemoveSmsGateways()
        {
            //arrange
            _testData.WhenSuccessfullyArchivingNationalSocietyWith2SmsGateways.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchivedId = _testData.WhenSuccessfullyArchivingNationalSocietyWith2SmsGateways.AdditionalData.NationalSocietyBeingArchived.Id;
            var smsGatewaysInNationalSocietyIds = _testData.WhenSuccessfullyArchivingNationalSocietyWith2SmsGateways.AdditionalData.SmsGatewaysIds;

            //act
            var result = await _nationalSocietyService.Archive(nationalSocietyBeingArchivedId);

            //assert
            await _smsGatewayServiceMock.Received(1).Delete(smsGatewaysInNationalSocietyIds[0]);
            await _smsGatewayServiceMock.Received(1).Delete(smsGatewaysInNationalSocietyIds[1]);
        }

        [Fact]
        public async Task ArchiveNationalSociety_WhenHasNoProjectsAndHasSomeUsersExceptHeadManager_ReturnsError()
        {
            //arrange
            _testData.WhenHasNoProjectsAndSomeUsersExceptHeadManager.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchivedId = _testData.WhenHasNoProjectsAndSomeUsersExceptHeadManager.AdditionalData.NationalSocietyBeingArchived.Id;

            //act
            var result = await _nationalSocietyService.Archive(nationalSocietyBeingArchivedId);

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Archive.ErrorHasRegisteredUsers);
        }

        [Fact]
        public async Task ArchiveNationalSociety_WhenHasProjects_ReturnsError()
        {
            //arrange
            _testData.ArchiveWhenHasProjects.GenerateData().AddToDbContext();
            var nationalSocietyBeingArchivedId = _testData.ArchiveWhenHasProjects.AdditionalData.NationalSocietyBeingArchived.Id;

            //act
            var result = await _nationalSocietyService.Archive(nationalSocietyBeingArchivedId);

            //assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.NationalSociety.Archive.ErrorHasOpenedProjects);
        }


        [Fact]
        public async Task ReopenNationalSociety_WhenSucess_ArchivedFlagIsFalse()
        {
            //arrange
            _testData.WhenReopeningNationalSociety.GenerateData().AddToDbContext();
            var nationalSocietyBeingRestored = _testData.WhenReopeningNationalSociety.AdditionalData.NationalSocietyBeingReopened;

            //act
            var result = await _nationalSocietyService.Reopen(nationalSocietyBeingRestored.Id);

            //assert
            result.IsSuccess.ShouldBeTrue();
            nationalSocietyBeingRestored.IsArchived.ShouldBeFalse();
        }

        [Fact]
        public async Task ReopenNationalSociety_WhenSuccess_SaveChangesIsCalled()
        {
            //arrange
            _testData.WhenReopeningNationalSociety.GenerateData().AddToDbContext();
            var nationalSocietyBeingRestoredId = _testData.WhenReopeningNationalSociety.AdditionalData.NationalSocietyBeingReopened.Id;

            //act
            var result = await _nationalSocietyService.Reopen(nationalSocietyBeingRestoredId);

            //assert
            result.IsSuccess.ShouldBeTrue();
            await _nyssContextMock.Received().SaveChangesAsync();
        }
    }
}
