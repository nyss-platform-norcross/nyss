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
using RX.Nyss.Web.Features.Administration.GlobalCoordinator.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Features.Administration.GlobalCoordinator
{
    public interface IGlobalCoordinatorService
    {
        Task<Result> RegisterGlobalCoordinator(RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto);
        Task<Result> UpdateGlobalCoordinator(EditGlobalCoordinatorRequestDto editGlobalCoordinatorRequestDto);
        Task<Result> GetGlobalCoordinator(int globalCoordinatorId);
        Task<Result<List<GetGlobalCoordinatorsResponseDto>>> GetGlobalCoordinators();
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

        public async Task<Result> RegisterGlobalCoordinator(RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto)
        {
            try
            {
                var identityUser = await CreateUser(registerGlobalCoordinatorRequestDto);
                var securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                var baseUrl = new Uri(_config.BaseUrl);
                var verificationUrl = new Uri(baseUrl, $"verifyEmail?email={WebUtility.UrlEncode(registerGlobalCoordinatorRequestDto.Email)}&token={WebUtility.UrlEncode(securityStamp)}").ToString();

                var (emailSubject, emailBody) = EmailTextGenerator.GenerateEmailVerificationEmail(
                    Role.GlobalCoordinator.ToString(),
                    verificationUrl,
                    registerGlobalCoordinatorRequestDto.Name);

                await _emailPublisherService.SendEmail((registerGlobalCoordinatorRequestDto.Email, registerGlobalCoordinatorRequestDto.Name), emailSubject, emailBody);

                return Result.Success(ResultKey.User.Registration.Success);
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
                .Where(u => u.Role == Role.GlobalCoordinator)
                .Where(u => u.Id == editGlobalCoordinatorRequestDto.Id)
                .SingleOrDefaultAsync();
            
            if (globalCoordinator == null)
            {
                _loggerAdapter.Debug($"Global coordinator with id {editGlobalCoordinatorRequestDto.Id} was not found");
                return Result.Error(ResultKey.User.Common.UserNotFound);
            }

            globalCoordinator.Name = editGlobalCoordinatorRequestDto.Name;
            globalCoordinator.PhoneNumber = editGlobalCoordinatorRequestDto.PhoneNumber;
            globalCoordinator.Organization = editGlobalCoordinatorRequestDto.Organization;

            await _dataContext.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> GetGlobalCoordinator(int globalCoordinatorId)
        {
            var globalCoordinator = await _dataContext.Users
                .Where(u => u.Role == Role.GlobalCoordinator)
                .Where(u => u.Id == globalCoordinatorId)
                .Select(u => new GetGlobalCoordinatorsResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    EmailAddress = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    Organization = u.Organization
                })
                .SingleOrDefaultAsync();

            if (globalCoordinator == null)
            {
                _loggerAdapter.Debug($"Global coordinator with id {globalCoordinatorId} was not found");
                return Result.Error(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetGlobalCoordinatorsResponseDto>(globalCoordinator, true);
        }

        public async Task<Result<List<GetGlobalCoordinatorsResponseDto>>> GetGlobalCoordinators()
        {
            var globalCoordinators = await _dataContext.Users
                .Where(u => u.Role == Role.GlobalCoordinator)
                .Select(u => new GetGlobalCoordinatorsResponseDto
                {
                    Id =  u.Id,
                    Name = u.Name,
                    EmailAddress = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    Organization = u.Organization
                })
                .ToListAsync();

            return new Result<List<GetGlobalCoordinatorsResponseDto>>(globalCoordinators, true);
        }

        private async Task CreateGlobalCoordinator(IdentityUser identityUser, RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto)
        {
            var globalCoordinator = new GlobalCoordinatorUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = registerGlobalCoordinatorRequestDto.Name,
                PhoneNumber = registerGlobalCoordinatorRequestDto.PhoneNumber,
                Organization =  registerGlobalCoordinatorRequestDto.Organization,
                Role = Role.GlobalCoordinator
            };

            await _dataContext.AddAsync(globalCoordinator);
            await _dataContext.SaveChangesAsync();
        }
    }
}
