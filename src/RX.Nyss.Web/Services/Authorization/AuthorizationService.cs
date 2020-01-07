using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Services.Authorization
{
    public interface IAuthorizationService
    {
        CurrentUser GetCurrentUser();
        bool IsCurrentUserInRole(Role role);
        string GetCurrentUserName();
        bool IsCurrentUserInAnyRole(IEnumerable<Role> roles);
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
                Roles = roles.Select(Enum.Parse<Role>).ToList()
            };
        }

        public string GetCurrentUserName() =>
            _httpContextAccessor.HttpContext.User.Identity.Name;

        public bool IsCurrentUserInRole(Role role) =>
            _httpContextAccessor.HttpContext.User.IsInRole(role.ToString());

        public bool IsCurrentUserInAnyRole(IEnumerable<Role> roles) =>
            _httpContextAccessor.HttpContext.User.GetRoles().Any(userRole => roles.Any(role => role.ToString() == userRole));
    }
}
