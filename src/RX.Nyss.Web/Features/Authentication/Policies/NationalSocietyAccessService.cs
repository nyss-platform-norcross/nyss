using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public interface INationalSocietyAccessService
    {
        Task<bool> HasAccessToAnyOfResourceNationalSocieties(ClaimsPrincipal user, List<int> resourceNationalSocietyIds);
        Task<List<int>> GetUserNationalSocietyIds<T>(int userId) where T: User;
    }

    public class NationalSocietyAccessService : INationalSocietyAccessService
    {
        private readonly IEnumerable<string> _rolesWithAccessToAllNationalSocieties;
        protected readonly INyssContext _nyssContext;
        protected readonly UserManager<IdentityUser> _userManager;

        public NationalSocietyAccessService(INyssContext nyssContext, UserManager<IdentityUser> userManager)
        {
            _nyssContext = nyssContext;
            _userManager = userManager;
            _rolesWithAccessToAllNationalSocieties = new List<Role> { Role.Administrator, Role.GlobalCoordinator }
                .Select(role => role.ToString());
        }

        public async Task<bool> HasAccessToAnyOfResourceNationalSocieties(ClaimsPrincipal user, List<int> resourceNationalSocietyIds)
        {
            var userNationalSocietyIds = await GetUserNationalSocietyIds(user);
            var hasAccessToAnyOfResourceNationalSocieties = resourceNationalSocietyIds.Intersect(userNationalSocietyIds).Any();
            return hasAccessToAnyOfResourceNationalSocieties || HasAccessToAllNationalSocieties(user);
        }

        public Task<List<int>> GetUserNationalSocietyIds<T>(int userId) where T: User =>
            _nyssContext.Users
                .OfType<T>()
                .Where(u => u.Id == userId)
                .SelectMany(u => u.UserNationalSocieties)
                .Select(uns => uns.NationalSocietyId)
                .ToListAsync();


        private async Task<List<int>> GetUserNationalSocietyIds(ClaimsPrincipal user)
        {
            var identityUser = await _userManager.FindByNameAsync(user.Identity.Name);
            var identityUserId = identityUser.Id;
            var userNationalSocietyIds = await _nyssContext.Users
                    .Where(u => u.IdentityUserId == identityUserId)
                    .SelectMany(u => u.UserNationalSocieties)
                    .Select(uns => uns.NationalSocietyId)
                    .ToListAsync();
            return userNationalSocietyIds;
        }

        private bool HasAccessToAllNationalSocieties(ClaimsPrincipal user) =>
            user.Claims.Any(c => c.Type == ClaimTypes.Role && _rolesWithAccessToAllNationalSocieties.Contains(c.Value));
    }
}
