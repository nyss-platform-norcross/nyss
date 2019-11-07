using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Project.Dto;
using RX.Nyss.Web.Utils.DataContract;
using RX.Nyss.Web.Utils.Logging;
using static RX.Nyss.Web.Utils.DataContract.Result;

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
        private readonly INyssContext _nyssContext;
        private readonly ILoggerAdapter _loggerAdapter;

        public ProjectService(INyssContext nyssContext, ILoggerAdapter loggerAdapter)
        {
            _nyssContext = nyssContext;
            _loggerAdapter = loggerAdapter;
        }

        public async Task<Result<ProjectResponseDto>> GetProject(int projectId)
        {
            var project = await _nyssContext.Projects
                .Select(p => new ProjectResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    TimeZone = p.TimeZone,
                    State = p.State
                })
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return Error<ProjectResponseDto>(ResultKey.Project.ProjectDoesNotExist);
            }

            var result = Success(project);

            return result;
        }

        public async Task<Result<List<ProjectListItemResponseDto>>> GetProjects(int nationalSocietyId)
        {
            var projects = await _nyssContext.Projects
                .Include(p => p.ProjectHealthRisks)
                .ThenInclude(phr => phr.Reports)
                .ThenInclude(r => r.ReportAlerts)
                .ThenInclude(ra => ra.Alert)
                .Include(p => p.DataCollectors)
                .Where(p => p.NationalSocietyId == nationalSocietyId)
                .OrderBy(p => p.State)
                .ThenBy(p => p.EndDate.HasValue)
                .ThenBy(p => p.EndDate)
                .ThenBy(p => p.Name)
                .Select(p => new ProjectListItemResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    State = p.State,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    ReportCount = p.ProjectHealthRisks.Sum(phr => phr.Reports.Count),
                    EscalatedAlertCount = p.ProjectHealthRisks
                        .SelectMany(phr => phr.Reports
                            .SelectMany(r => r.ReportAlerts
                                .Where(ra => ra.Alert.Status == AlertStatus.Escalated)
                                .Select(a => a.Alert.Id)
                            )
                        )
                        .Distinct()
                        .Count(),
                    ActiveDataCollectorCount = p.DataCollectors.Count,
                    SupervisorCount = -1
                })
                .ToListAsync();

            var result = Success(projects);

            return result;
        }

        public async Task<Result<int>> AddProject(int nationalSocietyId, ProjectRequestDto projectRequestDto)
        {
            try
            {
                var nationalSocietyExists = await _nyssContext.NationalSocieties.AnyAsync(ns => ns.Id == nationalSocietyId);

                if (!nationalSocietyExists)
                {
                    return Error<int>(ResultKey.Project.NationalSocietyDoesNotExist);
                }
                
                var projectToAdd = new Nyss.Data.Models.Project
                {
                    Name = projectRequestDto.Name,
                    TimeZone = projectRequestDto.TimeZone,
                    NationalSocietyId = nationalSocietyId,
                    State = ProjectState.Open,
                    StartDate = DateTime.UtcNow,
                    EndDate = null
                };

                await _nyssContext.Projects.AddAsync(projectToAdd);
                await _nyssContext.SaveChangesAsync();

                return Success(projectToAdd.Id, ResultKey.Project.SuccessfullyAdded);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.GetResult<int>();
            }
        }

        public async Task<Result> UpdateProject(int projectId, ProjectRequestDto projectRequestDto)
        {
            try
            {
                var projectToUpdate = await _nyssContext.Projects.FindAsync(projectId);

                if (projectToUpdate == null)
                {
                    return Error(ResultKey.Project.ProjectDoesNotExist);
                }

                projectToUpdate.Name = projectRequestDto.Name;
                projectToUpdate.TimeZone = projectRequestDto.TimeZone;

                await _nyssContext.SaveChangesAsync();

                return SuccessMessage(ResultKey.Project.SuccessfullyUpdated);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }

        public async Task<Result> DeleteProject(int projectId)
        {
            try
            {
                var projectToDelete = await _nyssContext.Projects.FindAsync(projectId);

                if (projectToDelete == null)
                {
                    return Error(ResultKey.Project.ProjectDoesNotExist);
                }

                _nyssContext.Projects.Remove(projectToDelete);
                await _nyssContext.SaveChangesAsync();

                return SuccessMessage(ResultKey.Project.SuccessfullyDeleted);
            }
            catch (ResultException exception)
            {
                _loggerAdapter.Debug(exception);
                return exception.Result;
            }
        }
    }
}
