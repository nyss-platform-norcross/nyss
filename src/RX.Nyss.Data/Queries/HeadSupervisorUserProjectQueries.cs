using System.Linq;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Queries
{
    public static class HeadSupervisorUserProjectQueries
    {
        public static IQueryable<HeadSupervisorUserProject> FilterAvailableUsers(this IQueryable<HeadSupervisorUserProject> headSupervisorUserProjects) =>
            headSupervisorUserProjects.Where(sup => !sup.HeadSupervisorUser.DeletedAt.HasValue);

        public static IQueryable<HeadSupervisorUserProject> FilterByCurrentUserRole(this IQueryable<HeadSupervisorUserProject> headSupervisorUserProjects, User currentUser, int? organizationId)
        {
            if (currentUser.Role == Role.Administrator)
            {
                return headSupervisorUserProjects;
            }

            if (currentUser.Role == Role.Supervisor)
            {
                return headSupervisorUserProjects.Take(0);
            }

            if (currentUser.Role == Role.HeadSupervisor)
            {
                return headSupervisorUserProjects.Where(sup => sup.HeadSupervisorUserId == currentUser.Id);
            }

            return headSupervisorUserProjects
                .Where(sup => sup.Project.NationalSociety.NationalSocietyUsers
                    .Single(nsu => nsu.UserId == sup.HeadSupervisorUserId)
                    .OrganizationId == organizationId);
        }

        public static IQueryable<HeadSupervisorUserProject> FilterByProject(this IQueryable<HeadSupervisorUserProject> headSupervisorUserProjects, int projectId) =>
            headSupervisorUserProjects.Where(sup => sup.ProjectId == projectId);
    }
}
