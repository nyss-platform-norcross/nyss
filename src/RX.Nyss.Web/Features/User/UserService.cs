using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.User.Dto;
using RX.Nyss.Web.Utils.DataContract;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.User
{
    public interface IUserService
    {
        Task<Result<List<GetNationalSocietyUsersResponseDto>>> GetUsersInNationalSociety(int nationalSocietyId);
        Task<Result<NationalSocietyUsersBasicDataResponseDto>> GetBasicData(int nationalSocietyUserId);
        Task<bool> GetUserHasAccessToAnyOfProvidedNationalSocieties(List<int> providedNationalSocietyIds, string identityName, IEnumerable<string> roles);
        Task<List<int>> GetUserNationalSocietyIds<T>(int userId) where T : Nyss.Data.Models.User;
        Task<List<int>> GetUserNationalSocietyIds(string identityName);
        bool HasAccessToAllNationalSocieties(IEnumerable<string> roles);
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

        public async Task<Result<List<GetNationalSocietyUsersResponseDto>>> GetUsersInNationalSociety(int nationalSocietyId)
        {
            var users = await _dataContext.UserNationalSocieties
                .Where(uns => uns.NationalSocietyId == nationalSocietyId)
                .Select(uns => new GetNationalSocietyUsersResponseDto
                {
                    Id = uns.User.Id,
                    Name = uns.User.Name,
                    Email = uns.User.EmailAddress,
                    PhoneNumber = uns.User.PhoneNumber,
                    Role = uns.User.Role.ToString(),
                    IsHeadManager = uns.NationalSociety.HeadManager == uns.User,
                    IsPendingHeadManager = uns.NationalSociety.PendingHeadManager == uns.User
                })
                .ToListAsync();

            return new Result<List<GetNationalSocietyUsersResponseDto>>(users, true);
        }

        public async Task<Result<NationalSocietyUsersBasicDataResponseDto>> GetBasicData(int nationalSocietyUserId)
        {
            var user = await _dataContext.Users
                .Where(u => u.Id == nationalSocietyUserId)
                .Select(u => new NationalSocietyUsersBasicDataResponseDto
                {
                    Email = u.EmailAddress,
                    Role = u.Role
                })
                .SingleOrDefaultAsync();

            return Success(user);
        }


        public async Task<bool> GetUserHasAccessToAnyOfProvidedNationalSocieties(List<int> providedNationalSocietyIds, string identityName, IEnumerable<string> roles)
        {
            var authorizedUserNationalSocietyIds = await GetUserNationalSocietyIds(identityName);
            var hasAccessToAnyOfProvidedNationalSocieties = providedNationalSocietyIds.Intersect(authorizedUserNationalSocietyIds).Any();
            return hasAccessToAnyOfProvidedNationalSocieties || HasAccessToAllNationalSocieties(roles);
        }

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

        public bool HasAccessToAllNationalSocieties(IEnumerable<string> roles) =>
            roles.Any(c => _rolesWithAccessToAllNationalSocieties.Contains(c));
    }
}


