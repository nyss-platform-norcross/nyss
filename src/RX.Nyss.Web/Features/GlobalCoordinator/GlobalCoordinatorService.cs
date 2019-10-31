using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
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
        Task<Result> RegisterGlobalCoordinator(CreateGlobalCoordinatorRequestDto dto);
        Task<Result> UpdateGlobalCoordinator(EditGlobalCoordinatorRequestDto dto);
        Task<Result<GetGlobalCoordinatorResponseDto>> GetGlobalCoordinator(int id);
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

        public async Task<Result> RegisterGlobalCoordinator(CreateGlobalCoordinatorRequestDto dto)
        {
            try
            {
                string securityStamp;

                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var identityUser = await _identityUserRegistrationService.CreateIdentityUser(dto.Email, Role.GlobalCoordinator);
                    securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                    await _dataContext.AddAsync(new GlobalCoordinatorUser
                    {
                        IdentityUserId = identityUser.Id,
                        EmailAddress = identityUser.Email,
                        Name = dto.Name,
                        PhoneNumber = dto.PhoneNumber,
                        AdditionalPhoneNumber = dto.AdditionalPhoneNumber,
                        Organization =  dto.Organization,
                        Role = Role.GlobalCoordinator
                    });

                    await _dataContext.SaveChangesAsync();
                    transactionScope.Complete();
                }

                await SendVerificationEmail(dto.Email, dto.Name, securityStamp);

                return Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task<Result> UpdateGlobalCoordinator(EditGlobalCoordinatorRequestDto dto)
        {
            var globalCoordinator = await _dataContext.Users
                .SingleOrDefaultAsync(u => u.Id == dto.Id && u.Role == Role.GlobalCoordinator);
            
            if (globalCoordinator == null)
            {
                _loggerAdapter.Debug($"Global coordinator with id {dto.Id} was not found");
                return Error(ResultKey.User.Common.UserNotFound);
            }

            globalCoordinator.Name = dto.Name;
            globalCoordinator.PhoneNumber = dto.PhoneNumber;
            globalCoordinator.AdditionalPhoneNumber = dto.AdditionalPhoneNumber;
            globalCoordinator.Organization = dto.Organization;

            await _dataContext.SaveChangesAsync();
            return Success();
        }

        public async Task<Result<GetGlobalCoordinatorResponseDto>> GetGlobalCoordinator(int id)
        {
            var globalCoordinator = await _dataContext.Users
                .Where(u => u.Id == id && u.Role == Role.GlobalCoordinator)
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
                _loggerAdapter.Debug($"Global coordinator with id {id} was not found");
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
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var globalCoordinator = await _dataContext.Users.FindAsync(id);

                    if (globalCoordinator == null)
                    {
                        _loggerAdapter.Debug($"Global coordinator with id {id} was not found");
                        throw new ResultException(ResultKey.User.Common.UserNotFound);
                    }

                    _dataContext.Users.Remove(globalCoordinator);
                    await _dataContext.SaveChangesAsync();

                    await _identityUserRegistrationService.DeleteIdentityUser(globalCoordinator.IdentityUserId);

                    transactionScope.Complete();
                }

                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task SendVerificationEmail(string email, string name, string securityStamp)
        {
            var baseUrl = new Uri(_config.BaseUrl);
            var verificationUrl = new Uri(baseUrl, $"verifyEmail?email={WebUtility.UrlEncode(email)}&token={WebUtility.UrlEncode(securityStamp)}").ToString();

            var (emailSubject, emailBody) = EmailTextGenerator.GenerateEmailVerificationEmail(
                role: Role.GlobalCoordinator.ToString(),
                callbackUrl: verificationUrl,
                name: name);

            await _emailPublisherService.SendEmail((email, name), emailSubject, emailBody);
        }
    }
}
