using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.User.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Features.User
{
    public interface IUserService
    {
        Task<Result<List<GetNationalSocietyUsersResponseDto>>> GetUsersInNationalSociety(int nationalSocietyId);
    }

    public class UserService : IUserService
    {
        private readonly INyssContext _dataContext;

        public UserService(INyssContext dataContext)
        {
            _dataContext = dataContext;
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
    }
}

