using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataManager.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Features.DataManager
{
    public interface IDataManagerService
    {
        Task<Result> CreateDataManager(int nationalSocietyId, CreateDataManagerRequestDto createDataManagerRequestDto);
        Task<Result> GetDataManager(int dataManagerId);
        Task<Result> UpdateDataManager(int dataManagerId, EditDataManagerRequestDto editDataManagerRequestDto); 
        Task<Result> DeleteDataManager(int dataManagerId);
    }

    public class DataManagerService : IDataManagerService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly INationalSocietyUserService _nationalSocietyUserService;

        public DataManagerService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext dataContext, ILoggerAdapter loggerAdapter)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _nationalSocietyUserService = nationalSocietyUserService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<Result> CreateDataManager(int nationalSocietyId, CreateDataManagerRequestDto createDataManagerRequestDto)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createDataManagerRequestDto.Email, Role.DataManager);
                await CreateDataManagerUser(identityUser, nationalSocietyId, createDataManagerRequestDto);

                transactionScope.Complete();
                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task CreateDataManagerUser(IdentityUser identityUser, int nationalSocietyId, CreateDataManagerRequestDto createDataManagerRequestDto)
        {
            var nationalSociety = await _dataContext.NationalSocieties.FindAsync(nationalSocietyId);
            if (nationalSociety == null)
            {
                throw new ResultException(ResultKey.User.Registration.NationalSocietyDoesNotExist);
            }

            var user = new DataManagerUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = createDataManagerRequestDto.Name,
                PhoneNumber = createDataManagerRequestDto.PhoneNumber,
                AdditionalPhoneNumber = createDataManagerRequestDto.AdditionalPhoneNumber,
                Organization = createDataManagerRequestDto.Organization
            };

            var userNationalSociety = CreateUserNationalSocietyReference(nationalSociety, user);

            await _dataContext.AddAsync(userNationalSociety);
            await _dataContext.SaveChangesAsync();
        }

        private UserNationalSociety CreateUserNationalSocietyReference(Nyss.Data.Models.NationalSociety nationalSociety, Nyss.Data.Models.User user) =>
            new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };

        public async Task<Result> GetDataManager(int nationalSocietyUserId)
        {
            var dataManager = await _dataContext.Users
                .OfType<DataManagerUser>()
                .Where(u => u.Id == nationalSocietyUserId)
                .Select(u => new GetDataManagerResponseDto()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    AdditionalPhoneNumber = u.AdditionalPhoneNumber,
                    Organization = u.Organization,
                })
                .SingleOrDefaultAsync();

            if (dataManager == null)
            {
                _loggerAdapter.Debug($"Data manager with id {nationalSocietyUserId} was not found");
                return Result.Error(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetDataManagerResponseDto>(dataManager, true);
        }

        public async Task<Result> UpdateDataManager(int dataManagerId, EditDataManagerRequestDto editDataManagerRequestDto)
        {
            try
            {
                var user = await _nationalSocietyUserService.GetNationalSocietyUser<DataManagerUser>(dataManagerId);

                user.Name = editDataManagerRequestDto.Name;
                user.PhoneNumber = editDataManagerRequestDto.PhoneNumber;
                user.Organization = editDataManagerRequestDto.Organization;

                await _dataContext.SaveChangesAsync();
                return Result.Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public Task<Result> DeleteDataManager(int dataManagerId) =>
            _nationalSocietyUserService.DeleteUser<DataManagerUser>(dataManagerId);
    }
}

