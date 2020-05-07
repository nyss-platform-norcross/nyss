using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Organizations.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Organizations
{
    public interface IOrganizationService
    {
        Task<Result<OrganizationResponseDto>> Get(int organizationId);
        Task<Result<List<OrganizationListResponseDto>>> List(int nationalSocietyId);
        Task<Result<int>> Create(int nationalSocietyId, OrganizationRequestDto gatewaySettingRequestDto);
        Task<Result> Edit(int organizationId, OrganizationRequestDto gatewaySettingRequestDto);
        Task<Result> Delete(int organizationId);
        Task<bool> ValidateAccessForAssigningOrganization(int nationalSocietyId);
        Task<bool> ValidateAccessForChangingOrganization(int userId);
        Task<Result> CheckAccessForOrganizationEdition(UserNationalSociety userLink);
    }

    public class OrganizationService : IOrganizationService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IAuthorizationService _authorizationService;

        public OrganizationService(
            INyssContext nyssContext,
            ILoggerAdapter loggerAdapter,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _authorizationService = authorizationService;
        }

        public async Task<Result<OrganizationResponseDto>> Get(int organizationId)
        {
            var gatewaySetting = await _nyssContext.Organizations
                .Select(gs => new OrganizationResponseDto
                {
                    Id = gs.Id,
                    Name = gs.Name
                })
                .FirstOrDefaultAsync(gs => gs.Id == organizationId);

            if (gatewaySetting == null)
            {
                return Error<OrganizationResponseDto>(ResultKey.NationalSociety.Organization.SettingDoesNotExist);
            }

            var result = Success(gatewaySetting);

            return result;
        }

        public async Task<Result<List<OrganizationListResponseDto>>> List(int nationalSocietyId)
        {
            var gatewaySettings = await _nyssContext.Organizations
                .Where(gs => gs.NationalSocietyId == nationalSocietyId)
                .OrderBy(gs => gs.Id)
                .Select(gs => new OrganizationListResponseDto
                {
                    Id = gs.Id,
                    Name = gs.Name,
                    HeadManager = gs.NationalSociety.HeadManager.Name,
                    Projects = "" // TODO
                })
                .ToListAsync();

            var result = Success(gatewaySettings);

            return result;
        }

        public async Task<Result<int>> Create(int nationalSocietyId, OrganizationRequestDto gatewaySettingRequestDto)
        {
            try
            {
                var nationalSociety = await _nyssContext.NationalSocieties
                    .Include(x => x.Country)
                    .SingleOrDefaultAsync(ns => ns.Id == nationalSocietyId);

                if (nationalSociety == null)
                {
                    return Error<int>(ResultKey.NationalSociety.Organization.NationalSocietyDoesNotExist);
                }

                var gatewaySettingToAdd = new Organization
                {
                    Name = gatewaySettingRequestDto.Name,
                    NationalSocietyId = nationalSocietyId
                };

                await _nyssContext.Organizations.AddAsync(gatewaySettingToAdd);
                await _nyssContext.SaveChangesAsync();
                
                return Success(gatewaySettingToAdd.Id, ResultKey.NationalSociety.Organization.SuccessfullyAdded);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.GetResult<int>();
            }
        }

        public async Task<Result> Edit(int organizationId, OrganizationRequestDto gatewaySettingRequestDto)
        {
            try
            {
                var entity = await _nyssContext.Organizations
                    .Include(x => x.NationalSociety.Country)
                    .SingleOrDefaultAsync(x => x.Id == organizationId);

                if (entity == null)
                {
                    return Error(ResultKey.NationalSociety.Organization.SettingDoesNotExist);
                }

                entity.Name = gatewaySettingRequestDto.Name;

                await _nyssContext.SaveChangesAsync();

                return SuccessMessage(ResultKey.NationalSociety.Organization.SuccessfullyUpdated);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        public async Task<Result> Delete(int organizationId)
        {
            try
            {
                var organizationToDelete = await _nyssContext.Organizations.Where(o => o.Id == organizationId).Select(o =>
                    new
                    {
                        Organization = o,
                        AnyUsers = o.NationalSocietyUsers.Any(),
                        IsLastOrganization = o.NationalSociety.Organizations.Count == 1
                    }).SingleOrDefaultAsync();

                if (organizationToDelete == null)
                {
                    return Error(ResultKey.NationalSociety.Organization.SettingDoesNotExist);
                }

                if (organizationToDelete.AnyUsers)
                {
                    return Error(ResultKey.NationalSociety.Organization.Deletion.HasUsers);
                }

                if (organizationToDelete.IsLastOrganization)
                {
                    return Error(ResultKey.NationalSociety.Organization.Deletion.LastOrganization);
                }

                _nyssContext.Organizations.Remove(organizationToDelete.Organization);

                await _nyssContext.SaveChangesAsync();
                
                return SuccessMessage(ResultKey.NationalSociety.Organization.SuccessfullyDeleted);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        public async Task<bool> ValidateAccessForAssigningOrganization(int nationalSocietyId)
        {
            if (!_authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator, Role.Manager, Role.TechnicalAdvisor))
            {
                return false;
            }

            var nationalSociety = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => new
                {
                    HasCoordinator = ns.NationalSocietyUsers.Any(u => u.User.Role == Role.Coordinator),
                    HeadManagerId = (int?)ns.HeadManager.Id
                })
                .SingleAsync();

            if (nationalSociety.HasCoordinator)
            {
                return false;
            }

            if (!_authorizationService.IsCurrentUserInAnyRole(Role.Manager, Role.TechnicalAdvisor))
            {
                return true;
            }

            return nationalSociety.HeadManagerId == _authorizationService.GetCurrentUser().Id;
        }

        public async Task<bool> ValidateAccessForChangingOrganization(int userId)
        {
            if (!_authorizationService.IsCurrentUserInRole(Role.Coordinator))
            {
                return false;
            }

            return await _nyssContext.NationalSocieties.AnyAsync(ns => ns.HeadManager.Id == userId || ns.PendingHeadManager.Id == userId);
        }

        public async Task<Result> CheckAccessForOrganizationEdition(UserNationalSociety userLink)
        {
            if (!_authorizationService.IsCurrentUserInAnyRole(Role.Administrator, Role.Coordinator))
            {
                return Error(ResultKey.User.Common.OnlyCoordinatorCanChangeTheOrganizationOfAnotherUser);
            }

            if (_authorizationService.IsCurrentUserInRole(Role.Coordinator))
            {
                var currentUser = _authorizationService.GetCurrentUser();

                var currentUserLink = await _nyssContext.UserNationalSocieties
                    .Where(un => un.UserId == currentUser.Id)
                    .SingleOrDefaultAsync();

                if (currentUserLink.NationalSocietyId != userLink.NationalSocietyId)
                {
                    return Error(ResultKey.UnexpectedError);
                }
            }

            if (!_authorizationService.IsCurrentUserInRole(Role.Administrator) && !await _nyssContext.NationalSocieties.AnyAsync(ns => ns.HeadManager.Id == userLink.UserId || ns.PendingHeadManager.Id == userLink.UserId))
            {
                return Error<bool>(ResultKey.User.Common.CoordinatorCanChangeTheOrganizationOnlyForHeadManager);
            }

            return Success();
        }
    }
}
