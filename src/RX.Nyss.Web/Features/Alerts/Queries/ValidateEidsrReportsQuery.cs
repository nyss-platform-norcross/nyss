using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Repositories;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Services.EidsrService;
using static RX.Nyss.Common.Utils.DataContract.Result;
using EidsrApiProperties = RX.Nyss.Web.Services.EidsrClient.Dto.EidsrApiProperties;
using RX.Nyss.Common.Utils.Logging;

namespace RX.Nyss.Web.Features.Alerts.Queries;

public class ValidateEidsrReportsQuery : IRequest<Result<ValidateEidsrReportsResponse>>
{
    public ValidateEidsrReportsQuery(int alertId)
    {
        AlertId = alertId;
    }

    private int AlertId { get; }

    public class Handler : IRequestHandler<ValidateEidsrReportsQuery, Result<ValidateEidsrReportsResponse>>
    {
        private readonly IEidsrRepository _repository;
        private readonly IEidsrService _eidsrService;
        private readonly INyssWebConfig _nyssWebConfig;
        private readonly ICryptographyService _cryptographyService;
        private readonly ILoggerAdapter _logger;

        public Handler(
            IEidsrRepository repository,
            IEidsrService eidsrService,
            INyssWebConfig nyssWebConfig,
            ICryptographyService cryptographyService,
            ILoggerAdapter logger)
        {
            _repository = repository;
            _eidsrService = eidsrService;
            _nyssWebConfig = nyssWebConfig;
            _cryptographyService = cryptographyService;
            _logger = logger;
        }

        private static readonly EidsrDbReportTemplateValidator _templateValidator = new();
        private static readonly EidsrDbReportDataValidator _reportDataValidator = new();

        public async Task<Result<ValidateEidsrReportsResponse>> Handle(ValidateEidsrReportsQuery request, CancellationToken cancellationToken)
        {
            var res = new ValidateEidsrReportsResponse();

            try
            {
                var reports = _repository.GetReportsForEidsr(request.AlertId).ToList();

                var template = reports.FirstOrDefault()?.EidsrDbReportTemplate;

                var templateValidation = await _templateValidator.ValidateAsync(template ?? new EidsrDbReportTemplate(), cancellationToken);
                res.IsIntegrationConfigValid = templateValidation.IsValid;

                if (!res.IsIntegrationConfigValid)
                {
                    res.IsEidsrApiConnectionRunning = false;
                }
                else
                {
                    var pingResult = await _eidsrService.GetProgram(new EidsrApiProperties
                    {
                        Password = _cryptographyService.Decrypt(
                         template?.EidsrApiProperties.PasswordHash ?? string.Empty,
                         _nyssWebConfig.VariableKey,
                         _nyssWebConfig.SupplementaryKey),
                        Url = template?.EidsrApiProperties.Url ?? string.Empty,
                        UserName = template?.EidsrApiProperties.UserName ?? string.Empty,
                    }, template?.Program ?? string.Empty);

                    res.IsEidsrApiConnectionRunning = pingResult.IsSuccess && !string.IsNullOrEmpty(pingResult.Value?.Name);
                }
                // Validate all reports in parallel
                var validationTasks = reports
                    .Select(r => _reportDataValidator.ValidateAsync(r.EidsrDbReportData, cancellationToken));

                var validationResults = await Task.WhenAll(validationTasks);
                res.AreReportsValidCount = validationResults.Count(v => v.IsValid);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to validate EIDSR reports for alert");
                res.IsEidsrApiConnectionRunning = false;
                res.IsIntegrationConfigValid = false;
                res.AreReportsValidCount = 0;
            }

            return Success(res);
        }
    }
}

public class ValidateEidsrReportsResponse
{
    public bool IsIntegrationConfigValid { get; set; }

    public bool IsEidsrApiConnectionRunning { get; set; }

    public int AreReportsValidCount { get; set; }
}

public class EidsrDbReportTemplateValidator : AbstractValidator<EidsrDbReportTemplate>
{
    public EidsrDbReportTemplateValidator()
    {
        RuleFor(x => x.Program).NotEmpty();

        RuleFor(x => x.EidsrApiProperties).NotEmpty().DependentRules(() =>
        {
            RuleFor(x => x.EidsrApiProperties.Url).NotEmpty();
            RuleFor(x => x.EidsrApiProperties.PasswordHash).NotEmpty();
            RuleFor(x => x.EidsrApiProperties.UserName).NotEmpty();
        });
    }
}

public class EidsrDbReportDataValidator : AbstractValidator<EidsrDbReportData>
{
    public EidsrDbReportDataValidator()
    {
        RuleFor(x => x.OrgUnit).NotEmpty();
    }
}
