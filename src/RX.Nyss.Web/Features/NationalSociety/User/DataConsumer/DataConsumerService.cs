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
        Task<Result> UpdateDataConsumer(EditDataConsumerRequestDto editDataConsumerRequestDto);
        Task<Result> DeleteDataConsumer(int id);
    }

    public class DataConsumerService : IDataConsumerService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly ICommonUserService<DataConsumerUser> _commonUserService;

        public DataConsumerService(IIdentityUserRegistrationService identityUserRegistrationService, INyssContext dataContext, ILoggerAdapter loggerAdapter, ICommonUserService<DataConsumerUser> commonUserService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _commonUserService = commonUserService;
        }

        public async Task<Result> CreateDataConsumer(int nationalSocietyId, CreateDataConsumerRequestDto createDataConsumerRequestDto)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createDataConsumerRequestDto.Email, Role.DataConsumer);
                await _commonUserService.CreateNationalSocietyUser(identityUser, nationalSocietyId, createDataConsumerRequestDto);

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
            _commonUserService.GetNationalSocietyUser(dataConsumerId);

        public Task<Result> UpdateDataConsumer(EditDataConsumerRequestDto editDataConsumerRequestDto) =>
            _commonUserService.UpdateNationalSocietyUser(editDataConsumerRequestDto);

        public Task<Result> DeleteDataConsumer(int id) =>
            _commonUserService.DeleteNationalSocietyUser(id);
    }
}

