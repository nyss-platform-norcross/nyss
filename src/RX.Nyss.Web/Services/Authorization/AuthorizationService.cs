using Microsoft.AspNetCore.Http;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Services.Authorization
{
    public interface IAuthorizationService
    {
        CurrentUser GetCurrentUser();
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthorizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public CurrentUser GetCurrentUser()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var identityName = httpContext.User.Identity.Name;
            var roles = httpContext.User.GetRoles();

            return new CurrentUser
            {
                Name = identityName,
                Roles = roles
            };
        }
    }
}
