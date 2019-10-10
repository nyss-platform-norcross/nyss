using System.Threading.Tasks;

namespace RX.Nyss.Web.Features.User
{
    public interface IUserService
    {
        Task<AddUserResult> AddUser(string email, string password, bool emailConfirmed = false);
        Task<AssignRoleResult> AssignRole(string email, string role);
        Task EnsureRoleExists(string role);
    }
}
