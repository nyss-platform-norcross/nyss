using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Feedback
{
    [Route("api/Feedback")]
    public class FeedbackController : BaseController
    {
        private readonly IFeedbackService _feedbackService;
        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Send feedback
        /// </summary>
        /// <param name="sendFeedbackRequestDto"></param>
        /// <returns></returns>
        [HttpPost]
        [NeedsRole(Role.Administrator, Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor, Role.Supervisor)]
        public async Task<Result> Create([FromBody] SendFeedbackRequestDto sendFeedbackRequestDto) =>
            await _feedbackService.SendFeedback(sendFeedbackRequestDto, Request.Headers["User-Agent"].ToString());
    }

    public class SendFeedbackRequestDto
    {
        public string Message { get; set; }
        public bool IncludeContactDetails { get; set; }
    }
}
