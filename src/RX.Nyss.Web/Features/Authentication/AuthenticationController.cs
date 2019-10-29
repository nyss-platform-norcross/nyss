using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Web.Features.Authentication.Dto;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Authentication
{
    [Route("api/authentication")]
    public class AuthenticationController : BaseController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [Route("status"), HttpGet, AllowAnonymous]
        public Result<StatusResponseDto> Status() =>
            _authenticationService.GetStatus(User);

        [Route("login"), HttpPost, AllowAnonymous]
        public async Task<Result<LoginResponseDto>> Login([FromBody]LoginRequestDto dto) =>
            await _authenticationService.Login(dto);

        [Route("logout"), HttpPost]
        public async Task<Result> Logout() =>
            await _authenticationService.Logout();
    }
}
