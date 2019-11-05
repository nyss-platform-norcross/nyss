using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.DataConsumer.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Features.DataConsumer
{
    public interface IDataConsumerService
    {
        Task<Result> CreateDataConsumer(int nationalSocietyId, CreateDataConsumerRequestDto createDataConsumerRequestDto);
        Task<Result> GetDataConsumer(int dataConsumerId);
        Task<Result> UpdateDataConsumer(int dataConsumerId, EditDataConsumerRequestDto editDataConsumerRequestDto);
        Task<Result> DeleteDataConsumer(int dataConsumerId);
    }

    public class DataConsumerService : IDataConsumerService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailService;

        public DataConsumerService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext dataContext, ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;

            _nationalSocietyUserService = nationalSocietyUserService;
        }

        public async Task<Result> CreateDataConsumer(int nationalSocietyId, CreateDataConsumerRequestDto createDataConsumerRequestDto)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createDataConsumerRequestDto.Email, Role.DataConsumer);
                var securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                await CreateDataConsumerUser(identityUser, nationalSocietyId, createDataConsumerRequestDto);

                transactionScope.Complete();

                await _verificationEmailService.SendVerificationEmail(createDataConsumerRequestDto.Email, createDataConsumerRequestDto.Name, securityStamp);
                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task CreateDataConsumerUser(IdentityUser identityUser, int nationalSocietyId, CreateDataConsumerRequestDto createDataConsumerRequestDto)
        {
            var nationalSociety = await _dataContext.NationalSocieties.FindAsync(nationalSocietyId);
            if (nationalSociety == null)
            {
                throw new ResultException(ResultKey.User.Registration.NationalSocietyDoesNotExist);
            }

            var user = new DataConsumerUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = createDataConsumerRequestDto.Name,
                PhoneNumber = createDataConsumerRequestDto.PhoneNumber,
                AdditionalPhoneNumber = createDataConsumerRequestDto.AdditionalPhoneNumber,
                Organization = createDataConsumerRequestDto.Organization
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

        public async Task<Result> GetDataConsumer(int nationalSocietyUserId)
        {
            var dataConsumer = await _dataContext.Users
                .OfType<DataConsumerUser>()
                .Where(u => u.Id == nationalSocietyUserId)
                .Select(u => new GetDataConsumerResponseDto()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    AdditionalPhoneNumber = u.AdditionalPhoneNumber,
                    Organization = u.Organization,
                })
                .SingleOrDefaultAsync();

            if (dataConsumer == null)
            {
                _loggerAdapter.Debug($"Data consumer with id {nationalSocietyUserId} was not found");
                return Result.Error(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetDataConsumerResponseDto>(dataConsumer, true);
        }

        public async Task<Result> UpdateDataConsumer(int dataConsumerId, EditDataConsumerRequestDto editDataConsumerRequestDto)
        {
            try
            {
                var user = await _nationalSocietyUserService.GetNationalSocietyUser<DataConsumerUser>(dataConsumerId);

                user.Name = editDataConsumerRequestDto.Name;
                user.PhoneNumber = editDataConsumerRequestDto.PhoneNumber;
                user.Organization = editDataConsumerRequestDto.Organization;

                await _dataContext.SaveChangesAsync();
                return Result.Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public Task<Result> DeleteDataConsumer(int dataConsumerId) =>
            _nationalSocietyUserService.DeleteUser<DataConsumerUser>(dataConsumerId);
    }
}

