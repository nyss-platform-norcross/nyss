using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Services
{
    public interface IIdentityUserRegistrationService
    {
        Task<IdentityUser> CreateIdentityUser(string email, Role role);
        Task<string> GenerateEmailVerification(string email);
        Task<Result> VerifyEmail(string email, string verificationToken);
        Task<string> GeneratePasswordResetToken(string email);
        Task<Result> ResetPassword(string email, string verificationToken, string newPassword);
        Task<Result> AddPassword(string email, string newPassword);
    }

    public class IdentityUserRegistrationService : IIdentityUserRegistrationService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILoggerAdapter _loggerAdapter;

        public IdentityUserRegistrationService(UserManager<IdentityUser> userManager, 
            ILoggerAdapter loggerAdapter)
        {
            _userManager = userManager;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<IdentityUser> CreateIdentityUser(string email, Role role)
        {
            var identityUser = await AddIdentityUser(email);
            await AssignRole(email, role.ToString());

            return identityUser;
        }

        public async Task<string> GenerateEmailVerification(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<Result> VerifyEmail(string email, string verificationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Error(ResultKey.User.VerifyEmail.NotFound);
            }

            var confirmationResult = await _userManager.ConfirmEmailAsync(user, verificationToken);

            if (!confirmationResult.Succeeded)
            {
                return Error(ResultKey.User.VerifyEmail.Failed, string.Join(", " ,confirmationResult.Errors.Select(x => x.Description)));
            }

            return Success(true, ResultKey.User.VerifyEmail.Success, confirmationResult);
        }

        public async Task<string> GeneratePasswordResetToken(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<Result> AddPassword(string email, string newPassword)
        { 
            var user = await _userManager.FindByEmailAsync(email);
            var passwordAddResult = await _userManager.AddPasswordAsync(user, newPassword);
            
            var isPasswordTooWeak = passwordAddResult.Errors.Any(x => x.IsPasswordTooWeak());
            if (isPasswordTooWeak)
            { 
                return Error(ResultKey.User.Registration.PasswordTooWeak, string.Join(", ", passwordAddResult.Errors.Select(x => x.Description)));
            }

            if (!passwordAddResult.Succeeded)
            {
                return Error(ResultKey.User.VerifyEmail.AddPassword.Failed);
            }

            return Success(true, ResultKey.User.VerifyEmail.AddPassword.Success, passwordAddResult);
        }

        public async Task<Result> ResetPassword(string email, string verificationToken, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var passwordChangeResult = await _userManager.ResetPasswordAsync(user, verificationToken, newPassword);

            if (!passwordChangeResult.Succeeded)
            {
                throw new ResultException(ResultKey.User.ResetPassword.Failed, passwordChangeResult);
            }

            return Success(true, ResultKey.User.ResetPassword.Success, passwordChangeResult);
        }

        private async Task<IdentityUser> AddIdentityUser(string email, bool emailConfirmed = false)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                throw new ResultException(ResultKey.User.Registration.UserAlreadyExists);
            }

            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = emailConfirmed
            };

            var userCreationResult = await _userManager.CreateAsync(user);

            if (!userCreationResult.Succeeded)
            {
                var isPasswordTooWeak = userCreationResult.Errors.Any(x => x.IsPasswordTooWeak());
                if (isPasswordTooWeak)
                {
                    throw new ResultException(ResultKey.User.Registration.PasswordTooWeak);
                }

                var errorMessages = string.Join(",", userCreationResult.Errors.Select(x => x.Description));
                _loggerAdapter.Debug($"A user {email} could not be created. {errorMessages}");

                throw new ResultException(ResultKey.User.Registration.UnknownError);
            }

            return user;
        }

        private async Task AssignRole(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                throw new ResultException(ResultKey.User.Registration.UserNotFound);
            }

            var assignmentToRoleResult = await _userManager.AddToRoleAsync(user, role);

            if (!assignmentToRoleResult.Succeeded)
            { 
                if (assignmentToRoleResult.Errors.Any(x => x.Code == IdentityErrorCode.UserAlreadyInRole.ToString()))
                {
                    throw new ResultException(ResultKey.User.Registration.UserAlreadyInRole);
                }

                var errorMessages = string.Join(",", assignmentToRoleResult.Errors.Select(x => x.Description));
                _loggerAdapter.Debug($"A role {role} could not be assigned. {errorMessages}");

                throw new ResultException(ResultKey.User.Registration.UnknownError);
            }
        }
    }
}
