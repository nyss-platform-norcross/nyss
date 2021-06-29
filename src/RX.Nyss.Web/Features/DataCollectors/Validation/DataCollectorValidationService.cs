using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.DataCollectors.Validation
{
    public interface IDataCollectorValidationService
    {
        Task<bool> PhoneNumberExists(string phoneNumber);
        Task<bool> PhoneNumberExistsToOther(int currentDataCollectorId, string phoneNumber);
        Task<bool> IsAllowedToCreateForSupervisor(int supervisorId);
        Task<bool> LocationHasDuplicateVillageAndZone(int dataCollectorId, DataCollectorLocationRequestDto dataCollectorLocation);
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
            var currentUser = await _authorizationService.GetCurrentUser();

            return !_authorizationService.IsCurrentUserInAnyRole(Role.Supervisor, Role.HeadSupervisor)
                || (_authorizationService.IsCurrentUserInRole(Role.Supervisor) && currentUser.Id == supervisorId)
                || (_authorizationService.IsCurrentUserInRole(Role.HeadSupervisor)
                    && await _nyssContext.Users.Where(u => u.Id == supervisorId).Select(u => (SupervisorUser)u).Select(u => u.HeadSupervisor.Id).FirstOrDefaultAsync() == currentUser.Id);
        }

        public async Task<bool> LocationHasDuplicateVillageAndZone(int dataCollectorId, DataCollectorLocationRequestDto dataCollectorLocation) =>
            await _nyssContext.DataCollectorLocations
                .AnyAsync(dcl => dcl.DataCollectorId == dataCollectorId
                    && dcl.Id != dataCollectorLocation.Id
                    && dcl.Village.Id == dataCollectorLocation.VillageId
                    && (dcl.Zone == null || dcl.Zone.Id == dataCollectorLocation.ZoneId));
    }
}