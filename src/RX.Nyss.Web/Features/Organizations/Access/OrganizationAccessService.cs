using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Web.Features.NationalSocieties.Access;

namespace RX.Nyss.Web.Features.Organizations.Access
{
    public interface IOrganizationAccessService
    {
        Task<bool> HasCurrentUserAccessToOrganization(int organizationId);
    }

    public class OrganizationAccessService : IOrganizationAccessService
    {
        private readonly INyssContext _nyssContext;
        private readonly INationalSocietyAccessService _nationalSocietyAccessService;

        public OrganizationAccessService(
            INyssContext nyssContext,
            INationalSocietyAccessService nationalSocietyAccessService)
        {
            _nyssContext = nyssContext;
            _nationalSocietyAccessService = nationalSocietyAccessService;
        }

        public async Task<bool> HasCurrentUserAccessToOrganization(int organizationId)
        {
            var nationalSocietyId = await _nyssContext.Organizations
                .Where(g => g.Id == organizationId)
                .Select(s => s.NationalSociety.Id)
                .SingleAsync();

            return await _nationalSocietyAccessService.HasCurrentUserAccessToNationalSocieties(new[] { nationalSocietyId });
        }
    }
}
