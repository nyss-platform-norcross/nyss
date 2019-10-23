using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Services
{
    public interface IUserIdentityService
    {
        Task<IdentityUser> Login(string userName, string password);
        Task<ICollection<string>> GetRoles(IdentityUser user);
        string CreateToken(string userName, IEnumerable<string> roles);
        Task Logout();
    }

    public class UserIdentityService : IUserIdentityService
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly NyssConfig _config;

        public UserIdentityService(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            NyssConfig config)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
        }

        public async Task<IdentityUser> Login(string userName, string password)
        {
            var signInResult = await _signInManager.PasswordSignInAsync(userName, password, true, false);

            if (!signInResult.Succeeded)
            {
                throw new ResultException("login.notSucceeded");
            }

            return await _userManager.FindByEmailAsync(userName);
        }
        
        public async Task Logout() =>
            await _signInManager.SignOutAsync();

        public async Task<ICollection<string>> GetRoles(IdentityUser user) =>
            await _userManager.GetRolesAsync(user);

        public string CreateToken(string userName, IEnumerable<string> roles)
        {
            var key = Encoding.ASCII.GetBytes(_config.Authentication.Secret);

            var nameClaims = new[] { new Claim(ClaimTypes.Name, userName) };
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));

            var securityKey = new SymmetricSecurityKey(key);

            var token = new JwtSecurityToken(
                issuer: _config.Authentication.Issuer,
                audience: _config.Authentication.Audience,
                expires: DateTime.UtcNow.AddDays(7),
                claims: nameClaims.Union(roleClaims),
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
