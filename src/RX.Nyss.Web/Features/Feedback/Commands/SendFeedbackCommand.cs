using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Feedback.Commands
{
    public class SendFeedbackCommand : IRequest<Result>
    {
        public string Message { get; set; }
    }

    public class Handler : IRequestHandler<SendFeedbackCommand, Result>
    {
        private readonly IAuthorizationService _authorizationService;

        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly INyssWebConfig _config;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IEmailPublisherService _emailPublisherService;

        public Handler(
            IAuthorizationService authorizationService,
            IDateTimeProvider dateTimeProvider,
            INyssWebConfig config,
            IHttpContextAccessor httpContextAccessor,
            IEmailPublisherService emailPublisherService)
        {
            _authorizationService = authorizationService;
            _dateTimeProvider = dateTimeProvider;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _emailPublisherService = emailPublisherService;
        }

        public async Task<Result> Handle(SendFeedbackCommand request, CancellationToken cancellationToken)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            var currentUser = await _authorizationService.GetCurrentUser();
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();

            var messageBody = $@"<p>{currentUser.Name} has sent the following feedback:</p><hr/>
                <p>{HtmlEncoder.Default.Encode(request.Message)}</p>
                <p>User's email: {currentUser.EmailAddress}</p>
                <p>User's phone: {currentUser.PhoneNumber}</p>
                <p>Browser used: {userAgent}</p>
                <p>Nyss platform version: {version}</p>
                <p>Date: {_dateTimeProvider.UtcNow:O}</p>";

            await _emailPublisherService.SendEmail(
                (_config.FeedbackReceiverEmail, null),
                "Feedback from user of Nyss platform",
                messageBody);

            return Result.Success();
        }
    }
}
