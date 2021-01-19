using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Queries
{
    public static class SupervisorUserProjectQueries
    {
        public static IQueryable<SupervisorUserProject> FilterAvailableUsers(this IQueryable<SupervisorUserProject> supervisorUserProjects) =>
            supervisorUserProjects.Where(sup => !sup.SupervisorUser.DeletedAt.HasValue);

        public static IQueryable<SupervisorUserProject> FilterByCurrentUserRole(this IQueryable<SupervisorUserProject> supervisorUserProjects, User currentUser, int? organizationId)
        {
            if (currentUser.Role == Role.Administrator)
            {
                return supervisorUserProjects;
            }

            if (currentUser.Role == Role.HeadSupervisor)
            {
                return supervisorUserProjects.Where(sup => sup.SupervisorUser.HeadSupervisor.Id == currentUser.Id);
            }

            return supervisorUserProjects.Where(sup => sup.Project.NationalSociety.NationalSocietyUsers.Single(nsu => nsu.UserId == sup.SupervisorUserId).OrganizationId == organizationId);
        }

        public static IQueryable<SupervisorUserProject> FilterByProject(this IQueryable<SupervisorUserProject> supervisorUserProjects, int projectId) =>
            supervisorUserProjects.Where(sup => sup.ProjectId == projectId);
    }
}
