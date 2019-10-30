using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Rx.Nyss.Web.Tests.Common
{
    public static class DbSetInitializer
    {
        public static void InitDb<T>(IQueryable<T> set, IQueryable<T> data) where T : class
        {
            set.Provider.Returns(data.Provider);
            set.Expression.Returns(data.Expression);
            set.ElementType.Returns(data.ElementType);
            set.GetEnumerator().Returns(data.GetEnumerator());
            set.AsNoTracking().Returns(data);
        }

        public static void InitDb<T>(DbSet<T> set, List<T> data) where T : class
        {
            InitDb(set, data.AsQueryable());
            set.When(x => x.Add(Arg.Any<T>())).Do(x => data.Add((T)x.Args()[0]));
            set.When(x => x.AddAsync(Arg.Any<T>())).Do(x => data.Add((T)x.Args()[0]));
            set.When(x => x.AddRangeAsync(Arg.Any<IEnumerable<T>>())).Do(x => data.AddRange((IEnumerable<T>)x.Args()[0]));
            set.When(x => x.AddRange(Arg.Any<IEnumerable<T>>())).Do(x => data.AddRange((IEnumerable<T>)x.Args()[0]));
        }
    }
}
