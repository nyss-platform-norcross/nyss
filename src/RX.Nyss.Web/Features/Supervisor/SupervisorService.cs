using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Supervisor.Dto;
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
        Task<Result> Remove(int supervisorId);
    }

    public class SupervisorService : ISupervisorService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _dataContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly INationalSocietyUserService _nationalSocietyUserService;
        private readonly IVerificationEmailService _verificationEmailService;

        public SupervisorService(IIdentityUserRegistrationService identityUserRegistrationService, INationalSocietyUserService nationalSocietyUserService, INyssContext dataContext, ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _nationalSocietyUserService = nationalSocietyUserService;
            _dataContext = dataContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;
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

        private async Task HandleSupervisorProjectReferences(SupervisorUser user, SupervisorUserProject currentProjectReference, int? selectedProjectId)
        {
            if (selectedProjectId.HasValue && selectedProjectId.Value == currentProjectReference?.ProjectId)
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

        private async Task AttachSupervisorToProject(SupervisorUser user, Project project)
        {
            var newSupervisorUserProject = CreateSupervisorUserProjectReference(project, user);
            await _dataContext.AddAsync(newSupervisorUserProject);
        }

        private async Task RemoveExistingProjectReference(SupervisorUserProject existingProjectReference)
        {
            if (existingProjectReference != null)
            {
                _dataContext.SupervisorUserProjects.Remove(existingProjectReference);
            }
        }

        private UserNationalSociety CreateUserNationalSocietyReference(Nyss.Data.Models.NationalSociety nationalSociety, Nyss.Data.Models.User user) =>
            new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };

        private SupervisorUserProject CreateSupervisorUserProjectReference(Project project, SupervisorUser supervisorUser) =>
            new SupervisorUserProject
            {
                Project = project,
                SupervisorUser = supervisorUser
            };

        public async Task<Result<GetSupervisorResponseDto>> Get(int nationalSocietyUserId)
        {
            var supervisor = await _dataContext.Users
                .OfType<SupervisorUser>()
                .Where(u => u.Id == nationalSocietyUserId)
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
                })
                .SingleOrDefaultAsync();

            if (supervisor == null)
            {
                _loggerAdapter.Debug($"Data manager with id {nationalSocietyUserId} was not found");
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

                var (supervisorUser, currentProjectReference) = (supervisorUserData.User, supervisorUserData.CurrentProjectReference);

                if (supervisorUser == null)
                {
                    _loggerAdapter.Debug($"A supervisor with id {supervisorId} was not found");
                    return Error(ResultKey.User.Common.UserNotFound);
                }

                supervisorUser.Name = editSupervisorRequestDto.Name;
                supervisorUser.Sex = editSupervisorRequestDto.Sex;
                supervisorUser.DecadeOfBirth = editSupervisorRequestDto.DecadeOfBirth;
                supervisorUser.PhoneNumber = editSupervisorRequestDto.PhoneNumber;
                supervisorUser.AdditionalPhoneNumber = editSupervisorRequestDto.AdditionalPhoneNumber;

                await HandleSupervisorProjectReferences(supervisorUser, currentProjectReference, editSupervisorRequestDto.ProjectId);

                await _dataContext.SaveChangesAsync();
                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task<Result> Remove(int supervisorId)
        {
            try
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var supervisorUser = await GetSupervisorUser(supervisorId);

                DetachSupervisorFromAllProjects(supervisorUser);
                await _nationalSocietyUserService.DeleteNationalSocietyUser(supervisorUser);
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
        }

        public async Task<SupervisorUser> GetSupervisorUser(int supervisorUserId) 
        {
            var supervisorUser = await _dataContext.Users
                .OfType<SupervisorUser>()
                .Include(u=> u.SupervisorUserProjects)
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
    }
}

