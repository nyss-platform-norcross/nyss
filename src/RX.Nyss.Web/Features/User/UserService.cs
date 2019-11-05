using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.User.Dto;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.User
{
    public interface IUserService
    {
        Task<Result<List<GetNationalSocietyUsersResponseDto>>> GetUsersInNationalSociety(int nationalSocietyId);
        Task<bool> GetUserHasAccessToAnyOfResourceNationalSocieties(List<int> resourceNationalSocietyIds, string identityName, IEnumerable<string> roles);
        Task<List<int>> GetUserNationalSocietyIds<T>(int userId) where T : Nyss.Data.Models.User;
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
                .Select(nsu => nsu.User)
                .Select(u => new GetNationalSocietyUsersResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role.ToString(),
                })
                .ToListAsync();

            return new Result<List<GetNationalSocietyUsersResponseDto>>(users, true);
        }

        public async Task<bool> GetUserHasAccessToAnyOfResourceNationalSocieties(List<int> resourceNationalSocietyIds, string identityName, IEnumerable<string> roles)
        {
            var authorizedUserNationalSocietyIds = await GetUserNationalSocietyIds(identityName);
            var hasAccessToAnyOfResourceNationalSocieties = resourceNationalSocietyIds.Intersect(authorizedUserNationalSocietyIds).Any();
            return hasAccessToAnyOfResourceNationalSocieties || HasAccessToAllNationalSocieties(roles);
        }

        public Task<List<int>> GetUserNationalSocietyIds<T>(int userId) where T : Nyss.Data.Models.User =>
            _dataContext.Users
                .OfType<T>()
                .Where(u => u.Id == userId)
                .SelectMany(u => u.UserNationalSocieties)
                .Select(uns => uns.NationalSocietyId)
                .ToListAsync();

        private async Task<List<int>> GetUserNationalSocietyIds(string identityName) =>
            await _dataContext.Users
                .Where(u => u.EmailAddress == identityName)
                .SelectMany(u => u.UserNationalSocieties)
                .Select(uns => uns.NationalSocietyId)
                .ToListAsync();

        private bool HasAccessToAllNationalSocieties(IEnumerable<string> roles) =>
            roles.Any(c => _rolesWithAccessToAllNationalSocieties.Contains(c));
    }
}


