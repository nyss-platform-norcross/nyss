using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;

namespace RX.Nyss.Web.Features.DataCollectors.Validation
{
    public interface IDataCollectorValidationService
    {
        Task<bool> PhoneNumberExists(string phoneNumber);
    }

    public class DataCollectorValidationService : IDataCollectorValidationService
    {
        private readonly INyssContext _nyssContext;

        public DataCollectorValidationService(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public async Task<bool> PhoneNumberExists(string phoneNumber) => 
            await _nyssContext.DataCollectors.AnyAsync(dc => dc.PhoneNumber == phoneNumber);
    }
}