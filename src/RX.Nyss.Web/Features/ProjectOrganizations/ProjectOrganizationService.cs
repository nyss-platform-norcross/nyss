using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.ProjectOrganizations.Dto;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.ProjectOrganizations
{
    public interface IProjectOrganizationService
    {
        Task<Result<List<ProjectOrganizationListResponseDto>>> List(int projectId);
        Task<Result<int>> Create(int projectId, ProjectOrganizationRequestDto createDto);
        Task<Result> Delete(int projectOrganizationId);
        Task<Result<ProjectOrganizationCreateDataResponseDto>> GetCreateData(int projectId);
    }

    public class ProjectOrganizationService : IProjectOrganizationService
    {
        private readonly INyssContext _nyssContext;
        private readonly IAuthorizationService _authorizationService;

        public ProjectOrganizationService(
            INyssContext nyssContext,
            IAuthorizationService authorizationService)
        {
            _nyssContext = nyssContext;
            _authorizationService = authorizationService;
        }

        public async Task<Result<List<ProjectOrganizationListResponseDto>>> List(int projectId)
        {
            var projectOrganizations = await _nyssContext.ProjectOrganizations
                .Where(gs => gs.ProjectId == projectId)
                .OrderBy(gs => gs.Id)
                .Select(gs => new ProjectOrganizationListResponseDto
                {
                    Id = gs.Id,
                    Name = gs.Organization.Name,
                    IsDefaultOrganization = gs.Organization.NationalSociety.DefaultOrganization == gs.Organization
                })
                .AsSplitQuery()
                .ToListAsync();

            var result = Success(projectOrganizations);

            return result;
        }

        public async Task<Result<ProjectOrganizationCreateDataResponseDto>> GetCreateData(int projectId)
        {
            var dto = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => new ProjectOrganizationCreateDataResponseDto
                {
                    Organizations = p.NationalSociety.Organizations
                        .Where(o => !p.ProjectOrganizations.Any(po => po.OrganizationId == o.Id))
                        .Select(o => new ProjectOrganizationCreateDataResponseDto.CreateProjectOrganizationDto
                        {
                            Id = o.Id,
                            Name = o.Name
                        })
                })
                .SingleAsync();

            return Success(dto);
        }

        public async Task<Result<int>> Create(int projectId, ProjectOrganizationRequestDto createDto)
        {
            var projectData = await _nyssContext.Projects
                .Where(p => p.Id == projectId)
                .Select(p => new
                {
                    p.AllowMultipleOrganizations,
                    IsClosed = p.EndDate.HasValue,
                    OrganiationsCount = p.ProjectOrganizations.Count
                })
                .SingleAsync();

            if (projectData.IsClosed)
            {
                return Error<int>(ResultKey.ProjectOrganization.ProjectIsClosed);
            }

            if (projectData.OrganiationsCount > 0 && !projectData.AllowMultipleOrganizations)
            {
                return Error<int>(ResultKey.ProjectOrganization.ProjectDoesNotAllowAddingMultipleOrganizations);
            }

            var projectOrganizationToAdd = new ProjectOrganization
            {
                OrganizationId = createDto.OrganizationId,
                ProjectId = projectId
            };

            await _nyssContext.ProjectOrganizations.AddAsync(projectOrganizationToAdd);
            await _nyssContext.SaveChangesAsync();

            return Success(projectOrganizationToAdd.Id);
        }

        public async Task<Result> Delete(int projectOrganizationId)
        {
            var data = await _nyssContext.ProjectOrganizations
                .Where(po => po.Id == projectOrganizationId)
                .Select(po => new
                {
                    ProjectOrganization = po,
                    HasOrganizationSupervisorsInProject = po.Project.NationalSociety.NationalSocietyUsers
                        .Where(nsu => nsu.OrganizationId == po.OrganizationId)
                        .Select(nsu => nsu.User)
                        .OfType<SupervisorUser>()
                        .Any(nsu => nsu.CurrentProject.Id == po.ProjectId),
                    ProjectOrganizationsCount = po.Project.ProjectOrganizations.Count
                })
                .SingleAsync();

            if (data.HasOrganizationSupervisorsInProject)
            {
                return Error<int>(ResultKey.ProjectOrganization.CannotRemoveOrganizationThatHasSupervisors);
            }

            if (data.ProjectOrganizationsCount == 1)
            {
                return Error<int>(ResultKey.ProjectOrganization.CannotRemoveLastOrganization);
            }

            _nyssContext.ProjectOrganizations.Remove(data.ProjectOrganization);

            await _nyssContext.SaveChangesAsync();
            return Success();
        }
    }
}
