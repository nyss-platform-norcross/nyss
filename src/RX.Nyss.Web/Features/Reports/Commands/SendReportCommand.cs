using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Services.Authorization;

namespace RX.Nyss.Web.Features.Reports.Commands
{
    public class SendReportCommand : IRequest<Result>
    {
        public int DataCollectorId { get; set; }

        public string Timestamp { get; set; }

        public string Text { get; set; }

        public int? ModemId { get; set; }

        public int UtcOffset { get; set; }

        public class Handler : IRequestHandler<SendReportCommand, Result>
        {
            private readonly INyssContext _nyssContext;

            private readonly IAuthorizationService _authorizationService;

            private readonly IHttpClientFactory _httpClientFactory;

            private readonly INyssWebConfig _config;

            public Handler(
                INyssContext nyssContext,
                IAuthorizationService authorizationService,
                IHttpClientFactory httpClientFactory,
                INyssWebConfig config)
            {
                _nyssContext = nyssContext;
                _authorizationService = authorizationService;
                _httpClientFactory = httpClientFactory;
                _config = config;
            }

            public async Task<Result> Handle(SendReportCommand request, CancellationToken cancellationToken)
            {
                using var httpClient = _httpClientFactory.CreateClient();
                var isHeadSupervisor = _authorizationService.IsCurrentUserInRole(Role.HeadSupervisor);

                var dataCollectorData = await _nyssContext.DataCollectors
                    .Where(dc => dc.Id == request.DataCollectorId)
                    .Select(dc => new
                    {
                        DataCollectorId = dc.Id,
                        NationalSocietyId = dc.Project.NationalSocietyId
                    })
                    .SingleOrDefaultAsync(cancellationToken);

                var gatewayData = await _nyssContext.GatewaySettings
                    .Include(gs => gs.Modems)
                    .Where(gs => gs.NationalSocietyId == dataCollectorData.NationalSocietyId)
                    .Select(gs => new
                    {
                        ApiKey = gs.ApiKey,
                        Modem = gs.Modems.FirstOrDefault(gm => gm.Id == request.ModemId)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (gatewayData == null)
                {
                    return Result.Error(ResultKey.Report.NoGatewaySettingFoundForNationalSociety);
                }

                var baseUri = new Uri(_config.FuncAppBaseUrl);
                var requestUri = new Uri(baseUri, new Uri("/api/enqueueNyssReport", UriKind.Relative));
                var reportProps = new Dictionary<string, string>
                {
                    { "Datacollectorid", dataCollectorData.DataCollectorId.ToString() },
                    { "Timestamp", request.Timestamp },
                    { "Text", request.Text },
                    { "Modemno", gatewayData.Modem?.ModemId.ToString() },
                    { "Headsupervisor", isHeadSupervisor ? "true" : null },
                    { "UtcOffset", request.UtcOffset.ToString() },
                    { "Apikey", gatewayData.ApiKey }
                };
                var content = new FormUrlEncodedContent(reportProps);

                var responseMessage = await httpClient.PostAsync(requestUri, content, cancellationToken);

                responseMessage.EnsureSuccessStatusCode();

                return Result.Success();
            }
        }

        public class Validator : AbstractValidator<SendReportCommand>
        {
            public Validator()
            {
                RuleFor(x => x.DataCollectorId).GreaterThan(0);
                RuleFor(x => x.Timestamp).NotNull().NotEmpty();
                RuleFor(x => x.Text).NotNull().NotEmpty();
                RuleFor(x => x.ModemId).NotEmpty().When(x => x.ModemId.HasValue);
                RuleFor(x => x.UtcOffset).NotEmpty();
            }
        }
    }
}
