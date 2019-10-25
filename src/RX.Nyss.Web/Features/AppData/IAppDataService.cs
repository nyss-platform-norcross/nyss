using System.Collections.Generic;
using System.Threading.Tasks;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Utils.DataContract;

namespace RX.Nyss.Web.Features.AppData
{
    public interface IAppDataService
    {
        Task<IEnumerable<ContentLanguage>> GetLanguages();
    }
}
