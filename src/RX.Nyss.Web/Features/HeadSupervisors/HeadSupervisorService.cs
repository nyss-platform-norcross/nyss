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
using RX.Nyss.Web.Features.HeadSupervisors.Dto;
using RX.Nyss.Web.Features.HeadSupervisors.Models;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.HeadSupervisors
{
    public interface IHeadSupervisorService
    {
        Task<Result> Create(int nationalSocietyId, CreateHeadSupervisorRequestDto createSupervisorRequestDto);
        Task<Result<GetHeadSupervisorResponseDto>> Get(int headSupervisorId, int nationalSocietyId);
        Task<Result> Edit(int headSupervisorId, EditHeadSupervisorRequestDto editHeadSupervisorRequestDto);
        Task<Result> Delete(int headSupervisorId);
    }

    public class HeadSupervisorService : IHeadSupervisorService
    {
        private readonly ILoggerAdapter _loggerAdapter;
        private readonly INyssContext _nyssContext;
        private readonly IIdentityUserRegistrationService _identityUserRegistrationService;
        private readonly IVerificationEmailService _verificationEmailService;
        private readonly IDeleteUserService _deleteUserService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAuthorizationService _authorizationService;
        private readonly IOrganizationService _organizationService;

        public HeadSupervisorService(IIdentityUserRegistrationService identityUserRegistrationService, INyssContext nyssContext, IOrganizationService organizationService,
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

        public async Task<Result> Create(int nationalSocietyId, CreateHeadSupervisorRequestDto createHeadSupervisorRequestDto)
        {
            try
            {
                string securityStamp;
                HeadSupervisorUser user;
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var identityUser = await _identityUserRegistrationService.CreateIdentityUser(createHeadSupervisorRequestDto.Email, Role.HeadSupervisor);
                    securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                    user = await CreateHeadSupervisorUser(identityUser, nationalSocietyId, createHeadSupervisorRequestDto);

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

        public async Task<Result<GetHeadSupervisorResponseDto>> Get(int headSupervisorId, int nationalSocietyId)
        {
            var currentUser = await _authorizationService.GetCurrentUser();

            var headSupervisor = await _nyssContext.UserNationalSocieties.FilterAvailable()
                .Where(un => un.UserId == headSupervisorId && un.NationalSocietyId == nationalSocietyId)
                .Select(un => new
                {
                    User = (HeadSupervisorUser)un.User,
                    UserNationalSociety = un
                })
                .Select(u => new GetHeadSupervisorResponseDto
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
                    CurrentProject = new EditHeadSupervisorFormDataDto.ListProjectsResponseDto
                    {
                        Id = u.User.CurrentProject.Id,
                        Name = u.User.CurrentProject.Name,
                        IsClosed = u.User.CurrentProject.State == ProjectState.Closed
                    },
                    ModemId = u.User.ModemId
                })
                .SingleOrDefaultAsync();

            if (headSupervisor == null)
            {
                _loggerAdapter.Debug($"Supervisor with id {headSupervisorId} was not found");
                return Error<GetHeadSupervisorResponseDto>(ResultKey.User.Common.UserNotFound);
            }

            headSupervisor.EditSupervisorFormData = new EditHeadSupervisorFormDataDto
            {
                AvailableProjects = await _nyssContext.Projects
                    .Where(p => p.NationalSociety.Id == nationalSocietyId
                        && (currentUser.Role == Role.Administrator || p.ProjectOrganizations.Any(po => po.OrganizationId == headSupervisor.OrganizationId)))
                    .Where(p => p.State == ProjectState.Open)
                    .Select(p => new EditHeadSupervisorFormDataDto.ListProjectsResponseDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        IsClosed = p.State == ProjectState.Closed
                    })
                    .ToListAsync()
            };

            if (headSupervisor.CurrentProject != null && headSupervisor.CurrentProject.IsClosed)
            {
                headSupervisor.EditSupervisorFormData.AvailableProjects.Add(new EditHeadSupervisorFormDataDto.ListProjectsResponseDto
                {
                    Id = headSupervisor.CurrentProject.Id,
                    Name = headSupervisor.CurrentProject.Name,
                    IsClosed = headSupervisor.CurrentProject.IsClosed
                });
            }

            return new Result<GetHeadSupervisorResponseDto>(headSupervisor, true);
        }

        public async Task<Result> Edit(int headSupervisorId, EditHeadSupervisorRequestDto editHeadSupervisorRequestDto)
        {
            try
            {
                var supervisorUserData = await GetHeadSupervisorUser(headSupervisorId);

                var headSupervisorUser = supervisorUserData.User;

                headSupervisorUser.Name = editHeadSupervisorRequestDto.Name;
                headSupervisorUser.Sex = editHeadSupervisorRequestDto.Sex;
                headSupervisorUser.DecadeOfBirth = editHeadSupervisorRequestDto.DecadeOfBirth;
                headSupervisorUser.PhoneNumber = editHeadSupervisorRequestDto.PhoneNumber;
                headSupervisorUser.AdditionalPhoneNumber = editHeadSupervisorRequestDto.AdditionalPhoneNumber;
                headSupervisorUser.Organization = editHeadSupervisorRequestDto.Organization;

                await UpdateHeadSupervisorProjectReferences(headSupervisorUser, supervisorUserData.CurrentProjectReference, editHeadSupervisorRequestDto.ProjectId);
                await UpdateModem(headSupervisorUser, editHeadSupervisorRequestDto.ModemId, editHeadSupervisorRequestDto.NationalSocietyId);

                if (editHeadSupervisorRequestDto.OrganizationId.HasValue)
                {
                    var userLink = await _nyssContext.UserNationalSocieties
                        .Where(un => un.UserId == headSupervisorId && un.NationalSocietyId == editHeadSupervisorRequestDto.NationalSocietyId)
                        .SingleOrDefaultAsync();

                    if (editHeadSupervisorRequestDto.OrganizationId.Value != userLink.OrganizationId)
                    {
                        var validationResult = await _organizationService.CheckAccessForOrganizationEdition(userLink);

                        if (!validationResult.IsSuccess)
                        {
                            return validationResult;
                        }

                        userLink.Organization = await _nyssContext.Organizations.FindAsync(editHeadSupervisorRequestDto.OrganizationId.Value);
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

        public async Task<Result> Delete(int headSupervisorId)
        {
            try
            {
                await _deleteUserService.EnsureCanDeleteUser(headSupervisorId, Role.HeadSupervisor);

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var headSupervisorUserData = await GetHeadSupervisorUser(headSupervisorId);

                await EnsureHeadSupervisorHasNoSupervisors(headSupervisorUserData.User, headSupervisorUserData.CurrentProjectReference.ProjectId);
                await EnsureSupervisorHasNoDataCollectors(headSupervisorUserData.User);

                RemoveExistingProjectReference(headSupervisorUserData.CurrentProjectReference);
                RemoveAlertRecipientsReferences(headSupervisorUserData.User);

                AnonymizeHeadSupervisor(headSupervisorUserData.User);
                headSupervisorUserData.User.DeletedAt = _dateTimeProvider.UtcNow;

                await _identityUserRegistrationService.DeleteIdentityUser(headSupervisorUserData.User.IdentityUserId);
                headSupervisorUserData.User.IdentityUserId = null;

                await _nyssContext.SaveChangesAsync();
                transactionScope.Complete();

                return Success();
            }
            catch (ResultException e)
            {
                _loggerAdapter.Debug(e);
                return e.Result;
            }
        }

        private async Task<HeadSupervisorUser> CreateHeadSupervisorUser(IdentityUser identityUser, int nationalSocietyId, CreateHeadSupervisorRequestDto createHeadSupervisorRequestDto)
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

            var user = new HeadSupervisorUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = createHeadSupervisorRequestDto.Name,
                PhoneNumber = createHeadSupervisorRequestDto.PhoneNumber,
                AdditionalPhoneNumber = createHeadSupervisorRequestDto.AdditionalPhoneNumber,
                ApplicationLanguage = defaultUserApplicationLanguage,
                DecadeOfBirth = createHeadSupervisorRequestDto.DecadeOfBirth,
                Sex = createHeadSupervisorRequestDto.Sex,
                Organization = createHeadSupervisorRequestDto.Organization
            };

            await AddNewHeadSupervisorToProject(user, createHeadSupervisorRequestDto.ProjectId, nationalSocietyId);
            await AttachHeadSupervisorToModem(user, createHeadSupervisorRequestDto.ModemId, nationalSocietyId);

            var userNationalSociety = new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };

            if (createHeadSupervisorRequestDto.OrganizationId.HasValue)
            {
                userNationalSociety.Organization = await _nyssContext.Organizations
                    .Where(o => o.Id == createHeadSupervisorRequestDto.OrganizationId.Value && o.NationalSocietyId == nationalSocietyId)
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

        private async Task AttachHeadSupervisorToModem(HeadSupervisorUser user, int? modemId, int nationalSocietyId)
        {
            if (modemId.HasValue)
            {
                var modem = await _nyssContext.GatewayModems.FirstOrDefaultAsync(gm => gm.Id == modemId && gm.GatewaySetting.NationalSocietyId == nationalSocietyId);
                if (modem == null)
                {
                    throw new ResultException(ResultKey.User.Registration.CannotAssignUserToModemInDifferentNationalSociety);
                }

                user.Modem = modem;
            }
        }

        private async Task UpdateModem(HeadSupervisorUser user, int? modemId, int nationalSocietyId)
        {
            var modem = await _nyssContext.GatewayModems.FirstOrDefaultAsync(gm => gm.Id == modemId && gm.GatewaySetting.NationalSocietyId == nationalSocietyId);

            if (modemId.HasValue && modem == null)
            {
                throw new ResultException(ResultKey.User.Registration.CannotAssignUserToModemInDifferentNationalSociety);
            }

            user.Modem = modem;
        }


        private async Task AddNewHeadSupervisorToProject(HeadSupervisorUser user, int? projectId, int nationalSocietyId)
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

                await AttachHeadSupervisorToProject(user, project);
            }
        }

        private async Task AttachHeadSupervisorToProject(HeadSupervisorUser user, Project project)
        {
            var newSupervisorUserProject = new HeadSupervisorUserProject
            {
                Project = project,
                HeadSupervisorUser = user
            };

            user.CurrentProject = project;
            await _nyssContext.AddAsync(newSupervisorUserProject);
        }

        private async Task EnsureHeadSupervisorHasNoSupervisors(HeadSupervisorUser user, int projectId)
        {
            var hasSupervisors = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .SelectMany(p => p.NationalSociety.NationalSocietyUsers.Where(nsu => nsu.User.Role == Role.Supervisor))
                .Select(nsu => (SupervisorUser)nsu.User)
                .AnyAsync(su => su.HeadSupervisor == user);

            if (hasSupervisors)
            {
                throw new ResultException(ResultKey.User.Deletion.CannotDeleteHeadSupervisorHasSupervisors);
            }
        }

        private async Task EnsureSupervisorHasNoDataCollectors(HeadSupervisorUser headSupervisorUser)
        {
            var dataCollectorInfo = await _nyssContext.DataCollectors
                .Where(dc => dc.HeadSupervisor == headSupervisorUser)
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

        private void RemoveAlertRecipientsReferences(HeadSupervisorUser user)
        {
            var alertRecipients = user.HeadSupervisorUserAlertRecipients.ToList();
            _nyssContext.HeadSupervisorUserAlertRecipients.RemoveRange(alertRecipients);
        }

        private async Task UpdateHeadSupervisorProjectReferences(HeadSupervisorUser user, HeadSupervisorUserProject currentProjectReference, int? selectedProjectId)
        {
            var projectHasNotChanged = selectedProjectId.HasValue && selectedProjectId.Value == currentProjectReference?.ProjectId;
            if (projectHasNotChanged)
            {
                return;
            }

            if (selectedProjectId.HasValue)
            {
                var userProjectData = await _nyssContext.Projects
                    .Where(p => p.State == ProjectState.Open)
                    .Where(p => user.UserNationalSocieties.Select(uns => uns.NationalSocietyId).Contains(p.NationalSociety.Id))
                    .Select(p => new
                    {
                        Project = p,
                        HasSupervisors = p.NationalSociety.NationalSocietyUsers
                            .Where(nsu => nsu.User.Role == Role.Supervisor)
                            .Select(nsu => (SupervisorUser)nsu.User)
                            .Any(su => su.HeadSupervisor == user)
                    }).SingleOrDefaultAsync(p => p.Project.Id == selectedProjectId.Value);

                if (userProjectData == null)
                {
                    throw new ResultException(ResultKey.User.Supervisor.ProjectDoesNotExistOrNoAccess);
                }

                if (!userProjectData.HasSupervisors)
                {
                    throw new ResultException(ResultKey.User.HeadSupervisor.CannotChangeProjectHeadSupervisorHasSupervisors);
                }

                await AttachHeadSupervisorToProject(user, userProjectData.Project);
            }

            RemoveExistingProjectReference(currentProjectReference);
        }

        private void RemoveExistingProjectReference(HeadSupervisorUserProject existingProjectReference)
        {
            if (existingProjectReference != null)
            {
                _nyssContext.HeadSupervisorUserProjects.Remove(existingProjectReference);
            }
        }
        private void AnonymizeHeadSupervisor(HeadSupervisorUser headSupervisorUser)
        {
            headSupervisorUser.Name = Anonymization.Text;
            headSupervisorUser.EmailAddress = Anonymization.Text;
            headSupervisorUser.PhoneNumber = Anonymization.Text;
            headSupervisorUser.AdditionalPhoneNumber = Anonymization.Text;
        }

        private async Task<HeadSupervisorUserData> GetHeadSupervisorUser(int supervisorUserId)
        {
            var supervisorUserData = await _nyssContext.Users.FilterAvailable()
                .OfType<HeadSupervisorUser>()
                .Include(u => u.UserNationalSocieties)
                .ThenInclude(uns => uns.Organization)
                .Include(u => u.HeadSupervisorUserAlertRecipients)
                .Where(u => u.Id == supervisorUserId)
                .Select(u => new HeadSupervisorUserData
                {
                    User = u,
                    CurrentProjectReference = u.HeadSupervisorUserProjects
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
