using System.Threading.Tasks;
using System.Transactions;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSociety.User.TechnicalAdvisor.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Features.NationalSociety.User.TechnicalAdvisor
{
    public interface ITechnicalAdvisorService
    {
        Task<Result> CreateTechnicalAdvisor(int nationalSocietyId, CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto);
        Task<Result> GetTechnicalAdvisor(int technicalAdvisorId);
        Task<Result> UpdateTechnicalAdvisor(EditTechnicalAdvisorRequestDto editTechnicalAdvisorRequestDto);
        Task<Result> DeleteTechnicalAdvisor(int TechnicalAdvisorId);
    }

    public class TechnicalAdvisorService : ITechnicalAdvisorService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly ICommonUserService<TechnicalAdvisorUser> _commonUserService;

        public TechnicalAdvisorService(IIdentityUserRegistrationService identityUserRegistrationService, INyssContext dataContext, ILoggerAdapter loggerAdapter, ICommonUserService<TechnicalAdvisorUser> commonUserService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _commonUserService = commonUserService;
        }

        public async Task<Result> CreateTechnicalAdvisor(int nationalSocietyId, CreateTechnicalAdvisorRequestDto createTechnicalAdvisorRequestDto)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createTechnicalAdvisorRequestDto.Email, Role.TechnicalAdvisor);
                await _commonUserService.CreateNationalSocietyUser(identityUser, nationalSocietyId, createTechnicalAdvisorRequestDto);

                transactionScope.Complete();
                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public Task<Result> GetTechnicalAdvisor(int technicalAdvisorId) =>
            _commonUserService.GetNationalSocietyUser(technicalAdvisorId);

        public Task<Result> UpdateTechnicalAdvisor(EditTechnicalAdvisorRequestDto editTechnicalAdvisorRequestDto) =>
            _commonUserService.UpdateNationalSocietyUser(editTechnicalAdvisorRequestDto);

        public Task<Result> DeleteTechnicalAdvisor(int technicalAdvisorId) =>
            _commonUserService.DeleteNationalSocietyUser(technicalAdvisorId);
    }
}

