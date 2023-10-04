using System.Linq;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
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

        private readonly UserManager<IdentityUser> _userManager;

        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly INyssWebConfig _config;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IEmailPublisherService _emailPublisherService;

        private readonly INyssContext _nyssContext;

        public Handler(
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager,
            IDateTimeProvider dateTimeProvider,
            INyssWebConfig config,
            IHttpContextAccessor httpContextAccessor,
            IEmailPublisherService emailPublisherService,
            INyssContext nyssContext)
        {
            _authorizationService = authorizationService;
            _userManager = userManager;
            _dateTimeProvider = dateTimeProvider;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _emailPublisherService = emailPublisherService;
            _nyssContext = nyssContext;
        }

        public async Task<Result> Handle(SendFeedbackCommand request, CancellationToken cancellationToken)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            var currentUser = await _authorizationService.GetCurrentUser();
            var currentIdentityUser = await _userManager.FindByIdAsync(currentUser.IdentityUserId);
            var roles = await _userManager.GetRolesAsync(currentIdentityUser);
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
            var nationalSocietyName = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.User == currentUser)
                .Select(uns => uns.NationalSociety.Name)
                .FirstOrDefaultAsync(cancellationToken);

            var messageBody = $@"<p>A user has sent the following feedback:</p><hr/>
                <p>{HtmlEncoder.Default.Encode(request.Message)}</p>
                <p>User's email: {currentUser.EmailAddress}</p>
                <p>User's national society: {nationalSocietyName}</p>
                <p>User's roles: {string.Join(", ", roles)}</p>
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
