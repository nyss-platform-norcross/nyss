using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Services
{
    public interface INationalSocietyUserService
    {
        Task<Result> DeleteUser<T>(int nationalSocietyUserId) where T : User;
        Task<T> GetNationalSocietyUser<T>(int nationalSocietyUserId) where T : User;
        Task DeleteNationalSocietyUser<T>(T nationalSocietyUser) where T : User;
        Task<T> GetNationalSocietyUserIncludingNationalSocieties<T>(int nationalSocietyUserId) where T : User;
    }

    public class NationalSocietyUserService : INationalSocietyUserService
    {
        private readonly INyssContext _dataContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;

        public NationalSocietyUserService(INyssContext dataContext, ILoggerAdapter loggerAdapter, IIdentityUserRegistrationService identityUserRegistrationService)
        {
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _identityUserRegistrationService = identityUserRegistrationService;
        }

        public async Task<T> GetNationalSocietyUser<T>(int nationalSocietyUserId) where T : User
        {
            var nationalSocietyUser = await _dataContext.Users
                .OfType<T>()
                .Where(u => u.Id == nationalSocietyUserId)
                .SingleOrDefaultAsync();

            if (nationalSocietyUser == null)
            {
                _loggerAdapter.Debug($"User with id {nationalSocietyUserId} and the role {typeof(T).ToString()} was not found");
                throw new ResultException(ResultKey.User.Common.UserNotFound);
            }

            return nationalSocietyUser;
        }

        public async Task<T> GetNationalSocietyUserIncludingNationalSocieties<T>(int nationalSocietyUserId) where T : User
        {
            var nationalSocietyUser = await _dataContext.Users
                .Include(u => u.UserNationalSocieties)
                .OfType<T>()
                .Where(u => u.Id == nationalSocietyUserId)
                .SingleOrDefaultAsync();

            if (nationalSocietyUser == null)
            {
                _loggerAdapter.Debug($"User with id {nationalSocietyUserId} and the role {typeof(T).ToString()} was not found");
                throw new ResultException(ResultKey.User.Common.UserNotFound);
            }

            return nationalSocietyUser;
        }


        public async Task<Result> DeleteUser<T>(int nationalSocietyUserId) where T : User
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var nationalSocietyUser = await GetNationalSocietyUserIncludingNationalSocieties<T>(nationalSocietyUserId);

                await DeleteNationalSocietyUser<T>(nationalSocietyUser);
                await _identityUserRegistrationService.DeleteIdentityUser(nationalSocietyUser.IdentityUserId);

                await _dataContext.SaveChangesAsync();

                transactionScope.Complete();
                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task DeleteNationalSocietyUser<T>(T nationalSocietyUser) where T : User
        {
            var userNationalSocieties = nationalSocietyUser.UserNationalSocieties;

            _dataContext.UserNationalSocieties.RemoveRange(userNationalSocieties);
            _dataContext.Users.Remove(nationalSocietyUser);
        }
    }
}
