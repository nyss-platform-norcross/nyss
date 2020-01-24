using System.Linq;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Data.Queries
{
    public static class UserNationalSocietyQueries
    {
        public static IQueryable<UserNationalSociety> FilterAvailableUsers(this IQueryable<UserNationalSociety> userNationalSocieties) =>
            userNationalSocieties.Where(uns => !uns.User.DeletedAt.HasValue);
    }
}
