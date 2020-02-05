using System.Reflection;
using System.Threading.Tasks;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
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
        private readonly INyssContext _nyssContext;
        private readonly INyssWebConfig _config;

        public FeedbackService(IEmailPublisherService emailPublisherService, INyssWebConfig config, INyssContext nyssContext, IAuthorizationService authorizationService, IDateTimeProvider dateTimeProvider)
        {
            _emailPublisherService = emailPublisherService;
            _config = config;
            _nyssContext = nyssContext;
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
                    <p>{sendFeedbackRequest.Message}</p>
                    <p>Browser used: {userAgent}</p>
                    <p>Nyss platform version: {version}</p>
                    <p>Date: {_dateTimeProvider.UtcNow:O}</p>";
                
                await _emailPublisherService.SendEmail((_config.FeedbackReceiverEmail, null), "Feedback from user of Nyss platform", anonymousFeedback);
                return Result.Success();
            }

            var currentUser = _authorizationService.GetCurrentUser();

            var feedback = $@"<p>{currentUser.Name} has sent the following feedback:</p><hr/>
                    <p>{sendFeedbackRequest.Message}</p>
                    <p>User's email: {currentUser.EmailAddress}</p>
                    <p>User's phone: {currentUser.PhoneNumber}</p>
                    <p>Browser used: {userAgent}</p>
                    <p>Nyss platform version: {version}</p>
                    <p>Date: {_dateTimeProvider.UtcNow:O}</p>";

            await _emailPublisherService.SendEmail((_config.FeedbackReceiverEmail, null), $"Feedback from {currentUser.Name} about the Nyss platform", feedback);
            return Result.Success();
        }
    }
}
