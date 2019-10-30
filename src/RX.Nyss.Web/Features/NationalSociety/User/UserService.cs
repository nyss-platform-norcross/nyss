using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSociety.User.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Features.NationalSociety.User
{
    public interface IUserService
    {
        Task<Result> GetUsersInNationalSociety(int nationalSocietyId);
    }

    public class UserService : IUserService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;

        public UserService(IIdentityUserRegistrationService identityUserRegistrationService, INyssContext dataContext, ILoggerAdapter loggerAdapter)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
        }

        
        public async Task<Result> GetUsersInNationalSociety(int nationalSocietyId)
        {
            var users = await _dataContext.UserNationalSocieties
                .Where(uns => uns.NationalSocietyId == nationalSocietyId)
                .Select(nsu => nsu.User)
                .Select(u => new GetNationalSocietyUsersResponseRowDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role.ToString(),
                })
                .ToListAsync();

            return new Result<List<GetNationalSocietyUsersResponseRowDto>>(users, true);
        }
    }
}

