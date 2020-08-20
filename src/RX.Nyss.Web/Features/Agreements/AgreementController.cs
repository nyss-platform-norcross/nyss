using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Utils;

namespace RX.Nyss.Web.Features.Agreements
{
    [Route("api/agreement")]
    public class AgreementController : BaseController
    {
        private readonly IAgreementService _agreementService;

        public AgreementController(IAgreementService agreementService)
        {
            _agreementService = agreementService;
        }

        /// <summary>
        /// Get all national societies that have pending, either new or updated, agreements for the current user
        /// </summary>
        /// <returns></returns>
        [HttpGet("listPending")]
        public async Task<Result> Status() =>
            await _agreementService.GetPendingAgreements();

        /// <summary>
        /// Get agreement documents in all languages for the pending national societies for the current user
        /// </summary>
        /// <returns></returns>
        [HttpGet("pendingAgreementDocuments")]
        [NeedsRole(Role.GlobalCoordinator, Role.Administrator, Role.Manager, Role.TechnicalAdvisor, Role.Coordinator)]
        public async Task<Result> GetPendingAgreementDocuments() =>
            await _agreementService.GetPendingAgreementDocuments();

        /// <summary>
        /// Will make the current user accept any pending agreements and storing it with the timestamp and a reference to a copied
        /// version of the agreement document in the selected language. It will also set the current user as the head manager for the default organization if he or she is pending as that.
        /// </summary>
        /// <param name="languageCode">The selected language the user has chosen to see the agreement in</param>
        /// <returns></returns>
        [HttpPost("accept")]
        [NeedsRole(Role.Manager, Role.TechnicalAdvisor, Role.Coordinator)]
        public async Task<Result> AcceptAgreement(string languageCode) =>
            await _agreementService.AcceptAgreement(languageCode);
    }
}
