using System.Threading.Tasks;
using System.Transactions;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSociety.User.DataManager.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Features.NationalSociety.User.DataManager
{
    public interface IDataManagerService
    {
        Task<Result> CreateDataManager(int nationalSocietyId, CreateDataManagerRequestDto createDataManagerRequestDto);
        Task<Result> GetDataManager(int dataManagerId);
        Task<Result> UpdateDataManager(EditDataManagerRequestDto editDataManagerRequestDto); 
        Task<Result> DeleteDataManager(int dataManagerId);
    }

    public class DataManagerService : IDataManagerService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly ICommonUserService<DataManagerUser> _commonUserService;

        public DataManagerService(IIdentityUserRegistrationService identityUserRegistrationService, INyssContext dataContext, ILoggerAdapter loggerAdapter, ICommonUserService<DataManagerUser> commonUserService)
            
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _commonUserService = commonUserService;
        }

        public async Task<Result> CreateDataManager(int nationalSocietyId, CreateDataManagerRequestDto createDataManagerRequestDto)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createDataManagerRequestDto.Email, Role.DataManager);
                await _commonUserService.CreateNationalSocietyUser(identityUser, nationalSocietyId, createDataManagerRequestDto);

                transactionScope.Complete();
                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public Task<Result> GetDataManager(int dataManagerId) =>
            _commonUserService.GetNationalSocietyUser(dataManagerId);

        public Task<Result> UpdateDataManager(EditDataManagerRequestDto editDataManagerRequestDto) =>
            _commonUserService.UpdateNationalSocietyUser(editDataManagerRequestDto);


        public Task<Result> DeleteDataManager(int dataManagerId) =>
            _commonUserService.DeleteNationalSocietyUser(dataManagerId);
    }
}

