using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Supervisor.Dto;
using RX.Nyss.Web.Features.User;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Supervisor
{
    public interface ISupervisorService
    {
        Task<Result> Create(int nationalSocietyId, CreateSupervisorRequestDto createSupervisorRequestDto);
        Task<Result<GetSupervisorResponseDto>> Get(int supervisorId);
        Task<Result> Edit(int supervisorId, EditSupervisorRequestDto editSupervisorRequestDto);
        Task<Result> Remove(int supervisorId, IEnumerable<string> deletingUserRoles);
        Task<bool> GetSupervisorHasAccessToProject(string supervisorIdentityName, int projectId);
    }

    public class SupervisorService : ISupervisorService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailService;
        private IUserService _userService;

        public SupervisorService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext dataContext, ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService, IUserService userService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _nationalSocietyUserService = nationalSocietyUserService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;
            _userService = userService;
        }

        public async Task<Result> Create(int nationalSocietyId, CreateSupervisorRequestDto createSupervisorRequestDto)
        {
            try
            {
                string securityStamp;
                SupervisorUser user;
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createSupervisorRequestDto.Email, Role.Supervisor);
                    securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                    user = await CreateSupervisorUser(identityUser, nationalSocietyId, createSupervisorRequestDto);

                    transactionScope.Complete();
                }
                await _verificationEmailService.SendVerificationEmail(user, securityStamp);
                return Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task<SupervisorUser> CreateSupervisorUser(IdentityUser identityUser, int nationalSocietyId, CreateSupervisorRequestDto createSupervisorRequestDto)
        {
            var nationalSociety = await _dataContext.NationalSocieties.Include(ns => ns.ContentLanguage)
                .SingleOrDefaultAsync(ns => ns.Id == nationalSocietyId);

            if (nationalSociety == null)
            {
                throw new ResultException(ResultKey.User.Registration.NationalSocietyDoesNotExist);
            }

            var defaultUserApplicationLanguage = await _dataContext.ApplicationLanguages
                .SingleOrDefaultAsync(al => al.LanguageCode == nationalSociety.ContentLanguage.LanguageCode);

            var user = new SupervisorUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = createSupervisorRequestDto.Name,
                PhoneNumber = createSupervisorRequestDto.PhoneNumber,
                AdditionalPhoneNumber = createSupervisorRequestDto.AdditionalPhoneNumber,
                ApplicationLanguage = defaultUserApplicationLanguage,
                DecadeOfBirth = createSupervisorRequestDto.DecadeOfBirth,
                Sex = createSupervisorRequestDto.Sex,
                Organization = createSupervisorRequestDto.Organization
            };

            await AddNewSupervisorToProject(user, createSupervisorRequestDto.ProjectId, nationalSocietyId);

            var userNationalSociety = CreateUserNationalSocietyReference(nationalSociety, user);

            await _dataContext.AddAsync(userNationalSociety);
            await _dataContext.SaveChangesAsync();
            return user;
        }

        private async Task AddNewSupervisorToProject(SupervisorUser user, int? projectId, int nationalSocietyId)
        {
            if (projectId.HasValue)
            {
                var project = await _dataContext.Projects
                    .Where(p => p.State == ProjectState.Open)
                    .Where(p => p.NationalSociety.Id == nationalSocietyId)
                    .SingleOrDefaultAsync(p => p.Id == projectId.Value);

                if (project == null)
                {
                    throw new ResultException(ResultKey.User.Supervisor.ProjectDoesNotExistOrNoAccess);
                }

                await AttachSupervisorToProject(user, project);
            }
        }

        private async Task AttachSupervisorToProject(SupervisorUser user, Nyss.Data.Models.Project project)
        {
            var newSupervisorUserProject = CreateSupervisorUserProjectReference(project, user);
            await _dataContext.AddAsync(newSupervisorUserProject);
        }

        private UserNationalSociety CreateUserNationalSocietyReference(Nyss.Data.Models.NationalSociety nationalSociety, Nyss.Data.Models.User user) =>
            new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };

        private SupervisorUserProject CreateSupervisorUserProjectReference(Nyss.Data.Models.Project project, SupervisorUser supervisorUser) =>
            new SupervisorUserProject
            {
                Project = project,
                SupervisorUser = supervisorUser
            };

        public async Task<Result<GetSupervisorResponseDto>> Get(int supervisorId)
        {
            var supervisor = await _dataContext.Users
                .OfType<SupervisorUser>()
                .Where(u => u.Id == supervisorId)
                .Select(u => new GetSupervisorResponseDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Role = u.Role,
                    Email = u.EmailAddress,
                    PhoneNumber = u.PhoneNumber,
                    AdditionalPhoneNumber = u.AdditionalPhoneNumber,
                    Sex = u.Sex,
                    DecadeOfBirth = u.DecadeOfBirth,
                    ProjectId = u.SupervisorUserProjects
                        .Where(sup => sup.Project.State == ProjectState.Open)
                        .Select(sup => sup.ProjectId)
                        .SingleOrDefault(),
                    Organization = u.Organization,
                    
                })
                .SingleOrDefaultAsync();

            if (supervisor == null)
            {
                _loggerAdapter.Debug($"Data manager with id {supervisorId} was not found");
                return Error<GetSupervisorResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            return new Result<GetSupervisorResponseDto>(supervisor, true);
        }

        public async Task<Result> Edit(int supervisorId, EditSupervisorRequestDto editSupervisorRequestDto)
        {
            try
            {
                var supervisorUserData = await _dataContext.Users
                    .OfType<SupervisorUser>()
                    .Include(u => u.UserNationalSocieties)
                    .Where(u => u.Id == supervisorId)
                    .Select(u => new
                    {
                        User = u,
                        CurrentProjectReference = u.SupervisorUserProjects
                            .SingleOrDefault(sup => sup.Project.State == ProjectState.Open)
                    })
                    .SingleOrDefaultAsync();

                if (supervisorUserData == null)
                {
                    _loggerAdapter.Debug($"A supervisor with id {supervisorId} was not found");
                    return Error(ResultKey.User.Common.UserNotFound);
                }

                var supervisorUser = supervisorUserData.User;

                supervisorUser.Name = editSupervisorRequestDto.Name;
                supervisorUser.Sex = editSupervisorRequestDto.Sex;
                supervisorUser.DecadeOfBirth = editSupervisorRequestDto.DecadeOfBirth;
                supervisorUser.PhoneNumber = editSupervisorRequestDto.PhoneNumber;
                supervisorUser.AdditionalPhoneNumber = editSupervisorRequestDto.AdditionalPhoneNumber;
                supervisorUser.Organization = editSupervisorRequestDto.Organization;

                await UpdateSupervisorProjectReferences(supervisorUser, supervisorUserData.CurrentProjectReference, editSupervisorRequestDto.ProjectId);

                await _dataContext.SaveChangesAsync();
                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task UpdateSupervisorProjectReferences(SupervisorUser user, SupervisorUserProject currentProjectReference, int? selectedProjectId)
        {
            var projectHasNotChanged = selectedProjectId.HasValue && selectedProjectId.Value == currentProjectReference?.ProjectId;
            if (projectHasNotChanged)
            {
                return;
            }

            if (selectedProjectId.HasValue)
            {
                var project = await _dataContext.Projects
                    .Where(p => p.State == ProjectState.Open)
                    .Where(p => user.UserNationalSocieties.Select(uns => uns.NationalSocietyId).Contains(p.NationalSociety.Id))
                    .SingleOrDefaultAsync(p => p.Id == selectedProjectId.Value);

                if (project == null)
                {
                    throw new ResultException(ResultKey.User.Supervisor.ProjectDoesNotExistOrNoAccess);
                }

                await AttachSupervisorToProject(user, project);
            }

            RemoveExistingProjectReference(currentProjectReference);
        }

        private void RemoveExistingProjectReference(SupervisorUserProject existingProjectReference)
        {
            if (existingProjectReference != null)
            {
                _dataContext.SupervisorUserProjects.Remove(existingProjectReference);
            }
        }

        public async Task<Result> Remove(int supervisorId, IEnumerable<string> deletingUserRoles)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var supervisorUser = await GetSupervisorUser(supervisorId);
                _userService.EnsureHasPermissionsToDelteUser(supervisorUser.Role, deletingUserRoles);

                await EnsureSupervisorHasNoDataCollectors(supervisorUser);

                DetachSupervisorFromAllProjects(supervisorUser);
                _nationalSocietyUserService.DeleteNationalSocietyUser(supervisorUser);
                await _identityUserRegistrationService.DeleteIdentityUser(supervisorUser.IdentityUserId);

                await _dataContext.SaveChangesAsync();
                transactionScope.Complete();
                return Success(ResultKey.User.Registration.Success);
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }

            async Task EnsureSupervisorHasNoDataCollectors(SupervisorUser supervisorUser)
            {
                var dataCollectorInfo = await _dataContext.DataCollectors
                    .Where(dc => dc.Supervisor == supervisorUser)
                    .Select(dc => new { dc, IsDeleted = (dc.DeletedAt != null) })
                    .GroupBy(dc => dc.IsDeleted)
                    .Select(g => new { IsDeleted = g.Key, Count = g.Count() })
                    .ToListAsync();

                var notDeletedDataCollectorCount = dataCollectorInfo.SingleOrDefault(dc => !dc.IsDeleted)?.Count;
                if (notDeletedDataCollectorCount > 0)
                {
                    throw new ResultException(ResultKey.User.Deletion.CannotDeleteSupervisorWithDataCollectors);
                }

                var deletedDataCollectorCount = dataCollectorInfo.SingleOrDefault(dc => dc.IsDeleted)?.Count;
                if (deletedDataCollectorCount > 0)
                {
                    FormattableString updateDataCollectorsCommand = $"UPDATE Nyss.DataCollectors SET SupervisorId = null WHERE SupervisorId = {supervisorUser.Id}";
                    await _dataContext.Database.ExecuteSqlInterpolatedAsync(updateDataCollectorsCommand);
                }
            }
        }

        public async Task<SupervisorUser> GetSupervisorUser(int supervisorUserId)
        {
            var supervisorUser = await _dataContext.Users
                .OfType<SupervisorUser>()
                .Include(u => u.SupervisorUserProjects)
                .Include(u => u.UserNationalSocieties)
                .Where(u => u.Id == supervisorUserId)
                .SingleOrDefaultAsync();

            if (supervisorUser == null)
            {
                _loggerAdapter.Debug($"Supervisor user id {supervisorUserId} was not found");
                throw new ResultException(ResultKey.User.Common.UserNotFound);
            }

            return supervisorUser;
        }

        private void DetachSupervisorFromAllProjects(SupervisorUser deletedUser) =>
            _dataContext.SupervisorUserProjects.RemoveRange(deletedUser.SupervisorUserProjects);

        public async Task<bool> GetSupervisorHasAccessToProject(string supervisorIdentityName, int projectId) =>
            await _dataContext.SupervisorUserProjects.AnyAsync(sup => sup.SupervisorUser.EmailAddress == supervisorIdentityName && sup.ProjectId == projectId);
    }
}

