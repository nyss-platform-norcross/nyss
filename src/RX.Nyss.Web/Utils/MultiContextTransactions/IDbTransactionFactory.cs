using System.Threading.Tasks;
using RX.Nyss.Data;
using RX.Nyss.Web.Data;

namespace RX.Nyss.Web.Utils.MultiContextTransactions
{
    public interface IDbTransactionFactory
    {
        Task<IMultiContextTransaction<NyssContext, ApplicationDbContext>> BeginTransaction();
    }
}
