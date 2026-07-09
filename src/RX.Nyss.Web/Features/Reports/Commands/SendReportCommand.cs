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

            public async Task<Result> Handle(
    SendReportCommand request,
    CancellationToken cancellationToken)
            {
                // 1. Authorization
                var isHeadSupervisor = _authorizationService.IsCurrentUserInRole(Role.HeadSupervisor);

                // 2. Load data collector + national society
                var dataCollector = await _nyssContext.DataCollectors
                    .Where(dc => dc.Id == request.DataCollectorId)
                    .Select(dc => new
                    {
                        dc.Id,
                        dc.Project.NationalSocietyId
                    })
                    .SingleOrDefaultAsync(cancellationToken);

                if (dataCollector == null)
                {
                    return Result.Error(ResultKey.Report.DataCollectorNotFound);
                }

                // 3. Load gateway settings
                var gateway = await _nyssContext.GatewaySettings
                    .Where(gs => gs.NationalSocietyId == dataCollector.NationalSocietyId)
                    .Select(gs => new
                    {
                        gs.ApiKey,
                        Modem = gs.Modems.FirstOrDefault(m => m.Id == request.ModemId)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (gateway == null)
                {
                    return Result.Error(ResultKey.Report.NoGatewaySettingFoundForNationalSociety);
                }

                if (string.IsNullOrWhiteSpace(gateway.ApiKey))
                {
                    return Result.Error(ResultKey.Report.NoGatewaySettingFoundForNationalSociety);
                }

                // Optional: enforce modem existence if required
                if (request.ModemId.HasValue && gateway.Modem == null)
                {
                    return Result.Error(ResultKey.Report.NoGatewaySettingFoundForNationalSociety);
                }

                // 4. Build request payload safely
                var payload = new Dictionary<string, string>
                {
                    ["Datacollectorid"] = dataCollector.Id.ToString(),
                    ["Timestamp"] = request.Timestamp,
                    ["Text"] = request.Text,
                    ["UtcOffset"] = request.UtcOffset.ToString(),
                    ["Apikey"] = gateway.ApiKey
                };

                if (gateway.Modem != null)
                {
                    payload["Modemno"] = gateway.Modem.ModemId.ToString();
                }

                if (isHeadSupervisor)
                {
                    payload["Headsupervisor"] = "true";
                }

                // 5. Send HTTP request
                using var httpClient = _httpClientFactory.CreateClient();

                var requestUri = new Uri(
                    new Uri(_config.FuncAppBaseUrl.TrimEnd('/') + '/'),
                    "api/enqueueNyssReport");

                using var content = new FormUrlEncodedContent(payload);

                var response = await httpClient.PostAsync(requestUri, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    return Result.Error(ResultKey.Report.EnqueueNyssReportFailed, errorBody);
                }

                return Result.Success();
            }


            public class Validator : AbstractValidator<SendReportCommand>
            {
                public Validator()
                {
                    RuleFor(x => x.DataCollectorId)
                        .GreaterThan(0);

                    RuleFor(x => x.Timestamp)
                        .NotEmpty();

                    RuleFor(x => x.Text)
                        .NotEmpty();

                    RuleFor(x => x.UtcOffset)
                        .InclusiveBetween(-12, 14);

                    // If modem is required ONLY when provided
                    RuleFor(x => x.ModemId)
                        .NotNull()
                        .When(x => x.ModemId.HasValue);
                }
            }

        }
    }
}


