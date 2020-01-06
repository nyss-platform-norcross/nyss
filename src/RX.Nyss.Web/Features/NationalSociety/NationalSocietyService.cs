using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.NationalSociety.Dto;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSociety
{
    public interface INationalSocietyService
    {
        Task<Result<List<NationalSocietyListResponseDto>>> GetNationalSocieties(string userEmail, IEnumerable<string> userRoles);
        Task<Result<NationalSocietyResponseDto>> GetNationalSociety(int id);
        Task<Result> CreateNationalSociety(CreateNationalSocietyRequestDto nationalSociety);
        Task<Result> EditNationalSociety(int nationalSocietyId, EditNationalSocietyRequestDto nationalSociety);
        Task<Result> RemoveNationalSociety(int id);
        Task<Result> SetPendingHeadManager(int nationalSocietyId, int userId);
        Task<Result> SetAsHeadManager(string identityUserName);
        Task<Result<List<PendingHeadManagerConsentDto>>> GetPendingHeadManagerConsents(string identityUserName);
        Task<bool> HasUserAccessNationalSocieties(IEnumerable<int> providedNationalSocietyIds, string identityName, IEnumerable<string> roles);
    }

    public class NationalSocietyService : INationalSocietyService
    {
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly IUserService _userService;
        private readonly IEnumerable<string> _rolesWithAccessToAllNationalSocieties;

        public NationalSocietyService(INyssContext context, ILoggerAdapter loggerAdapter, IUserService userService)
        {
            _nyssContext = context;
            _loggerAdapter = loggerAdapter;
            _userService = userService;

            _rolesWithAccessToAllNationalSocieties = new [] { Role.Administrator, Role.GlobalCoordinator }
                .Select(role => role.ToString()).ToList();
        }

        public async Task<Result<List<NationalSocietyListResponseDto>>> GetNationalSocieties(string userEmail, IEnumerable<string> userRoles)
        {
            var nationalSocietiesQuery = _nyssContext.NationalSocieties
                .Include(x => x.HeadManager)
                .Include(x => x.PendingHeadManager)
                .Select(n => new NationalSocietyListResponseDto
                {
                    Id = n.Id,
                    ContentLanguage = n.ContentLanguage.DisplayName,
                    Name = n.Name,
                    Country = n.Country.Name,
                    StartDate = n.StartDate,
                    HeadManagerName = n.HeadManager.Name,
                    PendingHeadManagerName = n.PendingHeadManager.Name,
                    TechnicalAdvisor = string.Join(", ", n.NationalSocietyUsers.Where(u => u.User.Role == Role.TechnicalAdvisor).Select(u => u.User.Name).ToList())
                });

            if (!_userService.HasAccessToAllNationalSocieties(userRoles))
            {
                var availableNationalSocieties = await _userService.GetUserNationalSocietyIds(userEmail);
                nationalSocietiesQuery = nationalSocietiesQuery.Where(ns => availableNationalSocieties.Contains(ns.Id));
            }

            var list = await nationalSocietiesQuery
                .OrderBy(n => n.Name)
                .ToListAsync();

            return Success(list);
        }

        public async Task<Result<NationalSocietyResponseDto>> GetNationalSociety(int id)
        {
            var nationalSociety = await _nyssContext.NationalSocieties
                .Select(n => new NationalSocietyResponseDto
                {
                    Id = n.Id,
                    ContentLanguageId = n.ContentLanguage.Id,
                    ContentLanguageName = n.ContentLanguage.DisplayName,
                    Name = n.Name,
                    CountryId = n.Country.Id,
                    CountryName = n.Country.Name
                })
                .FirstOrDefaultAsync(n => n.Id == id);

            if (nationalSociety == null)
            {
                return Error<NationalSocietyResponseDto>(ResultKey.NationalSociety.NotFound);
            }

            return Success(nationalSociety);
        }

        public async Task<Result> CreateNationalSociety(CreateNationalSocietyRequestDto dto)
        {
            if (_nyssContext.NationalSocieties.Any(ns => ns.Name.ToLower() == dto.Name.ToLower()))
            {
                return Error<int>(ResultKey.NationalSociety.Creation.NameAlreadyExists);
            }

            var nationalSociety = new Nyss.Data.Models.NationalSociety
            {
                Name = dto.Name,
                ContentLanguage = await GetLanguageById(dto.ContentLanguageId),
                Country = await GetCountryById(dto.CountryId),
                IsArchived = false,
                StartDate = DateTime.UtcNow
            };

            if (nationalSociety.ContentLanguage == null)
            {
                return Error<int>(ResultKey.NationalSociety.Creation.LanguageNotFound);
            }

            if (nationalSociety.Country == null)
            {
                return Error<int>(ResultKey.NationalSociety.Creation.CountryNotFound);
            }

            await _nyssContext.AddAsync(nationalSociety);
            await _nyssContext.SaveChangesAsync();
            _loggerAdapter.Info($"A national society {nationalSociety} was created");
            return Success(nationalSociety.Id);
        }

        public async Task<Result> EditNationalSociety(int nationalSocietyId, EditNationalSocietyRequestDto dto)
        {
            if (_nyssContext.NationalSocieties.Any(ns => ns.Id != nationalSocietyId && ns.Name.ToLower() == dto.Name.ToLower()))
            {
                return Error<int>(ResultKey.NationalSociety.Creation.NameAlreadyExists);
            }

            var nationalSociety = await _nyssContext.NationalSocieties.FindAsync(nationalSocietyId);

            nationalSociety.Name = dto.Name;
            nationalSociety.ContentLanguage = await GetLanguageById(dto.ContentLanguageId);
            nationalSociety.Country = await GetCountryById(dto.CountryId);

            await _nyssContext.SaveChangesAsync();

            return SuccessMessage(ResultKey.NationalSociety.Edit.Success);
        }

        public async Task<Result> RemoveNationalSociety(int id)
        {
            var nationalSociety = await _nyssContext.NationalSocieties.FindAsync(id);
            _nyssContext.NationalSocieties.Remove(nationalSociety);
            await _nyssContext.SaveChangesAsync();
            return SuccessMessage(ResultKey.NationalSociety.Remove.Success);
        }

        public async Task<Result> SetPendingHeadManager(int nationalSocietyId, int userId)
        {
            var ns = await _nyssContext.NationalSocieties.FindAsync(nationalSocietyId);
            var user = await _nyssContext.Users.Include(u => u.UserNationalSocieties).FirstOrDefaultAsync(u => u.Id == userId);
            
            if (ns.NationalSocietyUsers.Count == 0 || ns.NationalSocietyUsers.All(x => x.UserId != userId))
            {
                return Error(ResultKey.NationalSociety.SetHead.NotAMemberOfSociety);
            }

            if (!(user is ManagerUser || user is TechnicalAdvisorUser))
            {
                return Error(ResultKey.NationalSociety.SetHead.NotApplicableUserRole);
            }

            ns.PendingHeadManager = user;
            await _nyssContext.SaveChangesAsync();

            return Success();
        }

        public async Task<Result> SetAsHeadManager(string identityUserName)
        {
            var user = await _nyssContext.Users
                .SingleOrDefaultAsync(u => u.EmailAddress == identityUserName);

            if (user == null)
            {
                return Error(ResultKey.User.Common.UserNotFound);
            }

            if (!(user is ManagerUser || user is TechnicalAdvisorUser))
            {
                return Error(ResultKey.NationalSociety.SetHead.NotApplicableUserRole);
            }

            var pendingSocieties = _nyssContext.NationalSocieties.Where(x => x.PendingHeadManager.Id == user.Id);
            var utcNow = DateTime.UtcNow;

            // Set until date for the previous consent
            await _nyssContext.HeadManagerConsents
                .Where(x => pendingSocieties.Select(y => y.Id).Contains(x.NationalSocietyId) && x.ConsentedUntil == null)
                .ForEachAsync(x => x.ConsentedUntil = utcNow);

            foreach (var nationalSociety in pendingSocieties)
            {
                nationalSociety.PendingHeadManager = null;
                nationalSociety.HeadManager = user;

                await _nyssContext.HeadManagerConsents.AddAsync(new HeadManagerConsent
                {
                    ConsentedFrom = utcNow,
                    NationalSocietyId = nationalSociety.Id,
                    UserEmailAddress = user.EmailAddress,
                    UserPhoneNumber = user.PhoneNumber
                });
            }

            await _nyssContext.SaveChangesAsync();

            return Success();
        }

        public async Task<Result<List<PendingHeadManagerConsentDto>>> GetPendingHeadManagerConsents(string identityUserName)
        {
            var userEntity = await _nyssContext.Users
                .Include(x => x.ApplicationLanguage)
                .SingleOrDefaultAsync(u => u.EmailAddress == identityUserName);

            if (userEntity == null)
            {
                return Error<List<PendingHeadManagerConsentDto>>(ResultKey.User.Common.UserNotFound);
            }

            var pendingSocieties = await _nyssContext.NationalSocieties
                .Where(ns => ns.PendingHeadManager.IdentityUserId == userEntity.IdentityUserId)
                .Select(ns => new PendingHeadManagerConsentDto
                {
                    NationalSocietyName = ns.Name,
                    NationalSocietyCountryName = ns.Country.Name,
                    NationalSocietyId = ns.Id
                })
                .ToListAsync();

            return Success(pendingSocieties);
        }

        public async Task<ContentLanguage> GetLanguageById(int id) =>
            await _nyssContext.ContentLanguages.FindAsync(id);

        public async Task<Country> GetCountryById(int id) =>
            await _nyssContext.Countries.FindAsync(id);

        public async Task<bool> HasUserAccessNationalSocieties(IEnumerable<int> providedNationalSocietyIds, string identityName, IEnumerable<string> roles)
        {
            if (HasAccessToAllNationalSocieties(roles))
            {
                return true;
            }

            var userNationalSocieties = await GetUserNationalSocietyIds(identityName);
            return providedNationalSocietyIds.Intersect(userNationalSocieties).Any();
        }

        public async Task<List<int>> GetUserNationalSocietyIds(string identityName) =>
            await _nyssContext.Users
                .Where(u => u.EmailAddress == identityName)
                .SelectMany(u => u.UserNationalSocieties)
                .Select(uns => uns.NationalSocietyId)
                .ToListAsync();

        public bool HasAccessToAllNationalSocieties(IEnumerable<string> roles) =>
            roles.Any(c => _rolesWithAccessToAllNationalSocieties.Contains(c));
    }
}
