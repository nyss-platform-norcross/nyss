using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.EidsrConfiguration.Commands;
using RX.Nyss.Web.Features.EidsrConfiguration.Dto;
using RX.Nyss.Web.Features.EidsrConfiguration.Queries;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.EidsrConfiguration;

[Route("api/eidsrconfiguration")]
public class EidsrConfigurationController : BaseController
{
    /// <summary>
    /// Gets EidsrConfiguration for National Society.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{nationalSocietyId}/get")]
    [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.Supervisor, Role.Coordinator, Role.HeadSupervisor),
     NeedsPolicy(Policy.NationalSocietyAccess)]
    public async Task<Result<EidsrIntegrationResponseDto>> Get(int nationalSocietyId) =>
        await Sender.Send(new GetEidsrIntegrationQuery(nationalSocietyId));

    /// <summary>
    /// Edits EidsrConfiguration (or creates if there is no EidsrConfiguration).
    /// </summary>
    /// <param name="nationalSocietyId"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    [HttpPost("{nationalSocietyId}/edit")]
    [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Coordinator),
     NeedsPolicy(Policy.NationalSocietyAccess)]
    public async Task<Result> Edit(int nationalSocietyId, [FromBody] EditEidsrIntegrationCommand.RequestBody body) =>
        await Sender.Send(new EditEidsrIntegrationCommand(nationalSocietyId, body));
}

