using System.Threading.Tasks;

namespace RX.Nyss.ReportApi.Handlers
{
    public interface ISmsHandler
    {
        bool CanHandle(string queryString);

        Task Handle(string queryString);
    }
}
