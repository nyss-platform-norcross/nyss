using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Services.Authorization
{
    public interface IAuthorizationService
    {
        User GetCurrentUser();
        bool IsCurrentUserInRole(Role role);
        string GetCurrentUserName();
        bool IsCurrentUserInAnyRole(IEnumerable<Role> roles);
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INyssContext _nyssContext;

        public AuthorizationService(
            IHttpContextAccessor httpContextAccessor,
            INyssContext nyssContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _nyssContext = nyssContext;
        }

        public User GetCurrentUser()
        {
            var userName = GetCurrentUserName();
            return _nyssContext.Users.FilterAvailable().SingleOrDefault(u => u.EmailAddress == userName);
        }

        public string GetCurrentUserName() =>
            _httpContextAccessor.HttpContext.User.Identity.Name;

        public bool IsCurrentUserInRole(Role role) =>
            _httpContextAccessor.HttpContext.User.IsInRole(role.ToString());

        public bool IsCurrentUserInAnyRole(IEnumerable<Role> roles) =>
            _httpContextAccessor.HttpContext.User.GetRoles().Any(userRole => roles.Any(role => role.ToString() == userRole));
    }
}
