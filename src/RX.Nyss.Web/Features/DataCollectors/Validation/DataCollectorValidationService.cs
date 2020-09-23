using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.DataCollectors.Validation
{
    public interface IDataCollectorValidationService
    {
        Task<bool> PhoneNumberExists(string phoneNumber);
        Task<bool> PhoneNumberExistsToOther(int currentDataCollectorId, string phoneNumber);
        Task<bool> IsAllowedToCreateForSupervisor(int supervisorId);
    }

    public class DataCollectorValidationService : IDataCollectorValidationService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public DataCollectorValidationService(INyssContext nyssContext, IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<bool> PhoneNumberExists(string phoneNumber) => 
            await _nyssContext.DataCollectors.AnyAsync(dc => dc.PhoneNumber == phoneNumber);

        public async Task<bool> PhoneNumberExistsToOther(int currentDataCollectorId, string phoneNumber) =>
            await _nyssContext.DataCollectors.AnyAsync(dc => dc.PhoneNumber == phoneNumber && dc.Id != currentDataCollectorId);

        public async Task<bool> IsAllowedToCreateForSupervisor(int supervisorId)
        {
            var currentUser = await _authorizationService.GetCurrentUserAsync();
            return currentUser.Role != Role.Supervisor || currentUser.Id == supervisorId;
        }
    }
}