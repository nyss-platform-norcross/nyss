using System.Threading.Tasks;
using RX.Nyss.Common.Services.StringsResources;
using RX.Nyss.Web.Features.Users;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Services
{
    public interface IStringsService
    {
        Task<StringsResourcesVault> GetForCurrentUser();
    }

    public class StringsService : IStringsService
    {
        private readonly IStringsResourcesService _stringsResourcesService;

        private readonly IAuthorizationService _authorizationService;

        private readonly IUserService _userService;

        private StringsResourcesVault _current;

        public StringsService(
            IStringsResourcesService stringsResourcesService,
            IAuthorizationService authorizationService,
            IUserService userService)
        {
            _stringsResourcesService = stringsResourcesService;
            _authorizationService = authorizationService;
            _userService = userService;
        }

        public async Task<StringsResourcesVault> GetForCurrentUser()
        {
            if (_current != null)
            {
                return _current;
            }

            var userApplicationLanguageCode = await _userService.GetUserApplicationLanguageCode(_authorizationService.GetCurrentUserName());

            _current = await _stringsResourcesService.GetStrings(userApplicationLanguageCode);

            return _current;
        }
    }
}
