using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.Authentication.Dto;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;


namespace RX.Nyss.Web.Features.Authentication
{
    public interface IUserAuthenticationService
    {
        Task<Result> Login(LoginInDto dto);
        Task<Result> Logout();
    }

    public class UserAuthenticationService : IUserAuthenticationService
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public UserAuthenticationService(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<Result> Login(LoginInDto dto)
        {
            var signInResult = await _signInManager.PasswordSignInAsync(dto.UserName, dto.Password, true, false);
            return signInResult.Succeeded
                ? Success("login.success")
                : Error("Error");
        }

        public async Task<Result> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Success("Works");
            }
            catch (Exception exception)
            {
                return Error("Error");
            }
        }
    }
}
