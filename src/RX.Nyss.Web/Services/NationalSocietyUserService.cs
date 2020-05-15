using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Services
{
    public interface INationalSocietyUserService
    {
        Task<Result> DeleteUser<T>(int nationalSocietyUserId, IEnumerable<string> deletingUserRoles) where T : User;
        Task<T> GetNationalSocietyUser<T>(int nationalSocietyUserId) where T : User;
        void DeleteNationalSocietyUser<T>(T nationalSocietyUser) where T : User;
        Task<T> GetNationalSocietyUserIncludingNationalSocieties<T>(int nationalSocietyUserId) where T : User;
        Task<T> GetNationalSocietyUserIncludingOrganizations<T>(int nationalSocietyUserId) where T : User;
    }

    public class NationalSocietyUserService : INationalSocietyUserService
    {
        private readonly INyssContext _dataContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly IDeleteUserService _deleteUserService;

        public NationalSocietyUserService(INyssContext dataContext, ILoggerAdapter loggerAdapter, IIdentityUserRegistrationService identityUserRegistrationService,
            IDeleteUserService deleteUserService)
        {
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _identityUserRegistrationService = identityUserRegistrationService;
            _deleteUserService = deleteUserService;
        }

        public async Task<T> GetNationalSocietyUser<T>(int nationalSocietyUserId) where T : User
        {
            var nationalSocietyUser = await _dataContext.Users.FilterAvailable()
                .OfType<T>()
                .Where(u => u.Id == nationalSocietyUserId)
                .SingleOrDefaultAsync();

            if (nationalSocietyUser == null)
            {
                _loggerAdapter.Warn($"User with id {nationalSocietyUserId} and the role {typeof(T)} was not found");
                throw new ResultException(ResultKey.User.Common.UserNotFound);
            }

            return nationalSocietyUser;
        }

        public async Task<T> GetNationalSocietyUserIncludingNationalSocieties<T>(int nationalSocietyUserId) where T : User
        {
            var nationalSocietyUser = await _dataContext.Users.FilterAvailable()
                .Include(u => u.UserNationalSocieties)
                .OfType<T>()
                .Where(u => u.Id == nationalSocietyUserId)
                .SingleOrDefaultAsync();

            if (nationalSocietyUser == null)
            {
                _loggerAdapter.Warn($"User with id {nationalSocietyUserId} and the role {typeof(T)} was not found");
                throw new ResultException(ResultKey.User.Common.UserNotFound);
            }

            return nationalSocietyUser;
        }
        public async Task<T> GetNationalSocietyUserIncludingOrganizations<T>(int nationalSocietyUserId) where T : User
        {
            var nationalSocietyUser = await _dataContext.Users
                .Include(u => u.UserNationalSocieties)
                .FilterAvailable()
                .OfType<T>()
                .Where(u => u.Id == nationalSocietyUserId)
                .SingleOrDefaultAsync();

            if (nationalSocietyUser == null)
            {
                _loggerAdapter.Warn($"User with id {nationalSocietyUserId} and the role {typeof(T)} was not found");
                throw new ResultException(ResultKey.User.Common.UserNotFound);
            }

            return nationalSocietyUser;
        }


        public async Task<Result> DeleteUser<T>(int nationalSocietyUserId, IEnumerable<string> deletingUserRoles) where T : User
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var nationalSocietyUser = await GetNationalSocietyUserIncludingNationalSocieties<T>(nationalSocietyUserId);
                await _deleteUserService.EnsureCanDeleteUser(nationalSocietyUserId, nationalSocietyUser.Role);

                DeleteNationalSocietyUser(nationalSocietyUser);
                await _identityUserRegistrationService.DeleteIdentityUser(nationalSocietyUser.IdentityUserId);

                await _dataContext.SaveChangesAsync();

                transactionScope.Complete();
                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Error(e);
                return e.Result;
            }
        }

        public void DeleteNationalSocietyUser<T>(T nationalSocietyUser) where T : User
        {
            var userNationalSocieties = nationalSocietyUser.UserNationalSocieties;

            _dataContext.UserNationalSocieties.RemoveRange(userNationalSocieties);
            _dataContext.Users.Remove(nationalSocietyUser);
        }
    }
}
