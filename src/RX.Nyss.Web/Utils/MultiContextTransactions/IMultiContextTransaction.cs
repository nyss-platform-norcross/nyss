using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RX.Nyss.Web.Utils.MultiContextTransactions
{
    public interface IMultiContextTransaction<TFirstContext, TSecondContext> : IDisposable
        where TFirstContext : DbContext
        where TSecondContext : DbContext
    {
        Task Commit();
    }
}
