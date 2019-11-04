using System.Threading.Tasks;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Web.Utils.Logging;
using RX.Nyss.Web.Features.HealthRisk;
using Xunit;
using RX.Nyss.Data.Concepts;
using System.Collections.Generic;
using Shouldly;
using RX.Nyss.Data.Models;
using System.Linq;
using MockQueryable.NSubstitute;
using RX.Nyss.Web.Utils.DataContract;
using System;
using RX.Nyss.Web.Features.HealthRisk.Dto;

namespace Rx.Nyss.Web.Tests.Features.HealthRisk
{
    public class HealthRiskServiceTests
    {
        private readonly IHealthRiskService _healthRiskService;
        private readonly INyssContext _nyssContextMock;
        private readonly ILoggerAdapter _loggerAdapterMock;
        private const string UserName = "admin@domain.com";
        private const string HealthRiskName = "AWD";
        private const int HealthRiskId = 1;
        private const int HealthRiskCode = 1;
        private HealthRiskType HealthRiskType = HealthRiskType.Human;
        private const string FeedbackMessage = "Clean yo self";
        private const string CaseDefinition = "Some symptoms";
        private const int LanguageId = 1;
        private const int AlertRuleId = 1;
        private const int AlertRuleCountThreshold = 5;
        private const int AlertRuleMeterThreshold = 10;
        private const int AlertRuleDaysThreshold = 2;

        public HealthRiskServiceTests()
        {
            // Arrange
            _nyssContextMock = Substitute.For<INyssContext>();
            _loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _healthRiskService = new HealthRiskService(_nyssContextMock, _loggerAdapterMock);

            var users = new List<User>
            {
                new GlobalCoordinatorUser
                {
                    EmailAddress = UserName,
                    ApplicationLanguage = new ApplicationLanguage
                    {
                        Id = LanguageId,
                        DisplayName = "English",
                        LanguageCode = "EN"
                    }
                }
            };
            var contentLanguages = new List<ContentLanguage>
            {
                new ContentLanguage
                {
                    Id = LanguageId,
                    DisplayName = "English",
                    LanguageCode = "EN"
                }
            };
            var languageContents = new List<HealthRiskLanguageContent>
            {
                new HealthRiskLanguageContent
                {
                    Id = LanguageId,
                    CaseDefinition = CaseDefinition,
                    FeedbackMessage = FeedbackMessage,
                    Name = HealthRiskName,
                    ContentLanguage = contentLanguages[0]
                }
            };
            var alertRules = new List<AlertRule>
            {
                new AlertRule
                {
                    Id = AlertRuleId,
                    CountThreshold = AlertRuleCountThreshold,
                    HoursThreshold = AlertRuleHoursThreshold,
                    MetersThreshold = AlertRuleMeterThreshold,
                }
            };
            var healthRisks = new List<RX.Nyss.Data.Models.HealthRisk>
            {
                new RX.Nyss.Data.Models.HealthRisk
                {
                    Id = HealthRiskId,
                    HealthRiskType = HealthRiskType,
                    HealthRiskCode = HealthRiskCode,
                    LanguageContents = languageContents,
                    AlertRule = alertRules[0]
                }
            };
            var contentLanguages = new List<ContentLanguage>
            {
                new ContentLanguage { Id = LanguageId }
            };

            var contentLanguageMockDbSet = contentLanguages.AsQueryable().BuildMockDbSet();
            var languageContentsMockDbSet = languageContents.AsQueryable().BuildMockDbSet();
            var healthRisksMockDbSet = healthRisks.AsQueryable().BuildMockDbSet();
            var alertRuleMockDbSet = alertRules.AsQueryable().BuildMockDbSet();
            var usersMockDbSet = users.AsQueryable().BuildMockDbSet();
            var contentLanguagesMockDbSet = contentLanguages.AsQueryable().BuildMockDbSet();

            _nyssContextMock.ContentLanguages.Returns(contentLanguageMockDbSet);
            _nyssContextMock.HealthRiskLanguageContents.Returns(languageContentsMockDbSet);
            _nyssContextMock.HealthRisks.Returns(healthRisksMockDbSet);
            _nyssContextMock.AlertRules.Returns(alertRuleMockDbSet);
            _nyssContextMock.ContentLanguages.Returns(contentLanguagesMockDbSet);
            _nyssContextMock.Users.Returns(usersMockDbSet);
        }

