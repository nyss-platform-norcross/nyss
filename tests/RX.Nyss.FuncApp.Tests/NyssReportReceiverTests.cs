using System.Net;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RX.Nyss.FuncApp.Configuration;
using RX.Nyss.FuncApp.Contracts;
using RX.Nyss.FuncApp.Services;
using Shouldly;
using Xunit;

namespace RX.Nyss.FuncApp.Tests
{
    public class NyssReportReceiverTests
    {
        private readonly IReportPublisherService _reportPublisherService;
        private readonly NyssReportReceiver _receiver;
        private readonly FunctionContext _functionContext;

        public NyssReportReceiverTests()
        {
            _reportPublisherService = Substitute.For<IReportPublisherService>();
            _functionContext = Substitute.For<FunctionContext>();

            _receiver = new NyssReportReceiver(
                Substitute.For<ILogger<NyssReportReceiver>>(),
                Substitute.For<IConfig>(),
                _reportPublisherService,
                Substitute.For<BlobServiceClient>());
        }

        [Fact]
        public async Task EnqueueNyssReport_WhenBodyIsEmpty_ShouldReturnBadRequest()
        {
            var request = new FakeHttpRequestData(_functionContext, string.Empty);

            var response = await _receiver.EnqueueNyssReport(request);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            await _reportPublisherService.DidNotReceive().AddReportToQueue(Arg.Any<Report>());
        }

        [Fact]
        public async Task EnqueueNyssReport_WhenBodyIsWhitespace_ShouldReturnBadRequest()
        {
            var request = new FakeHttpRequestData(_functionContext, "   \n\t  ");

            var response = await _receiver.EnqueueNyssReport(request);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            await _reportPublisherService.DidNotReceive().AddReportToQueue(Arg.Any<Report>());
        }
    }
}
