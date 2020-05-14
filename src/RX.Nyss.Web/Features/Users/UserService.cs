using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.ProjectAlertRecipients.Dto;
using RX.Nyss.Web.Features.Users.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Users
{
    public interface IUserService
    {
        Task<Result<List<GetNationalSocietyUsersResponseDto>>> List(int nationalSocietyId);
        Task<Result<NationalSocietyUsersCreateFormDataResponseDto>> GetCreateFormData(int nationalSocietyId);
        Task<Result> AddExisting(int nationalSocietyId, string userEmail);
        Task<string> GetUserApplicationLanguageCode(string userIdentityName);
        Task<Result<NationalSocietyUsersEditFormDataResponseDto>> GetEditFormData(int nationalSocietyUserId, int nationalSocietyId);
    }

    public class UserService : IUserService
    {
        private readonly INyssContext _dataContext;
        private readonly IAuthorizationService _authorizationService;

        public UserService(INyssContext dataContext, IAuthorizationService authorizationService)
        {
            _dataContext = dataContext;
            _authorizationService = authorizationService;
        }

        public async Task<Result<List<GetNationalSocietyUsersResponseDto>>> List(int nationalSocietyId)
        {
            var usersQuery = GetFilteredUsersQuery(nationalSocietyId);

            var users = await usersQuery
                .Select(uns => new GetNationalSocietyUsersResponseDto
                {
                    Id = uns.User.Id,
                    Name = uns.User.Name,
                    Email = uns.User.EmailAddress,
                    PhoneNumber = uns.User.PhoneNumber,
                    Role = uns.User.Role.ToString(),
                    Project = uns.User is SupervisorUser
                        ? ((SupervisorUser)uns.User).CurrentProject != null ? ((SupervisorUser)uns.User).CurrentProject.Name : null
                        : null,
                    OrganizationName = uns.Organization.Name,
                    OrganizationId = uns.Organization.Id,
                    IsHeadManager = uns.Organization.HeadManagerId.HasValue && uns.Organization.HeadManagerId == uns.User.Id,
                    IsPendingHeadManager = uns.Organization.PendingHeadManagerId.HasValue && uns.Organization.PendingHeadManagerId == uns.User.Id
                })
                .OrderByDescending(u => u.IsHeadManager).ThenBy(u => u.Name)
                .ToListAsync();

            return new Result<List<GetNationalSocietyUsersResponseDto>>(users, true);
        }

        public async Task<Result<NationalSocietyUsersCreateFormDataResponseDto>> GetCreateFormData(int nationalSocietyId)
        {
            var currentUser = _authorizationService.GetCurrentUser();

            var formData = await _dataContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => new NationalSocietyUsersCreateFormDataResponseDto
                {
                    Projects = _dataContext.Projects
                        .Where(p => p.NationalSociety.Id == ns.Id)
                        .Where(p => p.State == ProjectState.Open)
                        .Select(p => new ListOpenProjectsResponseDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            AlertRecipients = p.AlertNotificationRecipients.Select(anr => new ProjectAlertRecipientListResponseDto
                            {
                                Id = anr.Id,
                                Role = anr.Role,
                                Organization = anr.Organization,
                                Email = anr.Email,
                                PhoneNumber = anr.PhoneNumber
                            }).ToList()
                        }).ToList(),
                    Organizations = ns.Organizations.Select(o => new OrganizationsDto
                    {
                        Id = o.Id,
                        Name = o.Name
                    }).ToList(),
                    HasCoordinator = ns.NationalSocietyUsers.Any(u => u.User.Role == Role.Coordinator),
                    IsHeadManager = ns.DefaultOrganization.HeadManager == currentUser
                }).SingleAsync();

            return Success(formData);
        }

        public async Task<Result<NationalSocietyUsersEditFormDataResponseDto>> GetEditFormData(int nationalSocietyUserId, int nationalSocietyId)
        {
            var user = await _dataContext.Users
                .FilterAvailable()
                .Where(u => u.Id == nationalSocietyUserId && u.UserNationalSocieties.Any(uns => uns.NationalSocietyId == nationalSocietyId))
                .Select(u => new NationalSocietyUsersEditFormDataResponseDto
                {
                    Email = u.EmailAddress,
                    Role = u.Role,
                    Projects = _dataContext.Projects
                        .Where(p => p.NationalSociety.Id == nationalSocietyId)
                        .Where(p => p.State == ProjectState.Open)
                        .Select(p => new ListOpenProjectsResponseDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            AlertRecipients = u.Role == Role.Supervisor ? p.AlertNotificationRecipients.Select(anr => new ProjectAlertRecipientListResponseDto
                            {
                                Id = anr.Id,
                                Role = anr.Role,
                                Organization = anr.Organization,
                                Email = anr.Email,
                                PhoneNumber = anr.PhoneNumber
                            }).ToList() : null
                        }).ToList(),
                    Organizations = _dataContext.Organizations
                        .Where(o => o.NationalSociety.Id == nationalSocietyId)
                        .Select(o => new OrganizationsDto
                        {
                            Id = o.Id,
                            Name = o.Name
                        }).ToList()
                }).SingleOrDefaultAsync();

            return Success(user);
        }

        public async Task<Result> AddExisting(int nationalSocietyId, string userEmail)
        {
            var userData = await _dataContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == userEmail)
                .Select(u => new
                {
                    u.Id,
                    u.Role
                })
                .SingleOrDefaultAsync();

            if (userData == null)
            {
                return Error(ResultKey.User.Registration.UserNotFound);
            }

            if (userData.Role != Role.TechnicalAdvisor && userData.Role != Role.DataConsumer)
            {
                return Error(ResultKey.User.Registration.NoAssignableUserWithThisEmailFound);
            }

            var userAlreadyIsInThisNationalSociety = await _dataContext.UserNationalSocieties
                .FilterAvailableUsers()
                .AnyAsync(uns => uns.NationalSocietyId == nationalSocietyId && uns.UserId == userData.Id);

            if (userAlreadyIsInThisNationalSociety)
            {
                return Error(ResultKey.User.Registration.UserIsAlreadyInThisNationalSociety);
            }

            var nationalSocietyIsArchived = await _dataContext.NationalSocieties.AnyAsync(ns => ns.Id == nationalSocietyId && ns.IsArchived);
            if (nationalSocietyIsArchived)
            {
                return Error(ResultKey.User.Registration.CannotAddExistingUsersToArchivedNationalSociety);
            }


            var userNationalSociety = new UserNationalSociety
            {
                NationalSocietyId = nationalSocietyId,
                UserId = userData.Id
            };
            await _dataContext.UserNationalSocieties.AddAsync(userNationalSociety);
            await _dataContext.SaveChangesAsync();
            return Success();
        }

        public async Task<string> GetUserApplicationLanguageCode(string userIdentityName) =>
            await _dataContext.Users.FilterAvailable()
                .Where(u => u.EmailAddress == userIdentityName)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .SingleAsync();

        private IQueryable<UserNationalSociety> GetFilteredUsersQuery(int nationalSocietyId)
        {
            var query = _dataContext.UserNationalSocieties
                .Where(uns => uns.NationalSocietyId == nationalSocietyId);

            if (_authorizationService.IsCurrentUserInRole(Role.Administrator))
            {
                return query;
            }

            if (_authorizationService.IsCurrentUserInRole(Role.GlobalCoordinator))
            {
                return query
                    .Where(u =>
                        u.User.Role == Role.Coordinator ||
                        u.NationalSociety.DefaultOrganization.HeadManager == u.User ||
                        u.NationalSociety.DefaultOrganization.PendingHeadManager == u.User);
            }

            if (_authorizationService.IsCurrentUserInRole(Role.Coordinator))
            {
                return query
                    .Where(u =>
                        u.User.Role == Role.Coordinator ||
                        u.Organization.HeadManager == u.User ||
                        u.Organization.PendingHeadManager == u.User);
            }

            var currentUser = _authorizationService.GetCurrentUser();

            return query.Where(uns => uns.OrganizationId == query.Where(x => x.User == currentUser).Select(x => x.OrganizationId)
                .FirstOrDefault());
        }
    }
}
