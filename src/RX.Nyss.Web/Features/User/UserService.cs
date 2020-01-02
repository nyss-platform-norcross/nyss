using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.User.Dto;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.User
{
    public interface IUserService
    {
        Task<Result<List<GetNationalSocietyUsersResponseDto>>> GetUsersInNationalSociety(int nationalSocietyId, IEnumerable<string> callingUserRoles);
        Task<Result<NationalSocietyUsersBasicDataResponseDto>> GetBasicData(int nationalSocietyUserId);
        Task<List<int>> GetUserNationalSocietyIds<T>(int userId) where T : Nyss.Data.Models.User;
        Task<List<int>> GetUserNationalSocietyIds(string identityName);
        bool HasAccessToAllNationalSocieties(IEnumerable<string> roles);
        Task<bool> IsHeadManagerToNationalSociety(string identityName, int nationalSocietyId);
        Task<Result> AddExisting(int nationalSocietyId, string userEmail);
        Task<bool> HasUserAccessToNationalSociety(int nationalSocietyId, string identityName);
        Task<string> GetUserApplicationLanguageCode(string userIdentityName);
    }

    public class UserService : IUserService
    {
        private readonly INyssContext _dataContext;
        private readonly IEnumerable<string> _rolesWithAccessToAllNationalSocieties;

        public UserService(INyssContext dataContext)
        {
            _dataContext = dataContext;

            _rolesWithAccessToAllNationalSocieties = new List<Role> { Role.Administrator, Role.GlobalCoordinator }
                .Select(role => role.ToString());
        }

        public async Task<Result<List<GetNationalSocietyUsersResponseDto>>> GetUsersInNationalSociety(int nationalSocietyId, IEnumerable<string> callingUserRoles)
        {
            var usersQuery = callingUserRoles.Contains(Role.GlobalCoordinator.ToString())
                ? _dataContext.UserNationalSocieties.Where(u => u.User.Role != Role.Supervisor)
                : _dataContext.UserNationalSocieties;

            var users = await usersQuery
                .Where(uns => uns.NationalSocietyId == nationalSocietyId)
                .Select(uns => new GetNationalSocietyUsersResponseDto
                {
                    Id = uns.User.Id,
                    Name = uns.User.Name,
                    Email = uns.User.EmailAddress,
                    PhoneNumber = uns.User.PhoneNumber,
                    Role = uns.User.Role.ToString(),
                    Project = (uns.User is SupervisorUser)
                        ? ((SupervisorUser)uns.User).SupervisorUserProjects
                        .Where(sup => sup.Project.State == ProjectState.Open)
                        .Select(sup => sup.Project.Name)
                        .SingleOrDefault()
                        : null,
                    IsHeadManager = uns.NationalSociety.HeadManager != null && uns.NationalSociety.HeadManager.Id == uns.User.Id,
                    IsPendingHeadManager = uns.NationalSociety.PendingHeadManager != null && uns.NationalSociety.PendingHeadManager.Id == uns.User.Id
                })
                .ToListAsync();

            return new Result<List<GetNationalSocietyUsersResponseDto>>(users, true);
        }

        public async Task<Result<NationalSocietyUsersBasicDataResponseDto>> GetBasicData(int nationalSocietyUserId)
        {
            var user = await _dataContext.Users
                .Where(u => u.Id == nationalSocietyUserId)
                .Select(u => new NationalSocietyUsersBasicDataResponseDto { Email = u.EmailAddress, Role = u.Role })
                .SingleOrDefaultAsync();

            return Success(user);
        }

        public async Task<bool> HasUserAccessToNationalSociety(int nationalSocietyId, string identityName) =>
            await _dataContext.UserNationalSocieties
                .AnyAsync(uns => uns.NationalSocietyId == nationalSocietyId && uns.User.EmailAddress == identityName);

        public Task<List<int>> GetUserNationalSocietyIds<T>(int userId) where T : Nyss.Data.Models.User =>
            _dataContext.Users
                .OfType<T>()
                .Where(u => u.Id == userId)
                .SelectMany(u => u.UserNationalSocieties)
                .Select(uns => uns.NationalSocietyId)
                .ToListAsync();

        public async Task<List<int>> GetUserNationalSocietyIds(string identityName) =>
            await _dataContext.Users
                .Where(u => u.EmailAddress == identityName)
                .SelectMany(u => u.UserNationalSocieties)
                .Select(uns => uns.NationalSocietyId)
                .ToListAsync();

        public async Task<bool> IsHeadManagerToNationalSociety(string identityName, int nationalSocietyId) =>
            (await _dataContext.NationalSocieties.FindAsync(nationalSocietyId)).HeadManager?.EmailAddress != identityName;

        public bool HasAccessToAllNationalSocieties(IEnumerable<string> roles) =>
            roles.Any(c => _rolesWithAccessToAllNationalSocieties.Contains(c));

        public async Task<Result> AddExisting(int nationalSocietyId, string userEmail)
        {
            var userData = await _dataContext.Users
                .Where(u => u.EmailAddress == userEmail)
                .Select(u => new { u.Id, u.Role })
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
                .AnyAsync(uns => uns.NationalSocietyId == nationalSocietyId && uns.UserId == userData.Id);

            if (userAlreadyIsInThisNationalSociety)
            {
                return Error(ResultKey.User.Registration.UserIsAlreadyInThisNationalSociety);
            }

            var userNationalSociety = new UserNationalSociety { NationalSocietyId = nationalSocietyId, UserId = userData.Id };
            await _dataContext.UserNationalSocieties.AddAsync(userNationalSociety);
            await _dataContext.SaveChangesAsync();
            return Success();
        }

        public async Task<string> GetUserApplicationLanguageCode(string userIdentityName) =>
            await _dataContext.Users
                .Where(u => u.EmailAddress == userIdentityName)
                .Select(u => u.ApplicationLanguage.LanguageCode)
                .SingleAsync();
    }
}
