using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.User.Dto;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Features.User
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;


        public UserService(UserManager<IdentityUser> userManager, 
            ILoggerAdapter loggerAdapter, 
            INyssContext dataContext)
        {
            _userManager = userManager;
            _loggerAdapter = loggerAdapter;
            _dataContext = dataContext;
        }

        public async Task<Result> RegisterGlobalCoordinator(GlobalCoordinatorInDto globalCoordinatorInDto)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var identityUser = await CreateIdentityUser(globalCoordinatorInDto.Email, Role.GlobalCoordinator);
                await CreateGlobalCoordinator(identityUser, globalCoordinatorInDto);
                
                transactionScope.Complete();

                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task CreateGlobalCoordinator(IdentityUser identityUser, GlobalCoordinatorInDto globalCoordinatorInDto)
        {
            var globalCoordinator = new GlobalCoordinatorUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = globalCoordinatorInDto.Name,
                PhoneNumber = globalCoordinatorInDto.PhoneNumber,
                Role = Role.GlobalCoordinator
            };

            await _dataContext.AddAsync(globalCoordinator);
            await _dataContext.SaveChangesAsync();
        }

        private async Task<IdentityUser> CreateIdentityUser(string email, Role role)
        {
            var identityUser = await AddIdentityUser(email);
            await AssignRole(email, role.ToString());

            return identityUser;
            //ToDo: send an email with a link to set a password
        }

        public async Task<IdentityUser> AddIdentityUser(string email, bool emailConfirmed = false)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                throw new ResultException(ResultKey.User.Registration.UserAlreadyExists);
            }

            user = new IdentityUser
            {
                //ToDo: random number / guid or real email address?
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

        public async Task AssignRole(string email, string role)
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
