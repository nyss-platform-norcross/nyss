using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;

namespace RX.Nyss.Web.Features.EidsrConfiguration;

public interface IDistrictsOrgUnitsService
{
    Task<List<District>> GetNationalSocietyDistricts(int nationalSocietyId);
}

public class DistrictsOrgUnitsService : IDistrictsOrgUnitsService
{
    private readonly INyssContext _nyssContext;

    public DistrictsOrgUnitsService(INyssContext nyssContext)
    {
        _nyssContext = nyssContext;
    }

    public async Task<List<District>> GetNationalSocietyDistricts(int nationalSocietyId)
    {
        var res = await _nyssContext
            .Regions.Where(x => x.NationalSociety.Id == nationalSocietyId)
            .Include(x => x.Districts)
            .ThenInclude(x => x.EidsrOrganisationUnits)
            .SelectMany(x => x.Districts)
            .AsSplitQuery()
            .ToListAsync();

        return res;
    }
}
