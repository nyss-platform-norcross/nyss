using System.Threading.Tasks;
using RX.Nyss.Data;
using RX.Nyss.Web.Data;

namespace RX.Nyss.Web.Utils.MultiContextTransactions
{
    public class DbTransactionFactory : IDbTransactionFactory
    {
        private readonly NyssContext _nyssContext;
        private readonly ApplicationDbContext _applicationDbContext;

        public DbTransactionFactory(NyssContext nyssContext, ApplicationDbContext applicationDbContext)
        {
            _nyssContext = nyssContext;
            _applicationDbContext = applicationDbContext;
        }

        public async Task<IMultiContextTransaction<NyssContext, ApplicationDbContext>> BeginTransaction() 
            => await MultiContextTransaction<NyssContext, ApplicationDbContext>.Begin(_nyssContext, _applicationDbContext);
    }
}
