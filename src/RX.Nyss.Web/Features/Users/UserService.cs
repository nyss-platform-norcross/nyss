using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.Users.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Users
{
    public interface IUserService
    {
        Task<Result<List<GetNationalSocietyUsersResponseDto>>> List(int nationalSocietyId);

        Task<Result<NationalSocietyUsersCreateFormDataResponseDto>> GetCreateFormData(int nationalSocietyId);

        Task<string> GetUserApplicationLanguageCode(string userIdentityName);

        Task<Result<NationalSocietyUsersEditFormDataResponseDto>> GetEditFormData(int nationalSocietyUserId, int nationalSocietyId);

        Task UpdateUserEmail(User user, string email);
    }

    public class UserService : IUserService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public UserService(INyssContext nyssContext, IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<Result<List<GetNationalSocietyUsersResponseDto>>> List(int nationalSocietyId)
        {
            var usersQuery = await GetFilteredUsersQuery(nationalSocietyId);

            var users = await usersQuery
                .Select(uns => new GetNationalSocietyUsersResponseDto
                {
                    Id = uns.User.Id,
                    Name = uns.User.Name,
                    Email = uns.User is DataConsumerUser && uns.User.IsFirstLogin
                        ? "***"
                        : uns.User.EmailAddress,
                    PhoneNumber = uns.User is DataConsumerUser && uns.User.IsFirstLogin
                        ? "***"
                        : uns.User.PhoneNumber,
                    Role = uns.User.Role.ToString(),
                    Project = uns.User is SupervisorUser ? ((SupervisorUser)uns.User).CurrentProject != null
                            ? ((SupervisorUser)uns.User).CurrentProject.Name
                            : null
                        : uns.User is HeadSupervisorUser ? ((HeadSupervisorUser)uns.User).CurrentProject != null
                            ? ((HeadSupervisorUser)uns.User).CurrentProject.Name
                            : null
                        : null,
                    OrganizationName = uns.Organization.Name,
                    OrganizationId = uns.Organization != null
                        ? uns.Organization.Id
                        : 0,
                    IsHeadManager = uns.Organization.HeadManagerId.HasValue && uns.Organization.HeadManagerId == uns.User.Id,
                    IsPendingHeadManager = uns.Organization.PendingHeadManagerId.HasValue && uns.Organization.PendingHeadManagerId == uns.User.Id,
                    IsVerified = !uns.User.IsFirstLogin,
                })
                .OrderByDescending(u => u.IsHeadManager).ThenBy(u => u.Name)
                .AsSplitQuery()
                .ToListAsync();

            return new Result<List<GetNationalSocietyUsersResponseDto>>(users, true);
        }

        public async Task<Result<NationalSocietyUsersCreateFormDataResponseDto>> GetCreateFormData(int nationalSocietyId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            var organizationId = await _nyssContext.UserNationalSocieties
                .Where(un => un.UserId == currentUser.Id && un.NationalSocietyId == nationalSocietyId)
                .Select(un => un.OrganizationId)
                .SingleOrDefaultAsync();

            var formData = await _nyssContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => new NationalSocietyUsersCreateFormDataResponseDto
                {
                    Projects = _nyssContext.Projects
                        .Where(p => p.NationalSociety.Id == ns.Id && (currentUser.Role == Role.Administrator || p.ProjectOrganizations.Any(po => po.OrganizationId == organizationId)))
                        .Where(p => p.State == ProjectState.Open)
                        .Select(p => new ListOpenProjectsResponseDto
                        {
                            Id = p.Id,
                            Name = p.Name
                        }).ToList(),
                    Organizations = ns.Organizations
                        .Select(o => new OrganizationsDto
                        {
                            Id = o.Id,
                            Name = o.Name,
                            IsDefaultOrganization = o == ns.DefaultOrganization,
                            HasHeadManager = o.HeadManagerId.HasValue || o.PendingHeadManagerId.HasValue
                        }).ToList(),
                    HeadSupervisors = ns.NationalSocietyUsers
                        .Where(u => u.User.Role == Role.HeadSupervisor && (currentUser.Role == Role.Administrator || u.OrganizationId == organizationId))
                        .Select(u => new HeadSupervisorResponseDto
                        {
                            Id = u.UserId,
                            Name = u.User.Name
                        }).ToList(),
                    HasCoordinator = ns.NationalSocietyUsers.Any(u => u.User.Role == Role.Coordinator),
                    IsHeadManager = ns.DefaultOrganization.HeadManager == currentUser,
                    Modems = _nyssContext.GatewayModems
                        .Where(gm => gm.GatewaySetting.NationalSocietyId == ns.Id)
                        .Select(gm => new GatewayModemResponseDto
                        {
                            Id = gm.Id,
                            Name = gm.Name
                        }).ToList(),
                    CountryCode = ns.Country.CountryCode
                }).SingleAsync();

            return Success(formData);
        }

        public async Task<Result<NationalSocietyUsersEditFormDataResponseDto>> GetEditFormData(int nationalSocietyUserId, int nationalSocietyId)
        {
            var user = await _nyssContext.Users
                .FilterAvailable()
                .Where(u => u.Id == nationalSocietyUserId && u.UserNationalSocieties.Any(uns => uns.NationalSocietyId == nationalSocietyId))
                .Select(u => new
                {
                    User = u,
                    UserNationalSociety = u.UserNationalSocieties.FirstOrDefault(uns => uns.NationalSocietyId == nationalSocietyId)
                })
                .Select(u => new NationalSocietyUsersEditFormDataResponseDto
                {
                    //Email = u.User.EmailAddress,
                    Role = u.User.Role,
                    Organizations = _nyssContext.Organizations
                        .Where(o => o.NationalSociety.Id == nationalSocietyId)
                        .Select(o => new OrganizationsDto
                        {
                            Id = o.Id,
                            Name = o.Name
                        }).ToList(),
                    Modems = _nyssContext.GatewayModems
                        .Where(gm => gm.GatewaySetting.NationalSocietyId == nationalSocietyId)
                        .Select(gm => new GatewayModemResponseDto
                        {
                            Id = gm.Id,
                            Name = gm.Name
                        }).ToList(),
                    CountryCode = u.UserNationalSociety.NationalSociety.Country.CountryCode
                }).SingleOrDefaultAsync();

            return Success(user);
        }

        public async Task<string> GetUserApplicationLanguageCode(string userIdentityName) =>
            await _nyssContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == userIdentityName)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .SingleAsync();

        private async Task<IQueryable<UserNationalSociety>> GetFilteredUsersQuery(int nationalSocietyId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();
            var query = _nyssContext.UserNationalSocieties
                .Where(uns => uns.NationalSocietyId == nationalSocietyId);

            return currentUser switch
            {
                AdministratorUser _ => query,
                GlobalCoordinatorUser _ => query.Any(uns => uns.User is CoordinatorUser)
                    ? query.Where(uns => uns.User is CoordinatorUser)
                    : query.Where(uns => uns.NationalSociety.DefaultOrganization.HeadManager == uns.User
                        || uns.NationalSociety.DefaultOrganization.PendingHeadManager == uns.User),
                CoordinatorUser _ => query.Where(u => u.User is CoordinatorUser || u.User is DataConsumerUser
                    || u.Organization.HeadManager == u.User || u.Organization.PendingHeadManager == u.User),
                ManagerUser _ => query.Where(uns =>
                    uns.User is CoordinatorUser || uns.User is DataConsumerUser
                    || uns.OrganizationId == query.Where(x => x.User == currentUser).Select(x => x.OrganizationId).FirstOrDefault()),
                TechnicalAdvisorUser _ => query.Where(uns => uns.User is DataConsumerUser
                    || uns.OrganizationId == query.Where(x => x.User == currentUser).Select(x => x.OrganizationId).FirstOrDefault()),
                _ => query.Where(uns => uns.OrganizationId == query.Where(x => x.User == currentUser).Select(x => x.OrganizationId).FirstOrDefault())
            };
        }

        public async Task UpdateUserEmail(User user, string newEmail)
        {
            var oldEmail = user.EmailAddress;

            if (newEmail != null && newEmail != oldEmail) // email is changed
            {
                if (await _nyssContext.Users.AnyAsync(usr => usr.EmailAddress == newEmail)) // new email already exists in nyssContext
                {
                    throw new ResultException(ResultKey.User.Registration.EmailIsTaken);
                }

                // Update user email address and isFirstLogin state
                user.EmailAddress = newEmail;
                user.IsFirstLogin = true;
            }

        }
    }
}
