using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RX.Nyss.Data.Concepts;
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

        public async Task EnsureRoleExists(string role)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                var roleCreationResult = await _roleManager.CreateAsync(new IdentityRole(role));

                if (!roleCreationResult.Succeeded)
                {
                    var errorMessages = string.Join(",", roleCreationResult.Errors.Select(x => x.Description));
                    throw new Exception($"The {role} role could not be created. {errorMessages}");
                }
            }
        }

        public async Task<AddUserResult> AddUser(string email, string password, bool emailConfirmed = false)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                return AddUserResult.UserAlreadyExists;
            }

            user = new ApplicationUser
            {
                //ToDo: random number / guid or real email address?
                UserName = email,
                Email = email,
                EmailConfirmed = emailConfirmed
            };

            var userCreationResult = await _userManager.CreateAsync(user, password);

            if (userCreationResult.Succeeded)
            {
                return AddUserResult.Success;
            }

            var isPasswordTooWeak = userCreationResult.Errors.Any(x => x.IsPasswordTooWeak());
            if (isPasswordTooWeak)
            {
                return AddUserResult.PasswordTooWeak;
            }

            return AddUserResult.UnknownError;

            //ToDo: add logger
            //var errorMessages = string.Join(",", userCreationResult.Errors.Select(x => x.Description));
            //throw new Exception($"The {email} user could not be created. {errorMessages}");
        }

        public async Task<AssignRoleResult> AssignRole(string email, string role)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return AssignRoleResult.UserNotFound;
            }

            var assignmentToRoleResult = await _userManager.AddToRoleAsync(user, role);

            if (assignmentToRoleResult.Succeeded)
            {
                return AssignRoleResult.Success;
            }

            if (assignmentToRoleResult.Errors.Any(x => x.Code == IdentityErrorCode.UserAlreadyInRole.ToString()))
            {
                return AssignRoleResult.UserAlreadyInRole;
            }

            return AssignRoleResult.UnknownError; 
            
            //ToDo: add logger
            //var errorMessages = string.Join(",", assignmentToRoleResult.Errors.Select(x => x.Description));
            //throw new Exception($"The {role} role could not be assigned. {errorMessages}");
        }
    }
}
