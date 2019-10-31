using System.Threading.Tasks;
using System.Transactions;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSociety.User.DataConsumer.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Features.NationalSociety.User.DataConsumer
{
    public interface IDataConsumerService
    {
        Task<Result> CreateDataConsumer(int nationalSocietyId, CreateDataConsumerRequestDto createDataConsumerRequestDto);
        Task<Result> GetDataConsumer(int dataConsumerId);
        Task<Result> UpdateDataConsumer(int dataConsumerId, EditDataConsumerRequestDto editDataConsumerRequestDto);
        Task<Result> DeleteDataConsumer(int dataConsumerId);
    }

    public class DataConsumerService : BaseUserService<DataConsumerUser>, IDataConsumerService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;

        public DataConsumerService(IIdentityUserRegistrationService identityUserRegistrationService, INyssContext dataContext, ILoggerAdapter loggerAdapter) 
            : base(dataContext, loggerAdapter)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<Result> CreateDataConsumer(int nationalSocietyId, CreateDataConsumerRequestDto createDataConsumerRequestDto)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createDataConsumerRequestDto.Email, Role.DataConsumer);
                await CreateNationalSocietyUser(identityUser, nationalSocietyId, createDataConsumerRequestDto);

                transactionScope.Complete();
                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }
        public Task<Result> GetDataConsumer(int dataConsumerId) =>
            GetNationalSocietyUser(dataConsumerId);

        public Task<Result> UpdateDataConsumer(int dataConsumerId, EditDataConsumerRequestDto editDataConsumerRequestDto) =>
            UpdateNationalSocietyUser(dataConsumerId, editDataConsumerRequestDto);

        public async Task<Result> DeleteDataConsumer(int dataConsumerId)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var deletedDataConsumer = await DeleteNationalSocietyUser(dataConsumerId);
                await _identityUserRegistrationService.DeleteIdentityUser(deletedDataConsumer.IdentityUserId);

                transactionScope.Complete();
                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }
    }
}

