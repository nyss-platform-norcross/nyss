using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Web.Features.Feedback.Commands;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Feedback
{
    [Route("api/feedback")]
    public class FeedbackController : BaseController
    {
        [HttpPost("")]
        public async Task<Result> Create([FromBody] SendFeedbackCommand cmd) =>
            await Sender.Send(cmd);
    }
}
