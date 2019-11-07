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
        Task<User> GetNationalSocietyUser<T>(int nationalSocietyUserId) where T : User;
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

        public async Task<User> GetNationalSocietyUser<T>(int nationalSocietyUserId) where T: User
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

        public async Task<Result> DeleteUser<T>(int nationalSocietyUserId) where T : User
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var deletedDataConsumer = await DeleteNationalSocietyUser<T>(nationalSocietyUserId);
                await _identityUserRegistrationService.DeleteIdentityUser(deletedDataConsumer.IdentityUserId);

                transactionScope.Complete();
                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task<User> DeleteNationalSocietyUser<T>(int nationalSocietyUserId) where T : User
        {
            var nationalSocietyUser = await GetNationalSocietyUser<T>(nationalSocietyUserId);

            var userNationalSocieties = await _dataContext.UserNationalSocieties
                .Where(uns => uns.UserId == nationalSocietyUserId)
                .ToListAsync();

            _dataContext.UserNationalSocieties.RemoveRange(userNationalSocieties);
            _dataContext.Users.Remove(nationalSocietyUser);
            await _dataContext.SaveChangesAsync();

            return nationalSocietyUser;
        }
    }
}
