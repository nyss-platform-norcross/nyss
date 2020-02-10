using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Feedback
{
    public interface IFeedbackService
    {
        Task<Result> SendFeedback(SendFeedbackRequestDto sendFeedbackRequest, string userAgent);
    }

    public class FeedbackService : IFeedbackService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly INyssWebConfig _config;

        public FeedbackService(IEmailPublisherService emailPublisherService,
            INyssWebConfig config,
            IAuthorizationService authorizationService,
            IDateTimeProvider dateTimeProvider)
        {
            _emailPublisherService = emailPublisherService;
            _config = config;
            _authorizationService = authorizationService;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Result> SendFeedback(SendFeedbackRequestDto sendFeedbackRequest, string userAgent)
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var version = assemblyName.Version;

            if (!sendFeedbackRequest.IncludeContactDetails)
            {
                var anonymousFeedback = $@"<p>An anonymous user has sent the following feedback:</p><hr/>
                    <p>{HtmlEncoder.Default.Encode(sendFeedbackRequest.Message)}</p>
                    <p>Browser used: {userAgent}</p>
                    <p>Nyss platform version: {version}</p>
                    <p>Date: {_dateTimeProvider.UtcNow:O}</p>";

                await _emailPublisherService.SendEmail((_config.FeedbackReceiverEmail, null), "Feedback from user of Nyss platform", body: anonymousFeedback);
                return Result.Success();
            }

            var currentUser = _authorizationService.GetCurrentUser();

            var feedback = $@"<p>{currentUser.Name} has sent the following feedback:</p><hr/>
                <p>{HtmlEncoder.Default.Encode(sendFeedbackRequest.Message)}</p>
                <p>User's email: {currentUser.EmailAddress}</p>
                <p>User's phone: {currentUser.PhoneNumber}</p>
                <p>Browser used: {userAgent}</p>
                <p>Nyss platform version: {version}</p>
                <p>Date: {_dateTimeProvider.UtcNow:O}</p>";

            await _emailPublisherService.SendEmail((_config.FeedbackReceiverEmail, null), $"Feedback from user {currentUser.Name} about the Nyss platform", body: feedback);
            return Result.Success();
        }
    }
}
