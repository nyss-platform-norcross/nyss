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
        public Handler(
            IEidsrRepository repository,
            IEidsrService eidsrService,
            INyssWebConfig nyssWebConfig,
            ICryptographyService cryptographyService)
        {
            _repository = repository;
            _eidsrService = eidsrService;
            _nyssWebConfig = nyssWebConfig;
            _cryptographyService = cryptographyService;
        }

        public async Task<Result<ValidateEidsrReportsResponse>> Handle(ValidateEidsrReportsQuery request, CancellationToken cancellationToken)
        {
            var reports = _repository.GetReportsForEidsr(request.AlertId);

            var res = new ValidateEidsrReportsResponse();

            // assume all reports have the same template (come from the same national society)
            var template = reports.FirstOrDefault()?.EidsrDbReportTemplate;
            var templateValidation = await new EidsrDbReportTemplateValidator().ValidateAsync(template, cancellationToken);

            res.IsIntegrationConfigValid = templateValidation.IsValid;

            var pingResult = await _eidsrService.GetProgram(new EidsrApiProperties
            {
                Password = _cryptographyService.Decrypt(
                    template?.EidsrApiProperties.PasswordHash,
                    _nyssWebConfig.Key,
                    _nyssWebConfig.SupplementaryKey),
                Url = template?.EidsrApiProperties.Url,
                UserName = template?.EidsrApiProperties.UserName,
            }, template?.Program);

            res.IsEidsrApiConnectionRunning = pingResult.IsSuccess && !string.IsNullOrEmpty(pingResult.Value.Name);

            res.AreReportsValidCount = 0;

            foreach (var report in reports)
            {
                var validation = await new EidsrDbReportDataValidator().ValidateAsync(report.EidsrDbReportData, cancellationToken);

                if (validation.IsValid)
                {
                    res.AreReportsValidCount++;
                }
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
        RuleFor(x => x.GenderDataElementId).NotEmpty();
        RuleFor(x => x.LocationDataElementId).NotEmpty();
        RuleFor(x => x.EventTypeDataElementId).NotEmpty();
        RuleFor(x => x.PhoneNumberDataElementId).NotEmpty();
        RuleFor(x => x.SuspectedDiseaseDataElementId).NotEmpty();
        RuleFor(x => x.DateOfOnsetDataElementId).NotEmpty();

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
        RuleFor(x => x.Gender).NotEmpty();
        RuleFor(x => x.Location).NotEmpty();
        RuleFor(x => x.EventDate).NotEmpty();
        RuleFor(x => x.EventType).NotEmpty();
        RuleFor(x => x.OrgUnit).NotEmpty();
        RuleFor(x => x.PhoneNumber).NotEmpty();
        RuleFor(x => x.SuspectedDisease).NotEmpty();
        RuleFor(x => x.DateOfOnset).NotEmpty();
    }
}
