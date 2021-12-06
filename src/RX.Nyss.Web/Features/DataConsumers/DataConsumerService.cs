using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.DataConsumers.Dto;
using RX.Nyss.Web.Services;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.DataConsumers
{
    public interface IDataConsumerService
    {
        Task<Result> Create(int nationalSocietyId, CreateDataConsumerRequestDto createDataConsumerRequestDto);

        Task<Result<GetDataConsumerResponseDto>> Get(int dataConsumerId);

        Task<Result> Edit(int dataConsumerId, EditDataConsumerRequestDto editDataConsumerRequestDto);
    }

    public class DataConsumerService : IDataConsumerService
    {
        private readonly ILoggerAdapter _loggerAdapter;

        private readonly INyssContext _dataContext;

        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;

        private readonly INationalSocietyUserService _nationalSocietyUserService;

        private readonly IVerificationEmailService _verificationEmailService;

        public DataConsumerService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext dataContext,
            ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;
            _nationalSocietyUserService = nationalSocietyUserService;
        }

        public async Task<Result> Create(int nationalSocietyId, CreateDataConsumerRequestDto createDataConsumerRequestDto)
        {
            try
            {
                string securityStamp;
                DataConsumerUser user;
                ICollection<Organization> organizations;

                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createDataConsumerRequestDto.Email, Role.DataConsumer);
                    securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                    (user, organizations) = await CreateDataConsumerUser(identityUser, nationalSocietyId, createDataConsumerRequestDto);

                    transactionScope.Complete();
                }

                await _verificationEmailService.SendVerificationForDataConsumersEmail(user, string.Join(", ", organizations.Select(o => o.Name)), securityStamp);

                return Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task<Result<GetDataConsumerResponseDto>> Get(int nationalSocietyUserId)
        {
            var dataConsumer = await _dataContext.Users.FilterAvailable()
                .OfType<DataConsumerUser>()
                .Where(u => u.Id == nationalSocietyUserId)
                .Select(u => new GetDataConsumerResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Role = u.Role,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    AdditionalPhoneNumber = u.AdditionalPhoneNumber,
                    Organization = u.Organization
                })
                .SingleOrDefaultAsync();

            if (dataConsumer == null)
            {
                _loggerAdapter.Debug($"Data consumer with id {nationalSocietyUserId} was not found");
                return Error<GetDataConsumerResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetDataConsumerResponseDto>(dataConsumer, true);
        }

        public async Task<Result> Edit(int dataConsumerId, EditDataConsumerRequestDto editDataConsumerRequestDto)
        {
            try
            {
                var user = await _nationalSocietyUserService.GetNationalSocietyUser<DataConsumerUser>(dataConsumerId);

                user.Name = editDataConsumerRequestDto.Name;
                user.PhoneNumber = editDataConsumerRequestDto.PhoneNumber;
                user.Organization = editDataConsumerRequestDto.Organization;
                user.AdditionalPhoneNumber = editDataConsumerRequestDto.AdditionalPhoneNumber;

                await _dataContext.SaveChangesAsync();
                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task<(DataConsumerUser user, ICollection<Organization> Organizations)> CreateDataConsumerUser(IdentityUser identityUser, int nationalSocietyId, CreateDataConsumerRequestDto createDataConsumerRequestDto)
        {
            var nationalSociety = await _dataContext.NationalSocieties
                .Include(ns => ns.ContentLanguage)
                .Include(ns => ns.Organizations)
                .SingleOrDefaultAsync(ns => ns.Id == nationalSocietyId);

            if (nationalSociety == null)
            {
                throw new ResultException(ResultKey.User.Registration.NationalSocietyDoesNotExist);
            }

            if (nationalSociety.IsArchived)
            {
                throw new ResultException(ResultKey.User.Registration.CannotCreateUsersInArchivedNationalSociety);
            }

            var defaultUserApplicationLanguage = await _dataContext.ApplicationLanguages
                .SingleOrDefaultAsync(al => al.LanguageCode == nationalSociety.ContentLanguage.LanguageCode);

            var user = new DataConsumerUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = createDataConsumerRequestDto.Name,
                PhoneNumber = createDataConsumerRequestDto.PhoneNumber,
                AdditionalPhoneNumber = createDataConsumerRequestDto.AdditionalPhoneNumber,
                Organization = createDataConsumerRequestDto.Organization,
                ApplicationLanguage = defaultUserApplicationLanguage
            };

            var userNationalSociety = CreateUserNationalSocietyReference(nationalSociety, user);

            await _dataContext.AddAsync(userNationalSociety);
            await _dataContext.SaveChangesAsync();

            return (user, nationalSociety.Organizations);
        }

        private UserNationalSociety CreateUserNationalSocietyReference(NationalSociety nationalSociety, User user) =>
            new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };
    }
}
