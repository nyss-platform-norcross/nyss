using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using RX.Nyss.Web.Features.Authentication.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;


namespace RX.Nyss.Web.Features.Authentication
{
    public interface IAuthenticationService
    {
        Task<Result<LoginResponseDto>> Login(LoginRequestDto dto);
        Task<Result> Logout();
        StatusResponseDto GetStatus(ClaimsPrincipal user);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserIdentityService _userIdentityService;

        public AuthenticationService(IUserIdentityService userIdentityService)
        {
            _userIdentityService = userIdentityService;
        }

        public async Task<Result<LoginResponseDto>> Login(LoginRequestDto dto)
        {
            try
            {
                var user = await _userIdentityService.Login(dto.UserName, dto.Password);
                var roles = await _userIdentityService.GetRoles(user);
                var accessToken = _userIdentityService.CreateToken(user.UserName, roles);

                return Success(new LoginResponseDto
                {
                    AccessToken = accessToken
                });
            }
            catch (ResultException exception)
            {
                return exception.GetResult<LoginResponseDto>();
            }
        }

        public StatusResponseDto GetStatus(ClaimsPrincipal user) =>
            new StatusResponseDto
            {
                IsAuthenticated = user.Identity.IsAuthenticated,
                Data = user.Identity.IsAuthenticated
                    ? new StatusResponseDto.DataDto
                    {
                        Name = user.Identity.Name,
                        Email = user.FindFirstValue(ClaimTypes.Email),
                        Roles = user.FindAll(m => m.Type == ClaimTypes.Role).Select(x => x.Value).ToArray()
                    }
                    : null
            };

        public async Task<Result> Logout()
        {
            await _userIdentityService.Logout();
            return Success();
        }
    }
}