        [Fact]
        public async Task CreateHealthRisk_WhenSuccessful_ShouldReturnSuccess()
        {
            // Arrange
            var createHealthRiskDto = new CreateHealthRiskRequestDto
            {
                HealthRiskCode = 2,
                HealthRiskType = HealthRiskType,
                LanguageContent = new List<HealthRiskLanguageContentDto>
                {
                    new HealthRiskLanguageContentDto
                    {
                        Name = HealthRiskName,
                        FeedbackMessage = FeedbackMessage,
                        CaseDefinition = CaseDefinition,
                        LanguageId = LanguageId
                    }
                }
            };

            // Act
            var result = await _healthRiskService.CreateHealthRisk(createHealthRiskDto);

            // Assert
            await _nyssContextMock.Received(1).AddAsync(Arg.Any<RX.Nyss.Data.Models.HealthRisk>());
        }

        [Fact]
        public async Task CreateHealthRisk_WhenHealthRiskNumberAlreadyExists_ShouldReturnError()
        {
            // Arrange
            var createHealthRiskDto = new CreateHealthRiskRequestDto
            {
                HealthRiskCode = HealthRiskCode,
                HealthRiskType = HealthRiskType,
                LanguageContent = new List<HealthRiskLanguageContentDto>
                {
                    new HealthRiskLanguageContentDto
                    {
                        Name = HealthRiskName,
                        FeedbackMessage = FeedbackMessage,
                        CaseDefinition = CaseDefinition,
                        LanguageId = LanguageId
                    }
                }
            };

            // Act
            var result = await _healthRiskService.CreateHealthRisk(createHealthRiskDto);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.HealthRiskNumberAlreadyExists);
        }

        [Fact]
        public async Task GetHealthRisk_WhenHealthRiskDoesNotExists_ShouldReturnError()
        {
            // Act
            var result = await _healthRiskService.GetHealthRisk(2);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.HealthRiskNotFound);
        }

        [Fact]
        public async Task GetHealthRisks_WhenSuccess_ShouldReturnAllHealthRisks()
        {
            // Act
            var result = await _healthRiskService.GetHealthRisks(UserName);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count().ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task EditHealthRisk_WhenHealthRiskDoesntExist_ShouldReturnError()
        {
            // Arrange
            var editHealthRisk = new EditHealthRiskRequestDto
            {
                Id = 2
            };

            // Act
            var result = await _healthRiskService.EditHealthRisk(editHealthRisk);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.HealthRiskNotFound);
        }

        [Fact]
        public async Task EditHealthRisk_WhenSuccess_ShouldReturnSuccess()
        {
            // Arrange
            var editHealthRisk = new EditHealthRiskRequestDto
            {
                Id = HealthRiskId,
                HealthRiskCode = HealthRiskCode,
                HealthRiskType = HealthRiskType,
                AlertRuleCountThreshold = AlertRuleCountThreshold,
                AlertRuleHoursThreshold = AlertRuleHoursThreshold,
                AlertRuleMetersThreshold = 1,
                AlertRuleId = AlertRuleId,
                LanguageContent = new List<HealthRiskLanguageContentDto>
                {
                    new HealthRiskLanguageContentDto
                    {
                        LanguageId = LanguageId,
                        CaseDefinition = CaseDefinition,
                        FeedbackMessage = FeedbackMessage,
                        Name = HealthRiskName
                    }
                }
            };

            // Act
            var result = await _healthRiskService.EditHealthRisk(editHealthRisk);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.EditSuccess);
        }

        [Fact]
        public async Task RemoveHealthRisk_WhenSuccess_ShouldReturnSuccess()
        {
            // Act
            var result = await _healthRiskService.RemoveHealthRisk(HealthRiskId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.RemoveSuccess);
        }
    }
}
