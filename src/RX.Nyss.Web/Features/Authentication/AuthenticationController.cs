using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Web.Features.Authentication.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Authentication
{
    public class AuthenticationController : BaseController
    {
        private readonly IUserAuthenticationService _userAuthenticationService;

        public AuthenticationController(IUserAuthenticationService userAuthenticationService)
        {
            _userAuthenticationService = userAuthenticationService;
        }

        [Route("status"), HttpPost, AllowAnonymous]
        public IActionResult Status([FromBody]LoginInDto dto) =>
            Ok(new
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                Data = User.Identity.IsAuthenticated
                    ? new
                    {
                        Name = User.Identity.Name,
                        Email = User.FindFirstValue(ClaimTypes.Email),
                        Roles = User.FindAll(m => m.Type == ClaimTypes.Role).Select(x => x.Value).ToArray()
                    }
                    : null
            });

        [Route("login"), HttpPost, AllowAnonymous]
        public async Task<Result> Login([FromBody]LoginInDto dto) =>
            await _userAuthenticationService.Login(dto);

        [Route("logout"), HttpPost]
        public async Task<Result> Logout() =>
            await _userAuthenticationService.Logout();
    }
}
