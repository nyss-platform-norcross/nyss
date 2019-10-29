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
using RX.Nyss.Web.Features.NationalSociety.User.Dto.Create;
using RX.Nyss.Web.Features.NationalSociety.User.Dto.Edit;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;

namespace RX.Nyss.Web.Features.NationalSociety.User
{
    public interface IUserService
    {
        Task<Result> CreateDataManager(CreateNationalSocietyUserRequestDto createNationalSocietyUserRequestDto);
        Task<Result> CreateTechnicalAdvisor(CreateNationalSocietyUserRequestDto createNationalSocietyUserRequestDto);
        Task<Result> CreateDataConsumer(CreateNationalSocietyUserRequestDto createNationalSocietyUserRequestDto);
        Task<Result> UpdateNationalSocietyUser<T>(EditNationalSocietyUserRequestDto editNationalSocietyUserRequestDto) where T : Nyss.Data.Models.User;
        Task<Result> GetNationalSocietyUser(int nationalSocietyUserId);
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

        public async Task<Result> CreateDataManager(CreateNationalSocietyUserRequestDto createNationalSocietyUserRequestDto)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createNationalSocietyUserRequestDto.Email, Role.DataManager);
                await CreateNationalSocietyUser<DataManagerUser>(identityUser, createNationalSocietyUserRequestDto);

                transactionScope.Complete();
                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task<Result> CreateTechnicalAdvisor(CreateNationalSocietyUserRequestDto createNationalSocietyUserRequestDto)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createNationalSocietyUserRequestDto.Email, Role.TechnicalAdvisor);
                await CreateNationalSocietyUser<TechnicalAdvisorUser>(identityUser, createNationalSocietyUserRequestDto);

                transactionScope.Complete();
                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task<Result> CreateDataConsumer(CreateNationalSocietyUserRequestDto createNationalSocietyUserRequestDto)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createNationalSocietyUserRequestDto.Email, Role.DataConsumer);
                await CreateNationalSocietyUser<DataConsumerUser>(identityUser, createNationalSocietyUserRequestDto);

                transactionScope.Complete();
                return Result.Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task<Result> UpdateNationalSocietyUser<T>(EditNationalSocietyUserRequestDto editNationalSocietyUserRequestDto) where T: Nyss.Data.Models.User
        {
            var nationalSocietyUser = await _dataContext.Users
                .OfType<T>()
                .Where(u => u.Id == editNationalSocietyUserRequestDto.Id)
                .SingleOrDefaultAsync();

            if (nationalSocietyUser == null)
            {
                _loggerAdapter.Debug($"User with id {editNationalSocietyUserRequestDto.Id} and the role {typeof(T).ToString()} was not found");
                return Result.Error(ResultKey.User.Common.UserNotFound);
            }

            nationalSocietyUser.Name = editNationalSocietyUserRequestDto.Name;
            nationalSocietyUser.PhoneNumber = editNationalSocietyUserRequestDto.PhoneNumber;
            nationalSocietyUser.Organization = editNationalSocietyUserRequestDto.Organization;

            await _dataContext.SaveChangesAsync();
            return Result.Success();
        }

        public async Task<Result> GetNationalSocietyUser(int nationalSocietyUserId)
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

        private async Task CreateNationalSocietyUser<T>(IdentityUser identityUser, CreateNationalSocietyUserRequestDto createNationalSocietyUserRequestDto)
            where T : Nyss.Data.Models.User, new()
        {
            var nationalSociety = _dataContext.NationalSocieties.FirstOrDefault(ns => ns.Id == createNationalSocietyUserRequestDto.NationalSocietyId);
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

        private UserNationalSociety CreateUserNationalSocietyReference<T>(Nyss.Data.Models.NationalSociety nationalSociety, T user) where T : Nyss.Data.Models.User, new() =>
            new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };

        private void InitializeCommonUserProperties(Nyss.Data.Models.User nationalSocietyUser, IdentityUser identityUser, CreateNationalSocietyUserRequestDto createNationalSocietyUserRequestDto)
        {
            nationalSocietyUser.IdentityUserId = identityUser.Id;
            nationalSocietyUser.EmailAddress = identityUser.Email;
            nationalSocietyUser.Name = createNationalSocietyUserRequestDto.Name;
            nationalSocietyUser.PhoneNumber = createNationalSocietyUserRequestDto.PhoneNumber;
            nationalSocietyUser.AdditionalPhoneNumber = createNationalSocietyUserRequestDto.AdditionalPhoneNumber;
            nationalSocietyUser.Organization = createNationalSocietyUserRequestDto.Organization;
        }
    }
}

