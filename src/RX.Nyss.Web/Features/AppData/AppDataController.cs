using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.AppData
{
    [Route("api/appdata")]
    public class AppDataController : BaseController
    {
        [HttpGet, Route("get")]
        public IActionResult Get() =>
            Ok(new
            {
                User = User.Identity.IsAuthenticated 
                    ? new { User.Identity.Name } 
                    : null
            });
    }
}
