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
        Task<Result<StatusResponseDto>> GetStatus(ClaimsPrincipal user);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly INyssContext _nyssContext;
        private readonly IUserIdentityService _userIdentityService;

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
                return Success(new LoginResponseDto { AccessToken = accessToken });
            }
            catch (ResultException exception)
            {
                return exception.GetResult<LoginResponseDto>();
            }
        }

        public async Task<Result<StatusResponseDto>> GetStatus(ClaimsPrincipal user)
        {
            var email = user.FindFirstValue(ClaimTypes.Name);

            var userEntity = await _nyssContext.Users
                .Include(x => x.ApplicationLanguage)
                .SingleOrDefaultAsync(u => u.EmailAddress == email);

            if (userEntity == null)
            {
                return Error<StatusResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            var pendingSocieties = await _nyssContext.NationalSocieties
                .Where(ns => ns.PendingHeadManager.IdentityUserId == userEntity.IdentityUserId).AnyAsync();

            return Success(new StatusResponseDto
            {
                IsAuthenticated = user.Identity.IsAuthenticated,
                Data = user.Identity.IsAuthenticated
                    ? new StatusResponseDto.DataDto
                    {
                        Name = user.Identity.Name,
                        Email = email,
                        LanguageCode = userEntity.ApplicationLanguage?.LanguageCode ?? "en",
                        Roles = user.FindAll(m => m.Type == ClaimTypes.Role).Select(x => x.Value).ToArray(),
                        HasPendingHeadManagerConsents = pendingSocieties
                        HomePage = user.Identity.IsAuthenticated
                            ? GetHomePageData(userEntity)
                            : null
                    }
                    : null
            });
        }

        public async Task<Result> Logout()
        {
            await _userIdentityService.Logout();
            return Success();
        }

        private StatusResponseDto.HomePageDto GetHomePageData(Nyss.Data.Models.User userEntity) =>
            userEntity switch
            {
                SupervisorUser user => GetNationalSocietyHomePage(user),
                ManagerUser user => GetNationalSocietyHomePage(user),
                TechnicalAdvisorUser user => GetNationalSocietyHomePage(user),
                DataConsumerUser user => GetNationalSocietyHomePage(user),
                _ => GetRootHomePage()
            };

        private StatusResponseDto.HomePageDto GetNationalSocietyHomePage<T>(T user) where T : Nyss.Data.Models.User
        {
            var nationalSocietyIds = _nyssContext.UserNationalSocieties
                .Where(uns => uns.UserId == user.Id)
                .Select(uns => (int?) uns.NationalSocietyId)
                .ToList();

            if (!nationalSocietyIds.Any() || nationalSocietyIds.Count > 1)
            {
                return new StatusResponseDto.HomePageDto { Page = HomePageType.Root };
            }
            
            return new StatusResponseDto.HomePageDto
            {
                Page = HomePageType.NationalSociety,
                NationalSocietyId = nationalSocietyIds.Single()
            };
        }

        private StatusResponseDto.HomePageDto GetRootHomePage() =>
            new StatusResponseDto.HomePageDto { Page = HomePageType.Root };

        private async Task<IEnumerable<string>> GetRoles(IdentityUser user) => await _userIdentityService.GetRoles(user);

        private async Task<IEnumerable<Claim>> GetAdditionalClaims(IdentityUser identityUser) => await GetNationalSocietyClaims(identityUser);

        private async Task<List<Claim>> GetNationalSocietyClaims(IdentityUser identityUser) =>
            await _nyssContext.UserNationalSocieties
                .Where(uns => uns.User.IdentityUserId == identityUser.Id)
                .Select(uns => new Claim(ClaimType.ResourceAccess, $"{ResourceType.NationalSociety}:{uns.NationalSocietyId}"))
                .ToListAsync();

        private async Task<List<Nyss.Data.Models.NationalSociety>> GetPendingSocieties(IdentityUser user) =>
            await _nyssContext.NationalSocieties
                .Where(ns => ns.PendingHeadManager.IdentityUserId == user.Id)
                .ToListAsync();
    }
}
