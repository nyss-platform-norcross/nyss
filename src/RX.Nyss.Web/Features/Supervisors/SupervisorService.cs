using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Features.Organizations;
using RX.Nyss.Web.Features.Supervisors.Dto;
using RX.Nyss.Web.Features.Supervisors.Models;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Supervisors
{
    public interface ISupervisorService
    {
        Task<Result> Create(int nationalSocietyId, CreateSupervisorRequestDto createSupervisorRequestDto);
        Task<Result<GetSupervisorResponseDto>> Get(int supervisorId, int nationalSocietyId);
        Task<Result> Edit(int supervisorId, EditSupervisorRequestDto editSupervisorRequestDto);
        Task<Result> Delete(int supervisorId);
    }

    public class SupervisorService : ISupervisorService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly IVerificationEmailService _verificationEmailService;
        private readonly IDeleteUserService _deleteUserService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrganizationService _organizationService;

        public SupervisorService(IIdentityUserRegistrationService identityUserRegistrationService, INyssContext nyssContext, IOrganizationService organizationService,
            ILoggerAdapter loggerAdapter, IVerificationEmailService verificationEmailService, IDeleteUserService deleteUserService, IDateTimeProvider dateTimeProvider, IAuthorizationService authorizationService)
        {
            _identityUserRegistrationService = identityUserRegistrationService;
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
            _verificationEmailService = verificationEmailService;
            _deleteUserService = deleteUserService;
            _dateTimeProvider = dateTimeProvider;
            _authorizationService = authorizationService;
            _organizationService = organizationService;
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

        public async Task<Result<GetSupervisorResponseDto>> Get(int supervisorId, int nationalSocietyId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            var supervisor = await _nyssContext.UserNationalSocieties.FilterAvailable()
                .Where(un => un.UserId == supervisorId && un.NationalSocietyId == nationalSocietyId)
                .Select(un => new
                {
                    User = (SupervisorUser)un.User,
                    UserNationalSociety = un
                })
                .Select(u => new GetSupervisorResponseDto
                {
                    Id = u.User.Id,
                    Name = u.User.Name,
                    Role = u.User.Role,
                    Email = u.User.EmailAddress,
                    PhoneNumber = u.User.PhoneNumber,
                    AdditionalPhoneNumber = u.User.AdditionalPhoneNumber,
                    Sex = u.User.Sex,
                    DecadeOfBirth = u.User.DecadeOfBirth,
                    ProjectId = u.User.CurrentProject.Id,
                    Organization = u.User.Organization,
                    OrganizationId = u.UserNationalSociety.OrganizationId,
                    NationalSocietyId = u.UserNationalSociety.NationalSocietyId,
                    CurrentProject = new EditSupervisorFormDataDto.ListProjectsResponseDto
                    {
                        Id = u.User.CurrentProject.Id,
                        Name = u.User.CurrentProject.Name,
                        IsClosed = u.User.CurrentProject.State == ProjectState.Closed
                    },
                    HeadSupervisorId = u.User.HeadSupervisor != null ? u.User.HeadSupervisor.Id : (int?)null
                })
                .SingleOrDefaultAsync();

            if (supervisor == null)
            {
                _loggerAdapter.Debug($"Supervisor with id {supervisorId} was not found");
                return Error<GetSupervisorResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            supervisor.EditSupervisorFormData = new EditSupervisorFormDataDto
            {
                AvailableProjects = await _nyssContext.Projects
                    .Where(p => p.NationalSociety.Id == nationalSocietyId
                        && (currentUser.Role == Role.Administrator || p.ProjectOrganizations.Any(po => po.OrganizationId == supervisor.OrganizationId)))
                    .Where(p => p.State == ProjectState.Open)
                    .Select(p => new EditSupervisorFormDataDto.ListProjectsResponseDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IsClosed = p.State == ProjectState.Closed
                    })
                    .ToListAsync(),
                HeadSupervisors = await _nyssContext.UserNationalSocieties
                    .Where(uns => uns.NationalSocietyId == nationalSocietyId && uns.User.Role == Role.HeadSupervisor)
                    .Where(uns => currentUser.Role == Role.Administrator || uns.OrganizationId == supervisor.OrganizationId)
                    .Select(uns => new EditSupervisorFormDataDto.HeadSupervisorResponseDto
                    {
                        Id = uns.UserId,
                        Name = uns.User.Name
                    }).ToListAsync()
            };

            if (supervisor.CurrentProject != null && supervisor.CurrentProject.IsClosed)
            {
                supervisor.EditSupervisorFormData.AvailableProjects.Add(new EditSupervisorFormDataDto.ListProjectsResponseDto
                {
                    Id = supervisor.CurrentProject.Id,
                    Name = supervisor.CurrentProject.Name,
                    IsClosed = supervisor.CurrentProject.IsClosed
                });
            }

            return new Result<GetSupervisorResponseDto>(supervisor, true);
        }

        public async Task<Result> Edit(int supervisorId, EditSupervisorRequestDto editSupervisorRequestDto)
        {
            try
            {
                var supervisorUserData = await GetSupervisorUser(supervisorId);

                var supervisorUser = supervisorUserData.User;

                supervisorUser.Name = editSupervisorRequestDto.Name;
                supervisorUser.Sex = editSupervisorRequestDto.Sex;
                supervisorUser.DecadeOfBirth = editSupervisorRequestDto.DecadeOfBirth;
                supervisorUser.PhoneNumber = editSupervisorRequestDto.PhoneNumber;
                supervisorUser.AdditionalPhoneNumber = editSupervisorRequestDto.AdditionalPhoneNumber;
                supervisorUser.Organization = editSupervisorRequestDto.Organization;

                await UpdateSupervisorProjectReferences(supervisorUser, supervisorUserData.CurrentProjectReference, editSupervisorRequestDto.ProjectId);
                await UpdateHeadSupervisor(supervisorUser, editSupervisorRequestDto.HeadSupervisorId);

                if (editSupervisorRequestDto.OrganizationId.HasValue)
                {
                    var userLink = await _nyssContext.UserNationalSocieties
                        .Where(un => un.UserId == supervisorId && un.NationalSocietyId == editSupervisorRequestDto.NationalSocietyId)
                        .SingleOrDefaultAsync();

                    if (editSupervisorRequestDto.OrganizationId.Value != userLink.OrganizationId)
                    {
                        var validationResult = await _organizationService.CheckAccessForOrganizationEdition(userLink);

                        if (!validationResult.IsSuccess)
                        {
                            return validationResult;
                        }

                        userLink.Organization = await _nyssContext.Organizations.FindAsync(editSupervisorRequestDto.OrganizationId.Value);
                    }
                }

                await _nyssContext.SaveChangesAsync();
                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        public async Task<Result> Delete(int supervisorId)
        {
            try
            {
                await _deleteUserService.EnsureCanDeleteUser(supervisorId, Role.Supervisor);

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var supervisorUserData = await GetSupervisorUser(supervisorId);

                await EnsureSupervisorHasNoDataCollectors(supervisorUserData.User);

                RemoveExistingProjectReference(supervisorUserData.CurrentProjectReference);
                RemoveAlertRecipientsReferences(supervisorUserData.User);

                AnonymizeSupervisor(supervisorUserData.User);
                supervisorUserData.User.DeletedAt = _dateTimeProvider.UtcNow;

                await _identityUserRegistrationService.DeleteIdentityUser(supervisorUserData.User.IdentityUserId);
                supervisorUserData.User.IdentityUserId = null;

                await _nyssContext.SaveChangesAsync();
                transactionScope.Complete();

                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }

            async Task EnsureSupervisorHasNoDataCollectors(SupervisorUser supervisorUser)
            {
                var dataCollectorInfo = await _nyssContext.DataCollectors
                    .Where(dc => dc.Supervisor == supervisorUser)
                    .Select(dc => new
                    {
                        dc,
                        IsDeleted = dc.DeletedAt != null
                    })
                    .GroupBy(dc => dc.IsDeleted)
                    .Select(g => new
                    {
                        IsDeleted = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var notDeletedDataCollectorCount = dataCollectorInfo.SingleOrDefault(dc => !dc.IsDeleted)?.Count;
                if (notDeletedDataCollectorCount > 0)
                {
                    throw new ResultException(ResultKey.User.Deletion.CannotDeleteSupervisorWithDataCollectors);
                }
            }
        }

        private async Task<SupervisorUser> CreateSupervisorUser(IdentityUser identityUser, int nationalSocietyId, CreateSupervisorRequestDto createSupervisorRequestDto)
        {
            var nationalSociety = await _nyssContext.NationalSocieties.Include(ns => ns.ContentLanguage)
                .SingleOrDefaultAsync(ns => ns.Id == nationalSocietyId);

            if (nationalSociety == null)
            {
                throw new ResultException(ResultKey.User.Registration.NationalSocietyDoesNotExist);
            }

            if (nationalSociety.IsArchived)
            {
                throw new ResultException(ResultKey.User.Registration.CannotCreateUsersInArchivedNationalSociety);
            }

            var defaultUserApplicationLanguage = await _nyssContext.ApplicationLanguages
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

            await AttachSupervisorToHeadSupervisor(user, createSupervisorRequestDto.HeadSupervisorId);

            var userNationalSociety = new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };

            if (createSupervisorRequestDto.OrganizationId.HasValue)
            {
                userNationalSociety.Organization = await _nyssContext.Organizations
                    .Where(o => o.Id == createSupervisorRequestDto.OrganizationId.Value && o.NationalSocietyId == nationalSocietyId)
                    .SingleAsync();
            }
            else
            {
                var currentUser = await _authorizationService.GetCurrentUser();

                userNationalSociety.Organization = await _nyssContext.UserNationalSocieties
                    .Where(uns => uns.UserId == currentUser.Id && uns.NationalSocietyId == nationalSocietyId)
                    .Select(uns => uns.Organization)
                    .SingleAsync();
            }

            await _nyssContext.AddAsync(userNationalSociety);
            await _nyssContext.SaveChangesAsync();
            return user;
        }

        private async Task AddNewSupervisorToProject(SupervisorUser user, int? projectId, int nationalSocietyId)
        {
            if (projectId.HasValue)
            {
                var project = await _nyssContext.Projects
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

        private async Task AttachSupervisorToProject(SupervisorUser user, Project project)
        {
            var newSupervisorUserProject = new SupervisorUserProject
            {
                Project = project,
                SupervisorUser = user
            };

            user.CurrentProject = project;
            await _nyssContext.AddAsync(newSupervisorUserProject);
        }

        private async Task AttachSupervisorToHeadSupervisor(SupervisorUser user, int? headSupervisorId)
        {
            if (headSupervisorId.HasValue)
            {
                user.HeadSupervisor = (HeadSupervisorUser)await _nyssContext.Users.FirstOrDefaultAsync(u => u.Id == headSupervisorId);
            }
        }

        private async Task UpdateHeadSupervisor(SupervisorUser user, int? headSupervisorId)
        {
            if (headSupervisorId.HasValue)
            {
                if (user.HeadSupervisor == null || user.HeadSupervisor.Id != headSupervisorId)
                {
                    user.HeadSupervisor = (HeadSupervisorUser)await _nyssContext.Users.FirstOrDefaultAsync(u => u.Id == headSupervisorId);
                }
            }
        }

        private void RemoveAlertRecipientsReferences(SupervisorUser user)
        {
            var alertRecipients = user.SupervisorAlertRecipients.ToList();
            _nyssContext.SupervisorUserAlertRecipients.RemoveRange(alertRecipients);
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
                var supervisorHasNotDeletedDataCollectors = await _nyssContext.DataCollectors.Where(dc => dc.Supervisor == user)
                    .AnyAsync(dc => dc.Project == dc.Supervisor.CurrentProject && !dc.DeletedAt.HasValue);

                if (supervisorHasNotDeletedDataCollectors)
                {
                    throw new ResultException(ResultKey.User.Supervisor.CannotChangeProjectSupervisorHasDataCollectors);
                }

                var project = await _nyssContext.Projects
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
                _nyssContext.SupervisorUserProjects.Remove(existingProjectReference);
            }
        }
        private void AnonymizeSupervisor(SupervisorUser supervisorUser)
        {
            supervisorUser.Name = Anonymization.Text;
            supervisorUser.EmailAddress = Anonymization.Text;
            supervisorUser.PhoneNumber = Anonymization.Text;
            supervisorUser.AdditionalPhoneNumber = Anonymization.Text;
        }

        public async Task<SupervisorUserData> GetSupervisorUser(int supervisorUserId)
        {
            var supervisorUserData = await _nyssContext.Users.FilterAvailable()
                .OfType<SupervisorUser>()
                .Include(u => u.UserNationalSocieties)
                .ThenInclude(uns => uns.Organization)
                .Include(u => u.SupervisorAlertRecipients)
                .Include(u => u.HeadSupervisor)
                .Where(u => u.Id == supervisorUserId)
                .Select(u => new SupervisorUserData
                {
                    User = u,
                    CurrentProjectReference = u.SupervisorUserProjects
                        .SingleOrDefault(sup => sup.Project.State == ProjectState.Open)
                })
                .SingleOrDefaultAsync();

            if (supervisorUserData == null)
            {
                _loggerAdapter.Debug($"Supervisor user id {supervisorUserId} was not found");
                throw new ResultException(ResultKey.User.Common.UserNotFound);
            }

            return supervisorUserData;
        }
    }
}
