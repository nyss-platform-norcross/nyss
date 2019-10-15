using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace RX.Nyss.Web.Utils.MultiContextTransactions
{
    public class MultiContextTransaction<TFirstContext, TSecondContext>: IMultiContextTransaction<TFirstContext, TSecondContext>
        where TFirstContext: DbContext
        where TSecondContext: DbContext
    {
        private readonly TFirstContext _firstContext;
        private readonly TSecondContext _secondContext;
        private IDbContextTransaction _transaction;

        private MultiContextTransaction(TFirstContext firstContext, TSecondContext secondContext)
        {
            _firstContext = firstContext;
            _secondContext = secondContext;
        }

        private async Task<MultiContextTransaction<TFirstContext, TSecondContext>> Initialize()
        {
            _transaction = await _firstContext.Database.BeginTransactionAsync();
            await _secondContext.Database.UseTransactionAsync(_transaction.GetDbTransaction());
            return this;
        }

        public async Task Commit() => await _transaction.CommitAsync();

        public void Dispose() => _transaction?.Dispose();

        public static async Task<IMultiContextTransaction<TFirstContext, TSecondContext>> Begin(TFirstContext firstContext, TSecondContext secondContext)
        {
            var multiContextTransaction = new MultiContextTransaction<TFirstContext, TSecondContext>(firstContext, secondContext);
            return await multiContextTransaction.Initialize();
        }
    }
}
