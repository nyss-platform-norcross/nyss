using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.DataContract;

using RX.Nyss.Web.Models;

namespace RX.Nyss.Web.Features.User
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<Result> RegisterUser(string email, Role role)
        {
            try
            {
                var randomPassword = Guid.NewGuid().ToString();
                await AddUser(email, randomPassword);
                await AssignRole(email, role.ToString());

                //ToDo: send an email with a link to set a password
                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                //ToDo: Add log
                return e.Result;
            }
        }

        public async Task EnsureRoleExists(string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleCreationResult = await _roleManager.CreateAsync(new IdentityRole(role));

                if (!roleCreationResult.Succeeded)
                {
                    ResultException.Throw(ResultKey.User.Seeding.RoleCouldNotBeCreated, 
                        new {RoleName=role, ErrorMessages = roleCreationResult.Errors });
                }
            }
        }

        public async Task AddUser(string email, string password, bool emailConfirmed = false)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                ResultException.Throw(ResultKey.User.Registration.UserAlreadyExists);
            }

            user = new ApplicationUser
            {
                //ToDo: random number / guid or real email address?
                UserName = email,
                Email = email,
                EmailConfirmed = emailConfirmed
            };

            var userCreationResult = await _userManager.CreateAsync(user, password);

            if (!userCreationResult.Succeeded)
            {
                var isPasswordTooWeak = userCreationResult.Errors.Any(x => x.IsPasswordTooWeak());
                if (isPasswordTooWeak)
                {
                    ResultException.Throw(ResultKey.User.Registration.PasswordTooWeak);
                }

                ResultException.Throw(ResultKey.User.Registration.UnknownError);

                //ToDo: add logger
                //var errorMessages = string.Join(",", userCreationResult.Errors.Select(x => x.Description));
                //throw new Exception($"The {email} user could not be created. {errorMessages}");
            }
        }

        public async Task AssignRole(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ResultException.Throw(ResultKey.User.Registration.UserNotFound);
            }

            var assignmentToRoleResult = await _userManager.AddToRoleAsync(user, role);

            if (!assignmentToRoleResult.Succeeded)
            { 
                if (assignmentToRoleResult.Errors.Any(x => x.Code == IdentityErrorCode.UserAlreadyInRole.ToString()))
                {
                    ResultException.Throw(ResultKey.User.Registration.UserAlreadyInRole);
                }

                ResultException.Throw(ResultKey.User.Registration.UnknownError);
            }
            //ToDo: add logger
            //var errorMessages = string.Join(",", assignmentToRoleResult.Errors.Select(x => x.Description));
            //throw new Exception($"The {role} role could not be assigned. {errorMessages}");
        }
    }
}
