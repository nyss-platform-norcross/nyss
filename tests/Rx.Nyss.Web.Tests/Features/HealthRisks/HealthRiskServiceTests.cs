using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.HealthRisks;
using RX.Nyss.Web.Features.HealthRisks.Dto;
using RX.Nyss.Web.Services.Authorization;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.HealthRisks
{
    public class HealthRiskServiceTests
    {
        private readonly IHealthRiskService _healthRiskService;
        private readonly INyssContext _nyssContextMock;
        private readonly HealthRisk _healthRisk;
        private const HealthRiskType HealthRiskType = Nyss.Data.Concepts.HealthRiskType.Human;
        private const string UserName = "admin@domain.com";
        private const string HealthRiskName = "AWD";
        private const int HealthRiskId = 1;
        private const int HealthRiskCode = 1;
        private const string FeedbackMessage = "Clean yo self";
        private const string CaseDefinition = "Some symptoms";
        private const int LanguageId = 1;
        private const int NewLanguageId = 2;
        private const int AlertRuleId = 1;
        private const int AlertRuleCountThreshold = 5;
        private const int AlertRuleKilometersThreshold = 10;
        private const int AlertRuleDaysThreshold = 2;

        public HealthRiskServiceTests()
        {
            // Arrange
            var authorizationService = Substitute.For<IAuthorizationService>();
            _nyssContextMock = Substitute.For<INyssContext>();
            _healthRiskService = new HealthRiskService(_nyssContextMock, authorizationService);

            var users = new List<User>
            {
                new GlobalCoordinatorUser
                {
                    EmailAddress = UserName,
                    ApplicationLanguage = new ApplicationLanguage
                    {
                        Id = LanguageId,
                        DisplayName = "English",
                        LanguageCode = "en"
                    }
                }
            };

            var contentLanguages = new List<ContentLanguage>
            {
                new ContentLanguage
                {
                    Id = LanguageId,
                    DisplayName = "English",
                    LanguageCode = "en"
                },
                new ContentLanguage
                {
                    Id = NewLanguageId,
                    DisplayName = "New language",
                    LanguageCode = "NEW"
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
                    DaysThreshold = AlertRuleDaysThreshold,
                    KilometersThreshold = AlertRuleKilometersThreshold
                }
            };

            _healthRisk = new HealthRisk
            {
                Id = HealthRiskId,
                HealthRiskType = HealthRiskType,
                HealthRiskCode = HealthRiskCode,
                LanguageContents = languageContents,
                AlertRule = alertRules[0]
            };

            var healthRisks = new List<HealthRisk>
            {
                _healthRisk
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
            var healthRiskRequestDto = new HealthRiskRequestDto
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
            await _healthRiskService.Create(healthRiskRequestDto);

            // Assert
            await _nyssContextMock.Received(1).AddAsync(Arg.Any<HealthRisk>());
        }

        [Fact]
        public async Task CreateHealthRisk_WhenHealthRiskNumberAlreadyExists_ShouldReturnError()
        {
            // Arrange
            var healthRiskRequestDto = new HealthRiskRequestDto
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
            var result = await _healthRiskService.Create(healthRiskRequestDto);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.HealthRiskNumberAlreadyExists);
        }

        [Fact]
        public async Task GetHealthRisk_WhenHealthRiskDoesNotExists_ShouldReturnError()
        {
            // Act
            var result = await _healthRiskService.Get(2);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.HealthRiskNotFound);
        }

        [Fact]
        public async Task GetHealthRisks_WhenSuccess_ShouldReturnAllHealthRisks()
        {
            // Act
            var result = await _healthRiskService.List();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.Count().ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task EditHealthRisk_WhenHealthRiskDoesNotExist_ShouldReturnError()
        {
            // Arrange
            var healthRiskRequestDto = new HealthRiskRequestDto();

            // Act
            var result = await _healthRiskService.Edit(2, healthRiskRequestDto);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.HealthRiskNotFound);
        }

        [Fact]
        public async Task EditHealthRisk_WhenHealthRiskCodeWasChangedAndHealthRiskContainsReports_ShouldReturnError()
        {
            // Arrange
            var projectHealthRisks = new List<ProjectHealthRisk>
            {
                new ProjectHealthRisk
                {
                    HealthRiskId = HealthRiskId,
                    Reports = new List<Report>
                    {
                        new Report()
                    }
                }
            };

            var projectHealthRisksMockDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksMockDbSet);

            var healthRiskRequestDto = new HealthRiskRequestDto
            {
                HealthRiskCode = 100,
                HealthRiskType = HealthRiskType,
                AlertRuleCountThreshold = AlertRuleCountThreshold,
                AlertRuleDaysThreshold = AlertRuleDaysThreshold,
                AlertRuleKilometersThreshold = 1,
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
            var result = await _healthRiskService.Edit(HealthRiskId, healthRiskRequestDto);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.HealthRiskContainsReports);
        }

        [Fact]
        public async Task EditHealthRisk_WhenNameInAnyLanguageWasChangedAndHealthRiskContainsReports_ShouldReturnError()
        {
            // Arrange
            var projectHealthRisks = new List<ProjectHealthRisk>
            {
                new ProjectHealthRisk
                {
                    HealthRiskId = HealthRiskId,
                    Reports = new List<Report>
                    {
                        new Report()
                    }
                }
            };

            var projectHealthRisksMockDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksMockDbSet);

            var healthRiskRequestDto = new HealthRiskRequestDto
            {
                HealthRiskCode = HealthRiskCode,
                HealthRiskType = HealthRiskType,
                AlertRuleCountThreshold = AlertRuleCountThreshold,
                AlertRuleDaysThreshold = AlertRuleDaysThreshold,
                AlertRuleKilometersThreshold = 1,
                LanguageContent = new List<HealthRiskLanguageContentDto>
                {
                    new HealthRiskLanguageContentDto
                    {
                        LanguageId = LanguageId,
                        CaseDefinition = CaseDefinition,
                        FeedbackMessage = FeedbackMessage,
                        Name = "New health risk name"
                    }
                }
            };

            // Act
            var result = await _healthRiskService.Edit(HealthRiskId, healthRiskRequestDto);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.HealthRiskContainsReports);
        }

        [Fact]
        public async Task EditHealthRisk_WhenHealthRiskNameForNewLanguageIsProvided_ShouldReturnSuccess()
        {
            // Arrange
            var projectHealthRisks = new List<ProjectHealthRisk>
            {
                new ProjectHealthRisk
                {
                    HealthRiskId = HealthRiskId,
                    Reports = new List<Report>
                    {
                        new Report()
                    }
                }
            };

            var projectHealthRisksMockDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksMockDbSet);

            var healthRiskRequestDto = new HealthRiskRequestDto
            {
                HealthRiskCode = HealthRiskCode,
                HealthRiskType = HealthRiskType,
                AlertRuleCountThreshold = AlertRuleCountThreshold,
                AlertRuleDaysThreshold = AlertRuleDaysThreshold,
                AlertRuleKilometersThreshold = 1,
                LanguageContent = new List<HealthRiskLanguageContentDto>
                {
                    new HealthRiskLanguageContentDto
                    {
                        LanguageId = LanguageId,
                        CaseDefinition = CaseDefinition,
                        FeedbackMessage = FeedbackMessage,
                        Name = HealthRiskName
                    },
                    new HealthRiskLanguageContentDto
                    {
                        LanguageId = NewLanguageId,
                        CaseDefinition = CaseDefinition,
                        FeedbackMessage = FeedbackMessage,
                        Name = "New health risk name"
                    }
                }
            };

            // Act
            var result = await _healthRiskService.Edit(HealthRiskId, healthRiskRequestDto);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.Edit.EditSuccess);
        }

        [Fact]
        public async Task EditHealthRisk_WhenHealthRiskNameForExistingLanguageIsAdded_ShouldReturnSuccess()
        {
            // Arrange
            var projectHealthRisks = new List<ProjectHealthRisk>
            {
                new ProjectHealthRisk
                {
                    HealthRiskId = HealthRiskId,
                    Reports = new List<Report>
                    {
                        new Report() { IsTraining = false }
                    }
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
                    ContentLanguage = _nyssContextMock.ContentLanguages.ElementAt(0)
                },
                new HealthRiskLanguageContent
                {
                    Id = NewLanguageId,
                    CaseDefinition = "",
                    FeedbackMessage = "",
                    Name = "",
                    ContentLanguage = _nyssContextMock.ContentLanguages.ElementAt(1)
                }
            };

            _healthRisk.LanguageContents = languageContents;

            var healthRisks = new List<HealthRisk> { _healthRisk };

            var healthRisksMockDbSet = healthRisks.AsQueryable().BuildMockDbSet();
            var projectHealthRisksMockDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            var languageContentMockDbSet = languageContents.AsQueryable().BuildMockDbSet();
            _nyssContextMock.HealthRisks.Returns(healthRisksMockDbSet);
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksMockDbSet);
            _nyssContextMock.HealthRiskLanguageContents.Returns(languageContentMockDbSet);


            var healthRiskRequestDto = new HealthRiskRequestDto
            {
                HealthRiskCode = HealthRiskCode,
                HealthRiskType = HealthRiskType,
                AlertRuleCountThreshold = AlertRuleCountThreshold,
                AlertRuleDaysThreshold = AlertRuleDaysThreshold,
                AlertRuleKilometersThreshold = 1,
                LanguageContent = new List<HealthRiskLanguageContentDto>
                {
                    new HealthRiskLanguageContentDto
                    {
                        LanguageId = LanguageId,
                        CaseDefinition = CaseDefinition,
                        FeedbackMessage = FeedbackMessage,
                        Name = HealthRiskName
                    },
                    new HealthRiskLanguageContentDto
                    {
                        LanguageId = NewLanguageId,
                        CaseDefinition = CaseDefinition,
                        FeedbackMessage = FeedbackMessage,
                        Name = "New health risk name"
                    }
                }
            };

            // Act
            var result = await _healthRiskService.Edit(HealthRiskId, healthRiskRequestDto);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.Edit.EditSuccess);
        }

        [Fact]
        public async Task EditHealthRisk_WhenSuccess_ShouldReturnSuccess()
        {
            // Arrange
            var projectHealthRisks = new List<ProjectHealthRisk>();
            var projectHealthRisksMockDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksMockDbSet);

            var healthRiskRequestDto = new HealthRiskRequestDto
            {
                HealthRiskCode = HealthRiskCode,
                HealthRiskType = HealthRiskType,
                AlertRuleCountThreshold = AlertRuleCountThreshold,
                AlertRuleDaysThreshold = AlertRuleDaysThreshold,
                AlertRuleKilometersThreshold = 1,
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
            var result = await _healthRiskService.Edit(HealthRiskId, healthRiskRequestDto);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.Edit.EditSuccess);
        }

        [Fact]
        public async Task RemoveHealthRisk_WhenSuccess_ShouldReturnSuccess()
        {
            // Arrange
            var projectHealthRisks = new List<ProjectHealthRisk>();
            var projectHealthRisksMockDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksMockDbSet);

            // Act
            var result = await _healthRiskService.Delete(HealthRiskId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.Remove.RemoveSuccess);
        }

        [Fact]
        public async Task RemoveHealthRisk_WhenAlertIsNotNull_ShouldRemoveAlertFromTheContext()
        {
            // Arrange
            var projectHealthRisks = new List<ProjectHealthRisk>();
            var projectHealthRisksMockDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksMockDbSet);

            var alertRule = new AlertRule();
            _healthRisk.AlertRule = alertRule;

            // Act
            await _healthRiskService.Delete(HealthRiskId);

            // Assert
            _nyssContextMock.AlertRules.Received(1).Remove(alertRule);
        }

        [Fact]
        public async Task RemoveHealthRisk_WhenAlertIsNull_ShouldNotCallRemoveAlertFromTheContext()
        {
            // Arrange
            var projectHealthRisks = new List<ProjectHealthRisk>();
            var projectHealthRisksMockDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksMockDbSet);

            _healthRisk.AlertRule = null;

            // Act
            await _healthRiskService.Delete(HealthRiskId);

            // Assert
            _nyssContextMock.AlertRules.DidNotReceiveWithAnyArgs().Remove(null);
        }

        [Fact]
        public async Task RemoveHealthRisk_WhenHealthRiskDoesNotExist_ShouldReturnError()
        {
            // Arrange
            const int nonExistentHealthRiskId = 2;

            // Act
            var result = await _healthRiskService.Delete(nonExistentHealthRiskId);

            // Assert
            _nyssContextMock.HealthRisks.DidNotReceiveWithAnyArgs().Remove(null);
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.HealthRiskNotFound);
    }

        [Fact]
        public async Task RemoveHealthRisk_WhenHealthRiskContainsReports_ShouldReturnError()
        {
            var projectHealthRisks = new List<ProjectHealthRisk>
            {
                new ProjectHealthRisk
                {
                    HealthRiskId = HealthRiskId,
                    Reports = new List<Report>
                    {
                        new Report()
}
                }
            };

            var projectHealthRisksMockDbSet = projectHealthRisks.AsQueryable().BuildMockDbSet();
            _nyssContextMock.ProjectHealthRisks.Returns(projectHealthRisksMockDbSet);

            // Act
            var result = await _healthRiskService.Delete(HealthRiskId);

            // Assert
            _nyssContextMock.HealthRisks.DidNotReceiveWithAnyArgs().Remove(null);
            result.IsSuccess.ShouldBeFalse();
            result.Message.Key.ShouldBe(ResultKey.HealthRisk.HealthRiskContainsReports);
        }
    }
}
