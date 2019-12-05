using System.Collections;
using System.Collections.Generic;
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
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.DataConsumer
{
    public interface IDataConsumerService
    {
        Task<Result> CreateDataConsumer(int nationalSocietyId, CreateDataConsumerRequestDto createDataConsumerRequestDto);
        Task<Result<GetDataConsumerResponseDto>> GetDataConsumer(int dataConsumerId);
        Task<Result> UpdateDataConsumer(int dataConsumerId, EditDataConsumerRequestDto editDataConsumerRequestDto);
        Task<Result> DeleteDataConsumer(int nationalSocietyId, int dataConsumerId, IEnumerable<string> deletingUserRoles);
    }

    public class DataConsumerService : IDataConsumerService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailService;
        
        private readonly IDeleteUserService _deleteService;

        public DataConsumerService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext dataContext, ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService, IDeleteUserService deleteService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;
            _deleteService = deleteService;
            _nationalSocietyUserService = nationalSocietyUserService;
        }

        public async Task<Result> CreateDataConsumer(int nationalSocietyId, CreateDataConsumerRequestDto createDataConsumerRequestDto)
        {
            try
            {
                string securityStamp;
                DataConsumerUser user;
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createDataConsumerRequestDto.Email, Role.DataConsumer);
                    securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                    user = await CreateDataConsumerUser(identityUser, nationalSocietyId, createDataConsumerRequestDto);

                    transactionScope.Complete();
                }
                await _verificationEmailService.SendVerificationEmail(user, securityStamp);
                return Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task<DataConsumerUser> CreateDataConsumerUser(IdentityUser identityUser, int nationalSocietyId, CreateDataConsumerRequestDto createDataConsumerRequestDto)
        {
            var nationalSociety = await _dataContext.NationalSocieties.Include(ns => ns.ContentLanguage)
                .SingleOrDefaultAsync(ns => ns.Id == nationalSocietyId);

            if (nationalSociety == null)
            {
                throw new ResultException(ResultKey.User.Registration.NationalSocietyDoesNotExist);
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
                ApplicationLanguage = defaultUserApplicationLanguage,
            };

            var userNationalSociety = CreateUserNationalSocietyReference(nationalSociety, user);

            await _dataContext.AddAsync(userNationalSociety);
            await _dataContext.SaveChangesAsync();
            
            return user;
        }

        private UserNationalSociety CreateUserNationalSocietyReference(Nyss.Data.Models.NationalSociety nationalSociety, Nyss.Data.Models.User user) =>
            new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };

        public async Task<Result<GetDataConsumerResponseDto>> GetDataConsumer(int nationalSocietyUserId)
        {
            var dataConsumer = await _dataContext.Users
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
                    Organization = u.Organization,
                })
                .SingleOrDefaultAsync();

            if (dataConsumer == null)
            {
                _loggerAdapter.Debug($"Data consumer with id {nationalSocietyUserId} was not found");
                return Error<GetDataConsumerResponseDto>(ResultKey.User.Common.UserNotFound);
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

        public async Task<Result> DeleteDataConsumer(int nationalSocietyId, int dataConsumerId, IEnumerable<string> deletingUserRoles)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var dataConsumerUser = await _nationalSocietyUserService.GetNationalSocietyUserIncludingNationalSocieties<DataConsumerUser>(dataConsumerId);
                _deleteService.EnsureHasPermissionsToDelteUser(dataConsumerUser.Role, deletingUserRoles);
                
                var userNationalSocieties = dataConsumerUser.UserNationalSocieties;

                var nationalSocietyReferenceToRemove = userNationalSocieties.SingleOrDefault(uns => uns.NationalSocietyId == nationalSocietyId);

                if (nationalSocietyReferenceToRemove == null)
                {
                    return Error(ResultKey.User.Registration.UserIsNotAssignedToThisNationalSociety);
                }

                var isUsersLastNationalSociety = (userNationalSocieties.Count == 1);
                _dataContext.UserNationalSocieties.Remove(nationalSocietyReferenceToRemove);

                if (isUsersLastNationalSociety)
                { 
                    _nationalSocietyUserService.DeleteNationalSocietyUser(dataConsumerUser);
                    await _identityUserRegistrationService.DeleteIdentityUser(dataConsumerUser.IdentityUserId);
                }

                await _dataContext.SaveChangesAsync();
                transactionScope.Complete();
                return Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }
    }
}

