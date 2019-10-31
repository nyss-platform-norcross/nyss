using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSociety.User.Dto;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;


namespace RX.Nyss.Web.Features.NationalSociety.User
{
    public abstract class BaseUserService<T> where T : Nyss.Data.Models.User, new()
    {
        private readonly INyssContext _dataContext;
        private readonly ILoggerAdapter _loggerAdapter;

        protected BaseUserService(INyssContext dataContext, ILoggerAdapter loggerAdapter)
        {
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
        }

        protected async Task CreateNationalSocietyUser(IdentityUser identityUser, int nationalSocietyId, ICreateNationalSocietyUserRequestDto createNationalSocietyUserRequestDto)
        {
            var nationalSociety = _dataContext.NationalSocieties.FirstOrDefault(ns => ns.Id == nationalSocietyId);
            if (nationalSociety == null)
            {
                throw new ResultException(ResultKey.User.Registration.NationalSocietyDoesNotExist);
            }

            var user = new T();
            InitializeCommonUserProperties(user, identityUser, createNationalSocietyUserRequestDto);
            var userNationalSociety = CreateUserNationalSocietyReference(nationalSociety, user);

            await _dataContext.AddAsync(userNationalSociety);
            await _dataContext.SaveChangesAsync();
        }

        private UserNationalSociety CreateUserNationalSocietyReference(Nyss.Data.Models.NationalSociety nationalSociety, T user)  =>
            new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };

        private void InitializeCommonUserProperties(Nyss.Data.Models.User nationalSocietyUser, IdentityUser identityUser, ICreateNationalSocietyUserRequestDto createNationalSocietyUserRequestDto)
        {
            nationalSocietyUser.IdentityUserId = identityUser.Id;
            nationalSocietyUser.EmailAddress = identityUser.Email;
            nationalSocietyUser.Name = createNationalSocietyUserRequestDto.Name;
            nationalSocietyUser.PhoneNumber = createNationalSocietyUserRequestDto.PhoneNumber;
            nationalSocietyUser.AdditionalPhoneNumber = createNationalSocietyUserRequestDto.AdditionalPhoneNumber;
            nationalSocietyUser.Organization = createNationalSocietyUserRequestDto.Organization;
        }

        protected async Task<Result> GetNationalSocietyUser(int nationalSocietyUserId)
        {
            var nationalSocietyLevelUser = await _dataContext.Users
                .Where(u => u.Id == nationalSocietyUserId)
                .Select(u => new GetNationalSocietyUserResponseDto()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    AdditionalPhoneNumber = u.AdditionalPhoneNumber,
                    Organization = u.Organization,
                })
                .SingleOrDefaultAsync();

            if (nationalSocietyLevelUser == null)
            {
                _loggerAdapter.Debug($"Global coordinator with id {nationalSocietyUserId} was not found");
                return Result.Error(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetNationalSocietyUserResponseDto>(nationalSocietyLevelUser, true);
        }

        protected async Task<Result> UpdateNationalSocietyUser(int nationalSocietyUserId, IEditNationalSocietyUserRequestDto editNationalSocietyUserRequestDto) 
        {
            var nationalSocietyUser = await _dataContext.Users
                .OfType<T>()
                .Where(u => u.Id == nationalSocietyUserId)
                .SingleOrDefaultAsync();

            if (nationalSocietyUser == null)
            {
                _loggerAdapter.Debug($"User with id {nationalSocietyUserId} and the role {typeof(T).ToString()} was not found");
                return Result.Error(ResultKey.User.Common.UserNotFound);
            }

            nationalSocietyUser.Name = editNationalSocietyUserRequestDto.Name;
            nationalSocietyUser.PhoneNumber = editNationalSocietyUserRequestDto.PhoneNumber;
            nationalSocietyUser.Organization = editNationalSocietyUserRequestDto.Organization;

            await _dataContext.SaveChangesAsync();
            return Result.Success();
        }

        protected async Task<Nyss.Data.Models.User> DeleteNationalSocietyUser(int nationalSocietyUserId) 
        {
            var nationalSocietyUser = await _dataContext.Users
                .OfType<T>()
                .Where(u => u.Id == nationalSocietyUserId)
                .SingleOrDefaultAsync();

            if (nationalSocietyUser == null)
            {
                _loggerAdapter.Debug($"User with id {nationalSocietyUserId} and the role {typeof(T).ToString()} was not found");
                throw new ResultException(ResultKey.User.Common.UserNotFound);
            }

            var userNationalSocieties = await _dataContext.UserNationalSocieties
                .Where(uns => uns.UserId == nationalSocietyUserId)
                .ToListAsync();

            _dataContext.UserNationalSocieties.RemoveRange(userNationalSocieties);
            _dataContext.Users.Remove(nationalSocietyUser);
            await _dataContext.SaveChangesAsync();

            return nationalSocietyUser;
        }
    }
}
