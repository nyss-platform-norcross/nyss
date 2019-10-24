using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Authentication.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils;
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
        private readonly INyssContext _nyssContext;

        public AuthenticationService(IUserIdentityService userIdentityService, INyssContext nyssContext)
        {
            _userIdentityService = userIdentityService;
            _nyssContext = nyssContext;
        }

        public async Task<Result<LoginResponseDto>> Login(LoginRequestDto dto)
        {
            try
            {
                var user = await _userIdentityService.Login(dto.UserName, dto.Password);
                var roles = await _userIdentityService.GetRoles(user);
                var additionalClaims = await GetAdditionalClaims(user);
                var accessToken = _userIdentityService.CreateToken(user.UserName, roles, additionalClaims);

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

        private async Task<IEnumerable<Claim>> GetAdditionalClaims(IdentityUser identityUser)
        {
            var isDataOwnerClaim = await GetIsDataOwnerClaim(identityUser);
            return new[] { isDataOwnerClaim };
        }

        private async Task<Claim> GetIsDataOwnerClaim(IdentityUser identityUser)
        {
            var isDataOwner = await GetUserIsDataOwner(identityUser);
            return new Claim(Policy.IsDataOwner.ToString(), isDataOwner ? "true" : "false");
        }

        private async Task<bool> GetUserIsDataOwner(IdentityUser identityUser)
        {
            var nyssDataManagerUser = await _nyssContext.Users
                .Where(u =>u.IdentityUserId == identityUser.Id)
                .Where(u => u is DataManagerUser)
                .Cast<DataManagerUser>()
                .SingleOrDefaultAsync();

            return nyssDataManagerUser != null && nyssDataManagerUser.IsDataOwner;
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
