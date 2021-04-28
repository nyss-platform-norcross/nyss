using System;
using System.Collections.Generic;
using System.Linq;
using MockQueryable.NSubstitute;
using NSubstitute;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Alerts;
using RX.Nyss.ReportApi.Features.Reports;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Features.Reports.Handlers;
using RX.Nyss.ReportApi.Features.Reports.Models;
using RX.Nyss.ReportApi.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Features.Reports.Handlers
{
    public class SmsEagleHandlerTests
    {
        private readonly ISmsEagleHandler _smsEagleHandler;
        private readonly INyssContext _nyssContextMock;
        private readonly IDateTimeProvider _dateTimeProviderMock;
        private readonly ILoggerAdapter _loggerAdapterMock;

        public SmsEagleHandlerTests()
        {
            var reportMessageServiceMock = Substitute.For<IReportMessageService>();
            _nyssContextMock = Substitute.For<INyssContext>();
            _loggerAdapterMock = Substitute.For<ILoggerAdapter>();
            _dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
            var stringsResourcesServiceMock = Substitute.For<IStringsResourcesService>();
            var queuePublisherServiceMock = Substitute.For<IQueuePublisherService>();
            var alertServiceMock = Substitute.For<IAlertService>();
            var reportValidationServiceMock = Substitute.For<IReportValidationService>();

            _smsEagleHandler = new SmsEagleHandler(reportMessageServiceMock, _nyssContextMock, _loggerAdapterMock, _dateTimeProviderMock,
                stringsResourcesServiceMock, queuePublisherServiceMock, alertServiceMock, reportValidationServiceMock);
        }

        [Fact]
        public void ValidateGatewaySetting_WhenApiKeyExists_ShouldNotThrowException()
        {
            // Arrange
            var apiKey = "api-key";
            var gatewaySettings = new List<GatewaySetting>
            {
                new GatewaySetting
                {
                    ApiKey = apiKey,
                    GatewayType = GatewayType.SmsEagle
                }
            };
            var gatewaySettingsDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsDbSet);

            // Assert
            Should.NotThrow(() => _smsEagleHandler.ValidateGatewaySetting(apiKey));
        }

        [Fact]
        public void ValidateGatewaySetting_WhenGatewayTypeDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var apiKey = "api-key";
            var gatewaySettings = new List<GatewaySetting>();
            var gatewaySettingsDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsDbSet);

            // Assert
            Should.Throw<ReportValidationException>(() => _smsEagleHandler.ValidateGatewaySetting(apiKey));
        }

        [Fact]
        public void ValidateGatewaySetting_WhenGatewayTypeIsNotSmsEagle_ShouldThrowException()
        {
            // Arrange
            var apiKey = "api-key";
            var gatewaySettings = new List<GatewaySetting>
            {
                new GatewaySetting
                {
                    ApiKey = apiKey,
                    GatewayType = GatewayType.Unknown
                }
            };
            var gatewaySettingsDbSet = gatewaySettings.AsQueryable().BuildMockDbSet();
            _nyssContextMock.GatewaySettings.Returns(gatewaySettingsDbSet);

            // Assert
            Should.Throw<ReportValidationException>(() => _smsEagleHandler.ValidateGatewaySetting(apiKey));
        }

        [Fact]
        public void ValidateDataCollector_WhenDataCollectorExists_ShouldNotThrowException()
        {
            // Arrange
            var phoneNumber = "+48123456789";
            var gatewayNationalSocietyId = 1;
            var dataCollectors = new List<DataCollector>
            {
                new DataCollector
                {
                    PhoneNumber = phoneNumber,
                    Project = new Project { NationalSocietyId = gatewayNationalSocietyId }
                }
            };
            var dataCollectorsDbSet = dataCollectors.AsQueryable().BuildMockDbSet();
            _nyssContextMock.DataCollectors.Returns(dataCollectorsDbSet);
            DataCollector validateDataCollectorResult = null;

            // Assert
            Should.NotThrow(async () => validateDataCollectorResult = await _smsEagleHandler.ValidateDataCollector(phoneNumber, gatewayNationalSocietyId));
            validateDataCollectorResult.ShouldBe(dataCollectors.First());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateDataCollector_WhenPhoneNumberIsNullOrEmpty_ShouldThrowException(string phoneNumber)
        {
            // Arrange
            var gatewayNationalSocietyId = 1;

            // Assert
            Should.Throw<ReportValidationException>(() => _smsEagleHandler.ValidateDataCollector(phoneNumber, gatewayNationalSocietyId));
        }

        [Fact]
        public void ValidateDataCollector_WhenDataCollectorDoesNotExist_ShouldThrowException()
        {
            // Arrange
            var phoneNumber = "+48123456789";
            var gatewayNationalSocietyId = 1;
            var dataCollectors = new List<DataCollector>();
            var dataCollectorsDbSet = dataCollectors.AsQueryable().BuildMockDbSet();
            _nyssContextMock.DataCollectors.Returns(dataCollectorsDbSet);

            // Assert
            Should.Throw<ReportValidationException>(() => _smsEagleHandler.ValidateDataCollector(phoneNumber, gatewayNationalSocietyId));
        }

        [Fact]
        public void ValidateDataCollector_WhenDataCollectorsNationalSocietyIsDifferentFromSmsGateways_ShouldThrowException()
        {
            // Arrange
            var phoneNumber = "+48123456789";
            var gatewayNationalSocietyId = 1;
            var dataCollectors = new List<DataCollector>
            {
                new DataCollector
                {
                    PhoneNumber = phoneNumber,
                    Project = new Project { NationalSocietyId = 2 }
                }
            };
            var dataCollectorsDbSet = dataCollectors.AsQueryable().BuildMockDbSet();
            _nyssContextMock.DataCollectors.Returns(dataCollectorsDbSet);

            // Assert
            Should.Throw<ReportValidationException>(() => _smsEagleHandler.ValidateDataCollector(phoneNumber, gatewayNationalSocietyId));
        }
    }
}
