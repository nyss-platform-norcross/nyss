using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RX.Nyss.Common.Utils.DataContract;

namespace RX.Nyss.Web.Services
{
    public interface IUserIdentityService
    {
        Task<IdentityUser> Login(string userName, string password);
        Task<ICollection<string>> GetRoles(IdentityUser user);
        Task Logout();
    }

    public class UserIdentityService : IUserIdentityService
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public UserIdentityService(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<IdentityUser> Login(string userName, string password)
        {
            var signInResult = await _signInManager.PasswordSignInAsync(userName, password, true, true);

            if (signInResult.Succeeded)
            {
                return await _userManager.FindByEmailAsync(userName);
            }

            if (signInResult.IsLockedOut)
            {
                throw new ResultException(ResultKey.Login.LockedOut);
            }

            throw new ResultException(ResultKey.Login.NotSucceeded);
        }
        
        public async Task Logout() =>
            await _signInManager.SignOutAsync();

        public async Task<ICollection<string>> GetRoles(IdentityUser user) =>
            await _userManager.GetRolesAsync(user);
    }
}
