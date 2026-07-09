using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.NSubstitute;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.ReportApi.Features.Alerts;
using RX.Nyss.ReportApi.Features.Reports;
using RX.Nyss.ReportApi.Features.Reports.Exceptions;
using RX.Nyss.ReportApi.Features.Reports.Handlers;
using RX.Nyss.ReportApi.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.ReportApi.Tests.Features.Reports.Handlers;

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
        var alertNotificationServiceMock = Substitute.For<IAlertNotificationService>();

        _smsEagleHandler = new SmsEagleHandler(reportMessageServiceMock, _nyssContextMock, _loggerAdapterMock, _dateTimeProviderMock,
            stringsResourcesServiceMock, queuePublisherServiceMock, alertServiceMock, reportValidationServiceMock, alertNotificationServiceMock);
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

    [Fact(Skip = "A Data Collector can now be registered without a phonenumber")]
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

    [Fact]
    public async Task Handle_WhenDuplicateMessageIdExists_ShouldNotSaveReport()
    {
        // Arrange
        var incomingMessageId = 1;
        var apiKey = "apikeytest";
        var rawReports = new List<RawReport>()
        {
            new RawReport
            {
                IncomingMessageId = incomingMessageId,
                ApiKey = apiKey
            }
        };
        var rawReportsDbSet = rawReports.AsQueryable().BuildMockDbSet();
        _nyssContextMock.RawReports.Returns(rawReportsDbSet);

        //Act
        await _smsEagleHandler.Handle($"msgid={incomingMessageId}&&apikey={apiKey}");

        //Assert
        _nyssContextMock.RawReports.Count().ShouldBe(1);
        await _nyssContextMock.RawReports.Received(0).AddAsync(Arg.Any<RawReport>());
    }
}
