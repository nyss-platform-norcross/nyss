using System.Threading.Tasks;

namespace RX.Nyss.Web.Features.User
{
    public interface IUserService
    {
        Task AddUser(string email, string password, bool emailConfirmed = false);
        Task AssignRole(string email, string role);
        Task EnsureRoleExists(string role);
    }
}
