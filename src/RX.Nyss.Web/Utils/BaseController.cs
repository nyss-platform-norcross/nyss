using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RX.Nyss.Web.Utils
{
    [ApiController]
    [Authorize]
    public class BaseController : ControllerBase
    {
    }
}
