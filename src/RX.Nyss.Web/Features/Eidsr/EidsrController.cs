using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.Eidsr.Dto;
using RX.Nyss.Web.Features.Eidsr.Queries;
using RX.Nyss.Web.Services.EidsrClient;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Eidsr;

[Route("api/eidsr")]
public class EidsrController : BaseController
{
    [HttpPost("organisationUnits")]
//    [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.Supervisor, Role.Coordinator, Role.HeadSupervisor),
//     NeedsPolicy(Policy.NationalSocietyAccess)]
    public async Task<Result<EidsrOrganisationUnitsResponse>> Get([FromBody] EidsrOrganisationUnitsRequestDto eidsrOrganisationUnitsRequestDto) =>
        await Sender.Send(new GetEidsrOrganisationUnitsQuery(eidsrOrganisationUnitsRequestDto));
}