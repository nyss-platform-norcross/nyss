using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
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
            var usersQuery = GetFilteredUsersQuery();

            var users = await usersQuery
                .FilterAvailableUsers()
                .Where(uns => uns.NationalSocietyId == nationalSocietyId)
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
                    IsHeadManager = uns.NationalSociety.HeadManager != null && uns.NationalSociety.HeadManager.Id == uns.User.Id,
                    IsPendingHeadManager = uns.NationalSociety.PendingHeadManager != null && uns.NationalSociety.PendingHeadManager.Id == uns.User.Id
                })
                .OrderByDescending(u => u.IsHeadManager).ThenBy(u => u.Name)
                .ToListAsync();

            return new Result<List<GetNationalSocietyUsersResponseDto>>(users, true);
        }

        public async Task<Result<NationalSocietyUsersCreateFormDataResponseDto>> GetCreateFormData(int nationalSocietyId)
        {
            var formData = await _dataContext.NationalSocieties
                .Where(ns => ns.Id == nationalSocietyId)
                .Select(ns => new NationalSocietyUsersCreateFormDataResponseDto
                {
                    OpenProjects = _dataContext.Projects
                        .Where(p => p.NationalSociety.Id == ns.Id)
                        .Where(p => p.State == ProjectState.Open)
                        .Select(p => new ListOpenProjectsResponseDto
                        {
                            Id = p.Id,
                            Name = p.Name
                        }).ToList(),
                    Organizations = ns.Organizations.Select(o => new OrganizationsDto
                    {
                        Id = o.Id,
                        Name = o.Name
                    }).ToList()
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
                    OpenProjects = _dataContext.Projects
                        .Where(p => p.NationalSociety.Id == nationalSocietyId)
                        .Where(p => p.State == ProjectState.Open)
                        .Select(p => new ListOpenProjectsResponseDto
                        {
                            Id = p.Id,
                            Name = p.Name
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

        private IQueryable<UserNationalSociety> GetFilteredUsersQuery()
        {
            if (_authorizationService.IsCurrentUserInRole(Role.GlobalCoordinator))
            {
                return _dataContext.UserNationalSocieties.Where(u => u.User.Role != Role.Supervisor);
            }

            if (_authorizationService.IsCurrentUserInRole(Role.Coordinator))
            {
                return _dataContext.UserNationalSocieties
                    .Where(u =>
                        u.User.Role == Role.Coordinator ||
                        u.NationalSociety.HeadManager == u.User ||
                        u.NationalSociety.PendingHeadManager == u.User);
            }

            return _dataContext.UserNationalSocieties;
        }
    }
}
