using System;
using System.Net;
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
using RX.Nyss.Web.Features.GlobalCoordinator.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.GlobalCoordinator
{
    public interface IGlobalCoordinatorService
    {
        Task<Result> RegisterGlobalCoordinator(CreateGlobalCoordinatorRequestDto createGlobalCoordinatorRequestDto);
        Task<Result> UpdateGlobalCoordinator(EditGlobalCoordinatorRequestDto editGlobalCoordinatorRequestDto);
        Task<Result<GetGlobalCoordinatorResponseDto>> GetGlobalCoordinator(int globalCoordinatorId);
        Task<Result<List<GetGlobalCoordinatorResponseDto>>> GetGlobalCoordinators();
        Task<Result> RemoveGlobalCoordinator(int id);
    }

    public class GlobalCoordinatorService : IGlobalCoordinatorService
    {
        private readonly IConfig _config;
        private readonly INyssContext _dataContext;
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly ILoggerAdapter _loggerAdapter;

        public GlobalCoordinatorService(
            IIdentityUserRegistrationService identityUserRegistrationService,
            INyssContext dataContext,
            ILoggerAdapter loggerAdapter,
            IConfig config,
            IEmailPublisherService emailPublisherService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _config = config;
            _emailPublisherService = emailPublisherService;
        }

        public async Task<Result> RegisterGlobalCoordinator(CreateGlobalCoordinatorRequestDto createGlobalCoordinatorRequestDto)
        {
            try
            {
                var identityUser = await CreateUser(registerGlobalCoordinatorRequestDto);
                var securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                var baseUrl = new Uri(_config.BaseUrl);
                var verificationUrl = new Uri(baseUrl, $"verifyEmail?email={WebUtility.UrlEncode(registerGlobalCoordinatorRequestDto.Email)}&token={WebUtility.UrlEncode(securityStamp)}").ToString();
                var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createGlobalCoordinatorRequestDto.Email, Role.GlobalCoordinator);
                await CreateGlobalCoordinator(identityUser, createGlobalCoordinatorRequestDto);

                var (emailSubject, emailBody) = EmailTextGenerator.GenerateEmailVerificationEmail(
                    Role.GlobalCoordinator.ToString(),
                    verificationUrl,
                    registerGlobalCoordinatorRequestDto.Name);

                await _emailPublisherService.SendEmail((registerGlobalCoordinatorRequestDto.Email, registerGlobalCoordinatorRequestDto.Name), emailSubject, emailBody);

                return Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task<IdentityUser> CreateUser(RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto)
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var identityUser = await _identityUserRegistrationService.CreateIdentityUser(registerGlobalCoordinatorRequestDto.Email, Role.GlobalCoordinator);
            await CreateGlobalCoordinator(identityUser, registerGlobalCoordinatorRequestDto);

            transactionScope.Complete();
            return identityUser;
        }
        
        public async Task<Result> UpdateGlobalCoordinator(EditGlobalCoordinatorRequestDto editGlobalCoordinatorRequestDto)
        {
            var globalCoordinator = await _dataContext.Users
                .SingleOrDefaultAsync(u => u.Id == editGlobalCoordinatorRequestDto.Id && u.Role == Role.GlobalCoordinator);
            
            if (globalCoordinator == null)
            {
                _loggerAdapter.Debug($"Global coordinator with id {editGlobalCoordinatorRequestDto.Id} was not found");
                return Error(ResultKey.User.Common.UserNotFound);
            }

            globalCoordinator.Name = editGlobalCoordinatorRequestDto.Name;
            globalCoordinator.PhoneNumber = editGlobalCoordinatorRequestDto.PhoneNumber;
            globalCoordinator.AdditionalPhoneNumber = editGlobalCoordinatorRequestDto.AdditionalPhoneNumber;
            globalCoordinator.Organization = editGlobalCoordinatorRequestDto.Organization;

            await _dataContext.SaveChangesAsync();
            return Success();
        }

        public async Task<Result<GetGlobalCoordinatorResponseDto>> GetGlobalCoordinator(int globalCoordinatorId)
        {
            var globalCoordinator = await _dataContext.Users
                .Where(u => u.Id == globalCoordinatorId && u.Role == Role.GlobalCoordinator)
                .Select(u => new GetGlobalCoordinatorResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    AdditionalPhoneNumber = u.AdditionalPhoneNumber,
                    Organization = u.Organization
                })
                .SingleOrDefaultAsync();

            if (globalCoordinator == null)
            {
                _loggerAdapter.Debug($"Global coordinator with id {globalCoordinatorId} was not found");
                return Error<GetGlobalCoordinatorResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            return Success(globalCoordinator);
        }

        public async Task<Result<List<GetGlobalCoordinatorResponseDto>>> GetGlobalCoordinators()
        {
            var globalCoordinators = await _dataContext.Users
                .Where(u => u.Role == Role.GlobalCoordinator)
                .Select(u => new GetGlobalCoordinatorResponseDto
                {
                    Id =  u.Id,
                    Name = u.Name,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    AdditionalPhoneNumber = u.AdditionalPhoneNumber,
                    Organization = u.Organization
                })
                .ToListAsync();

            return Success(globalCoordinators);
        }

        public async Task<Result> RemoveGlobalCoordinator(int id)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var deletedGlobalCoordinator = await DeleteGlobalCoordinator(id);
                await _identityUserRegistrationService.DeleteIdentityUser(deletedGlobalCoordinator.IdentityUserId);
                
                transactionScope.Complete();

                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task<User> DeleteGlobalCoordinator(int id)
        {
            var globalCoordinator = await _dataContext.Users.FindAsync(id);

            if (globalCoordinator == null)
            {
                _loggerAdapter.Debug($"Global coordinator with id {id} was not found");
                throw new ResultException(ResultKey.User.Common.UserNotFound);
            }

            _dataContext.Users.Remove(globalCoordinator);
            await _dataContext.SaveChangesAsync();

            return globalCoordinator;
        }

        private async Task CreateGlobalCoordinator(IdentityUser identityUser, CreateGlobalCoordinatorRequestDto createGlobalCoordinatorRequestDto)
        {
            var globalCoordinator = new GlobalCoordinatorUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = createGlobalCoordinatorRequestDto.Name,
                PhoneNumber = createGlobalCoordinatorRequestDto.PhoneNumber,
                AdditionalPhoneNumber = createGlobalCoordinatorRequestDto.AdditionalPhoneNumber,
                Organization =  createGlobalCoordinatorRequestDto.Organization,
                Role = Role.GlobalCoordinator
            };

            await _dataContext.AddAsync(globalCoordinator);
            await _dataContext.SaveChangesAsync();
        }
    }
}
