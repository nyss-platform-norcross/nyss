using System.Threading.Tasks;
using RX.Nyss.FuncApp.Contracts;

namespace RX.Nyss.FuncApp.Services
{
    public interface IEmailClient
    {
        Task SendEmail(SendEmailMessage message, bool sandboxMode);
        Task SendEmailAsTextOnly(SendEmailMessage message, bool sandboxMode);
    }
}
