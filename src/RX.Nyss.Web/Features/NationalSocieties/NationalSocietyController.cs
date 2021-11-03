using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common;
using RX.Nyss.Web.Features.NationalSocieties.Commands;
using RX.Nyss.Web.Features.NationalSocieties.Dto;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.NationalSocieties
{
    [Route("api/nationalSociety")]
    public class NationalSocietyController : BaseController
    {
        private readonly INationalSocietyService _nationalSocietyService;

        public NationalSocietyController(INationalSocietyService nationalSocietyService)
        {
            _nationalSocietyService = nationalSocietyService;
        }

        /// <summary>
        /// Gets all countries with country codes.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{nationalSocietyId}/get")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.DataConsumer, Role.Supervisor, Role.Coordinator, Role.HeadSupervisor), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result<NationalSocietyResponseDto>> Get(int nationalSocietyId) =>
            await _nationalSocietyService.Get(nationalSocietyId);

        /// <summary>
        /// Gets all countries with country codes.
        /// </summary>
        /// <returns></returns>
        [HttpGet("list")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.TechnicalAdvisor, Role.DataConsumer, Role.Coordinator)]
        public async Task<Result<List<NationalSocietyListResponseDto>>> List() =>
            await _nationalSocietyService.List();

        /// <summary>
        /// Creates a new National Society.
        /// </summary>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator)]
        public async Task<Result> Create([FromBody] CreateNationalSocietyRequestDto nationalSociety) =>
            await _nationalSocietyService.Create(nationalSociety);

        /// <summary>
        /// Edits an existing National Society.
        /// </summary>
        /// <param name="nationalSocietyId"></param>
        /// <param name="nationalSociety"></param>
        /// <returns></returns>
        [HttpPost("{nationalSocietyId}/edit")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Coordinator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Edit(int nationalSocietyId, [FromBody] EditNationalSocietyRequestDto nationalSociety) =>
            await _nationalSocietyService.Edit(nationalSocietyId, nationalSociety);

        /// <summary>
        /// Archives an existing National Society.
        /// </summary>
        /// <param name="nationalSocietyId"></param>
        /// <returns></returns>
        [HttpPost("{nationalSocietyId}/archive")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Archive(int nationalSocietyId) =>
            await Sender.Send(new ArchiveCommand(nationalSocietyId));

        /// <summary>
        /// Reopens an archived National Society.
        /// </summary>
        /// <param name="nationalSocietyId"></param>
        /// <returns></returns>
        [HttpPost("{nationalSocietyId}/reopen")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator), NeedsPolicy(Policy.NationalSocietyAccess)]
        public async Task<Result> Reopen(int nationalSocietyId) =>
            await _nationalSocietyService.Reopen(nationalSocietyId);
    }
}
