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
        Task<bool> ValidateAccessForAssigningOrganizationToUser(int nationalSocietyId);
        Task<bool> ValidateAccessForChangingOrganization(int userId);
        Task<Result> CheckAccessForOrganizationEdition(UserNationalSociety userLink);
        Task<Result> SetPendingHeadManager(int organizationId, int userId);
        IQueryable<NationalSociety> GetNationalSocietiesWithPendingAgreementsForUserQuery(User userEntity);
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
            var organization = await _nyssContext.Organizations
                .Select(gs => new OrganizationResponseDto
                {
                    Id = gs.Id,
                    Name = gs.Name
                })
                .FirstOrDefaultAsync(gs => gs.Id == organizationId);

            if (organization == null)
            {
                return Error<OrganizationResponseDto>(ResultKey.NationalSociety.Organization.SettingDoesNotExist);
            }

            var result = Success(organization);

            return result;
        }

        public async Task<Result<List<OrganizationListResponseDto>>> List(int nationalSocietyId)
        {
            var organizations = await _nyssContext.Organizations
                .Where(o => o.NationalSocietyId == nationalSocietyId)
                .OrderByDescending(o => o.NationalSociety.DefaultOrganization == o)
                .ThenBy(o => o.Id)
                .Select(o => new OrganizationListResponseDto
                {
                    Id = o.Id,
                    Name = o.Name,
                    HeadManager = o.HeadManager.Name,
                    Projects = string.Join(", ", o.OrganizationProjects.Select(op => op.Project.Name)),
                    IsDefaultOrganization = o.NationalSociety.DefaultOrganization == o
                })
                .ToListAsync();

            var result = Success(organizations);

            return result;
        }

        public async Task<Result<int>> Create(int nationalSocietyId, OrganizationRequestDto organizationRequestDto)
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

                var organizationToAdd = new Organization
                {
                    Name = organizationRequestDto.Name,
                    NationalSocietyId = nationalSocietyId
                };

                await _nyssContext.Organizations.AddAsync(organizationToAdd);
                await _nyssContext.SaveChangesAsync();
                
                return Success(organizationToAdd.Id, ResultKey.NationalSociety.Organization.SuccessfullyAdded);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.GetResult<int>();
            }
        }

        public async Task<Result> Edit(int organizationId, OrganizationRequestDto organizationRequestDto)
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

                entity.Name = organizationRequestDto.Name;

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

        public async Task<bool> ValidateAccessForAssigningOrganizationToUser(int nationalSocietyId)
        {
            if (_authorizationService.IsCurrentUserInRole(Role.Administrator))
            {
                return true;
            }

            var currentUserName = _authorizationService.GetCurrentUserName();

            var nationalSociety = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => new
                {
                    HasCoordinator = ns.NationalSocietyUsers.Any(u => u.User.Role == Role.Coordinator),
                    IsCurrentUserHeadManagerOfDefaultOrganization = ns.DefaultOrganization.HeadManager.EmailAddress == currentUserName
                })
                .SingleAsync();

            if (nationalSociety.IsCurrentUserHeadManagerOfDefaultOrganization && !nationalSociety.HasCoordinator)
            {
                return true;
            }

            if (_authorizationService.IsCurrentUserInRole(Role.Coordinator))
            {
                return true;
            }

            return false;
        }

        public async Task<bool> ValidateAccessForChangingOrganization(int userId)
        {
            if (!_authorizationService.IsCurrentUserInRole(Role.Coordinator))
            {
                return false;
            }

            return await _nyssContext.NationalSocieties.AnyAsync(ns => ns.DefaultOrganization.HeadManager.Id == userId || ns.DefaultOrganization.PendingHeadManager.Id == userId);
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

            if (!_authorizationService.IsCurrentUserInRole(Role.Administrator) && !await _nyssContext.NationalSocieties.AnyAsync(ns => ns.DefaultOrganization.HeadManager.Id == userLink.UserId || ns.DefaultOrganization.PendingHeadManager.Id == userLink.UserId))
            {
                return Error<bool>(ResultKey.User.Common.CoordinatorCanChangeTheOrganizationOnlyForHeadManager);
            }

            return Success();
        }

        public async Task<Result> SetPendingHeadManager(int organizationId, int userId)
        {
            var userNationalSocieties = await _nyssContext.UserNationalSocieties
                .Where(uns => uns.OrganizationId == organizationId && uns.UserId == userId)
                .Select(x => new { x.Organization, x.User, IsDefaultOrganization = x.NationalSociety.DefaultOrganization == x.Organization })
                .SingleOrDefaultAsync();

            if (userNationalSocieties == null)
            {
                return Error(ResultKey.NationalSociety.SetHead.NotAMemberOfSociety);
            }

            if (!(userNationalSocieties.User is ManagerUser || userNationalSocieties.User is TechnicalAdvisorUser))
            {
                return Error(ResultKey.NationalSociety.SetHead.NotApplicableUserRole);
            }

            if (userNationalSocieties.IsDefaultOrganization)
            {
                userNationalSocieties.Organization.PendingHeadManager = userNationalSocieties.User;
            }
            else
            {
                userNationalSocieties.Organization.HeadManager = userNationalSocieties.User;
            }
            
            await _nyssContext.SaveChangesAsync();

            return Success();
        }

        public IQueryable<NationalSociety> GetNationalSocietiesWithPendingAgreementsForUserQuery(User userEntity)
        {
            var notConsentedNationalSocieties = _nyssContext.NationalSocieties
                .Where(ns => !_nyssContext.NationalSocietyConsents
                    .Where(nsc => !nsc.ConsentedUntil.HasValue && nsc.UserEmailAddress == userEntity.EmailAddress)
                    .Select(x => x.NationalSocietyId).Distinct()
                    .Contains(ns.Id));

            return notConsentedNationalSocieties.Where(x =>
                userEntity.Role == Role.Coordinator && x.NationalSocietyUsers.Any(y => y.UserId == userEntity.Id) ||
                (userEntity.Role == Role.Manager || userEntity.Role == Role.TechnicalAdvisor) && x.DefaultOrganization.HeadManager == userEntity || x.DefaultOrganization.PendingHeadManager == userEntity);
        }
    }
}
