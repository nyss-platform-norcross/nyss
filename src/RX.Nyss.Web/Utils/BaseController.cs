using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Web.Utils.Filters;

namespace RX.Nyss.Web.Utils
{
    [ApiController]
    [Authorize]
    [SetCorrectResponseCode]
    public class BaseController : ControllerBase
    {
        private ISender _sender;

        protected ISender Sender => _sender ??= (ISender)HttpContext.RequestServices.GetService(typeof(ISender));
    }
}
