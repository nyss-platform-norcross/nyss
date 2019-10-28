using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Administration.GlobalCoordinator.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Features.Administration.GlobalCoordinator
{
    public interface IGlobalCoordinatorService
    {
        Task<(Result, string emailVerificationCode)> RegisterGlobalCoordinator(RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto);
    }

    public class GlobalCoordinatorService: IGlobalCoordinatorService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;

        public GlobalCoordinatorService(IIdentityUserRegistrationService identityUserRegistrationService, INyssContext dataContext, ILoggerAdapter loggerAdapter)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<(Result, string emailVerificationCode)> RegisterGlobalCoordinator(RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var identityUser = await _identityUserRegistrationService.CreateIdentityUser(registerGlobalCoordinatorRequestDto.Email, Role.GlobalCoordinator);
                await CreateGlobalCoordinator(identityUser, registerGlobalCoordinatorRequestDto);
                var emailVerificationCode = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                transactionScope.Complete();

                return (Result.Success(ResultKey.User.Registration.Success), emailVerificationCode);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return (e.Result, null);
            }
        }

        private async Task CreateGlobalCoordinator(IdentityUser identityUser, RegisterGlobalCoordinatorRequestDto registerGlobalCoordinatorRequestDto)
        {
            var globalCoordinator = new GlobalCoordinatorUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = registerGlobalCoordinatorRequestDto.Name,
                PhoneNumber = registerGlobalCoordinatorRequestDto.PhoneNumber,
                Role = Role.GlobalCoordinator
            };

            await _dataContext.AddAsync(globalCoordinator);
            await _dataContext.SaveChangesAsync();
        }
    }
}
