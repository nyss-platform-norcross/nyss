using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Eidsr.Dto;
using RX.Nyss.Web.Features.Eidsr.Queries;
using RX.Nyss.Web.Services.EidsrClient.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Eidsr;

[Route("api/eidsr")]
public class EidsrController : BaseController
{
    [HttpPost("organisationUnits")]
    [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Coordinator)]
    public async Task<Result<EidsrOrganisationUnitsResponse>> GetOrganisationUnits([FromBody] EidsrRequestDto eidsrOrganisationUnitsRequestDto) =>
        await Sender.Send(new GetEidsrOrganisationUnitsQuery(eidsrOrganisationUnitsRequestDto));

    [HttpPost("program")]
    [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Coordinator)]
    public async Task<Result<EidsrProgramResponse>> GetProgram([FromBody] EidsrRequestDto eidsrOrganisationUnitsRequestDto) =>
        await Sender.Send(new GetEidsrProgramQuery(eidsrOrganisationUnitsRequestDto));
}