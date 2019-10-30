using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
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
        Result<StatusResponseDto> GetStatus(ClaimsPrincipal user);
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
                var roles = await GetRoles(user);
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

        private async Task<IEnumerable<string>> GetRoles(IdentityUser user)
        {
            var userRoles = await _userIdentityService.GetRoles(user);

            return await IsDataOwner(user)
                ? userRoles.Union(new[] { FunctionalRole.DataOwner.ToString() })
                : userRoles;
        }

        public Result<StatusResponseDto> GetStatus(ClaimsPrincipal user) =>
            Success(new StatusResponseDto
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
            });

        public async Task<Result> Logout()
        {
            await _userIdentityService.Logout();
            return Success();
        }

        private async Task<IEnumerable<Claim>> GetAdditionalClaims(IdentityUser identityUser)
        {
            return await GetNationalSocietyClaims(identityUser);
        }

        private async Task<bool> IsDataOwner(IdentityUser identityUser) =>
            await _nyssContext.Users
                .OfType<DataManagerUser>()
                .AnyAsync(u => u.IdentityUserId == identityUser.Id && u.IsDataOwner);

        private async Task<List<Claim>> GetNationalSocietyClaims(IdentityUser identityUser) =>
            await _nyssContext.UserNationalSocieties
                .Where(uns => uns.User.IdentityUserId == identityUser.Id)
                .Select(uns => new Claim(ClaimType.ResourceAccess, $"{ResourceType.NationalSociety}:{uns.NationalSocietyId}"))
                .ToListAsync();
    }
}
