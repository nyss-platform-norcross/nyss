using System;
using System.Threading.Tasks;
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
    }
}
