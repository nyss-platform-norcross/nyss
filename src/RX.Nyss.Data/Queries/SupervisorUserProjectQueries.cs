using System.Linq;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Queries
{
    public static class SupervisorUserProjectQueries
    {
        public static IQueryable<SupervisorUserProject> FilterAvailableUsers(this IQueryable<SupervisorUserProject> supervisorUserProjects) =>
            supervisorUserProjects.Where(sup => !sup.SupervisorUser.DeletedAt.HasValue);
    }
}
