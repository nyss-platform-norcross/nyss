using System.Linq;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Queries
{
    public static class UserQueries
    {
        public static IQueryable<User> FilterAvailable(this IQueryable<User> users) =>
            users.Where(u => !u.DeletedAt.HasValue);
    }
}
