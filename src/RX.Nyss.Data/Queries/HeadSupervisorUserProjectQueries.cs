using System.Linq;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Queries
{
    public static class HeadSupervisorUserProjectQueries
    {
        public static IQueryable<HeadSupervisorUserProject> FilterAvailableUsers(this IQueryable<HeadSupervisorUserProject> headSupervisorUserProjects) =>
            headSupervisorUserProjects.Where(sup => !sup.HeadSupervisorUser.DeletedAt.HasValue);
    }
}
