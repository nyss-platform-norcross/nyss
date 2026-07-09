using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.GlobalCoordinators.Dto;
using RX.Nyss.Web.Services;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.GlobalCoordinators
{
    public interface IGlobalCoordinatorService
    {
        Task<Result> Create(CreateGlobalCoordinatorRequestDto dto);
        Task<Result> Edit(EditGlobalCoordinatorRequestDto dto);
        Task<Result<GetGlobalCoordinatorResponseDto>> Get(int id);
        Task<Result<List<GetGlobalCoordinatorResponseDto>>> List();
        Task<Result> Delete(int id);
    }

    public class GlobalCoordinatorService : IGlobalCoordinatorService
    {
        private const string EnglishLanguageCode = "en";
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IVerificationEmailService _verificationEmailService;
        private readonly IDeleteUserService _deleteUserService;

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

        public async Task<Result> Create(CreateGlobalCoordinatorRequestDto dto)
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
                        ApplicationLanguage = defaultUserApplicationLanguage
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

        public async Task<Result> Edit(EditGlobalCoordinatorRequestDto dto)
        {
            var globalCoordinator = await _dataContext.Users.FilterAvailable()
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

        public async Task<Result<GetGlobalCoordinatorResponseDto>> Get(int id)
        {
            var globalCoordinator = await _dataContext.Users.FilterAvailable()
                .Where(u => u.Id == id && u.Role == Role.GlobalCoordinator)
                .Select(u => new GetGlobalCoordinatorResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    AdditionalPhoneNumber = u.AdditionalPhoneNumber ?? string.Empty,
                    Organization = u.Organization ?? string.Empty
                })
                .SingleOrDefaultAsync();

            if (globalCoordinator == null)
            {
                _loggerAdapter.Debug($"Global coordinator with id {id} was not found");
                return Error<GetGlobalCoordinatorResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            return Success(globalCoordinator);
        }

        public async Task<Result<List<GetGlobalCoordinatorResponseDto>>> List()
        {
            var globalCoordinators = await _dataContext.Users.FilterAvailable()
                .Where(u => u.Role == Role.GlobalCoordinator)
                .Select(u => new GetGlobalCoordinatorResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    AdditionalPhoneNumber = u.AdditionalPhoneNumber ?? string.Empty,
                    Organization = u.Organization ?? string.Empty
                })
                .OrderBy(gc => gc.Name)
                .AsSplitQuery()
                .ToListAsync();

            return Success(globalCoordinators);
        }

        public async Task<Result> Delete(int id)
        {
            try
            {
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var globalCoordinator = await _dataContext.Users.FilterAvailable().FirstOrDefaultAsync(u => u.Id == id);

                    if (globalCoordinator == null)
                    {
                        _loggerAdapter.Debug($"Global coordinator with id {id} was not found");
                        throw new ResultException(ResultKey.User.Common.UserNotFound);
                    }

                    await _deleteUserService.EnsureCanDeleteUser(id, Role.GlobalCoordinator);

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
