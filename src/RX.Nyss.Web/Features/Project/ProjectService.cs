using System.Collections.Generic;
using System.Threading.Tasks;
using RX.Nyss.Web.Features.Project.Dto;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.Project
{
    public interface IProjectService
    {
        Task<Result<ProjectResponseDto>> GetProject(int projectId);
        Task<Result<List<ProjectListItemResponseDto>>> GetProjects(int nationalSocietyId);
        Task<Result<int>> AddProject(int nationalSocietyId, ProjectRequestDto projectRequestDto);
        Task<Result> UpdateProject(int projectId, ProjectRequestDto projectRequestDto);
        Task<Result> DeleteProject(int projectId);
    }

    public class ProjectService : IProjectService
    {
        public async Task<Result<ProjectResponseDto>> GetProject(int projectId) => throw new System.NotImplementedException();

        public async Task<Result<List<ProjectListItemResponseDto>>> GetProjects(int nationalSocietyId) => throw new System.NotImplementedException();

        public async Task<Result<int>> AddProject(int nationalSocietyId, ProjectRequestDto projectRequestDto) => throw new System.NotImplementedException();

        public async Task<Result> UpdateProject(int projectId, ProjectRequestDto projectRequestDto) => throw new System.NotImplementedException();

        public async Task<Result> DeleteProject(int projectId) => throw new System.NotImplementedException();
    }
}
