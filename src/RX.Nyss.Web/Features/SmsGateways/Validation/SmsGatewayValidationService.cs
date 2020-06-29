using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.SmsGateways.Validation
{
    public interface ISmsGatewayValidationService
    {
        Task<bool> NationalSocietyExists(int nationalSocietyId);
        Task<bool> GatewayExists(int smsGatewayId);
        Task<bool> ApiKeyExists(string apiKey);
    }

    public class SmsGatewayValidationService : ISmsGatewayValidationService
    {
        private readonly INyssContext _nyssContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SmsGatewayValidationService(INyssContext nyssContext, IHttpContextAccessor httpContextAccessor)
        {
            _nyssContext = nyssContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> NationalSocietyExists(int nationalSocietyId) =>
            await _nyssContext.NationalSocieties.AnyAsync(ns => ns.Id == nationalSocietyId);

        public async Task<bool> GatewayExists(int smsGatewayId) =>
            await _nyssContext.GatewaySettings.AnyAsync(gs => gs.Id == smsGatewayId);

        public async Task<bool> ApiKeyExists(string apiKey) =>
            await _nyssContext.GatewaySettings.AnyAsync(gs => gs.ApiKey == apiKey);
    }
}
