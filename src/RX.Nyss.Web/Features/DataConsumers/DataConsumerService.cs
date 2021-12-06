using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataConsumers.Dto;
using RX.Nyss.Web.Services;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.DataConsumers
{
    public interface IDataConsumerService
    {
        Task<Result> Edit(int dataConsumerId, EditDataConsumerRequestDto editDataConsumerRequestDto);
    }

    public class DataConsumerService : IDataConsumerService
    {
        private readonly INationalSocietyUserService _nationalSocietyUserService;

        private readonly ILoggerAdapter _loggerAdapter;

        private readonly INyssContext _dataContext;

        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;

        private readonly IVerificationEmailService _verificationEmailService;

        public DataConsumerService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext dataContext,
            ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;
            _nationalSocietyUserService = nationalSocietyUserService;
        }

        public async Task<Result> Edit(int dataConsumerId, EditDataConsumerRequestDto editDataConsumerRequestDto)
        {
            try
            {
                var user = await _nationalSocietyUserService.GetNationalSocietyUser<DataConsumerUser>(dataConsumerId);

                user.Name = editDataConsumerRequestDto.Name;
                user.PhoneNumber = editDataConsumerRequestDto.PhoneNumber;
                user.Organization = editDataConsumerRequestDto.Organization;
                user.AdditionalPhoneNumber = editDataConsumerRequestDto.AdditionalPhoneNumber;

                await _dataContext.SaveChangesAsync();
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
