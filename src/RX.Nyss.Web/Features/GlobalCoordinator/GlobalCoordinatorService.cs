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
using RX.Nyss.Web.Features.User;
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

        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IVerificationEmailService _verificationEmailService;
        private readonly IDeleteUserService _deleteUserService;
        private const string EnglishLanguageCode = "en";

        public GlobalCoordinatorService(
            IIdentityUserRegistrationService identityUserRegistrationService,
            INyssContext dataContext,
            ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService, IDeleteUserService deleteUserService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;
            _deleteUserService = deleteUserService;
        }

        public async Task<Result> RegisterGlobalCoordinator(CreateGlobalCoordinatorRequestDto dto)
        {
            try
            {
                string securityStamp;
                GlobalCoordinatorUser user;
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var identityUser = await _identityUserRegistrationService.CreateIdentityUser(dto.Email, Role.GlobalCoordinator);
                    securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                    var defaultUserApplicationLanguage = await _dataContext.ApplicationLanguages
                        .SingleOrDefaultAsync(al => al.LanguageCode == EnglishLanguageCode);

                    user = new GlobalCoordinatorUser
                    {
                        IdentityUserId = identityUser.Id,
                        EmailAddress = identityUser.Email,
                        Name = dto.Name,
                        PhoneNumber = dto.PhoneNumber,
                        AdditionalPhoneNumber = dto.AdditionalPhoneNumber,
                        Organization = dto.Organization,
                        Role = Role.GlobalCoordinator,
                        ApplicationLanguage = defaultUserApplicationLanguage,
                    };
                    await _dataContext.AddAsync(user);

                    await _dataContext.SaveChangesAsync();
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
                .OrderBy(gc => gc.Name)
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
                    _deleteUserService.EnsureCanDeleteUser(id, Role.GlobalCoordinator);


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


    }
}
