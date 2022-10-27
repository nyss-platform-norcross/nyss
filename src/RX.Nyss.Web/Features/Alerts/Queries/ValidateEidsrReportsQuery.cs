using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Repositories;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Alerts.Queries;

public class ValidateEidsrReportsQuery : IRequest<Result<ValidationResult>>
{
    public ValidateEidsrReportsQuery(int alertId)
    {
        AlertId = alertId;
    }

    private int AlertId { get; }

    public class Handler : IRequestHandler<ValidateEidsrReportsQuery, Result>
    {
        private readonly IEidsrRepository _repository;

        public Handler(IEidsrRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> Handle(ValidateEidsrReportsQuery request, CancellationToken cancellationToken)
        {
            var reports = _repository.GetReportsForEidsr(request.AlertId);

            var result = await new EidsrDbReportsValidator().ValidateAsync(reports, cancellationToken);

            return Success(result);
        }
    }
}

public class EidsrDbReportsValidator : AbstractValidator<List<EidsrDbReport>>
{
    public EidsrDbReportsValidator()
    {
        RuleForEach(x => x).SetValidator(new EidsrDbReportValidator());
    }
}

public class EidsrDbReportValidator : AbstractValidator<EidsrDbReport>
{
    public EidsrDbReportValidator()
    {
        RuleFor(er => er.EidsrDbReportTemplate).NotEmpty().DependentRules(() =>
        {
            RuleFor(x => x.EidsrDbReportTemplate.Program).NotEmpty();
            RuleFor(x => x.EidsrDbReportTemplate.GenderDataElementId).NotEmpty();
            RuleFor(x => x.EidsrDbReportTemplate.LocationDataElementId).NotEmpty();
            RuleFor(x => x.EidsrDbReportTemplate.EventTypeDataElementId).NotEmpty();
            RuleFor(x => x.EidsrDbReportTemplate.PhoneNumberDataElementId).NotEmpty();
            RuleFor(x => x.EidsrDbReportTemplate.SuspectedDiseaseDataElementId).NotEmpty();
            RuleFor(x => x.EidsrDbReportTemplate.DateOfOnsetDataElementId).NotEmpty();

            RuleFor(x => x.EidsrDbReportTemplate.EidsrApiProperties).NotEmpty().DependentRules(() =>
            {
                RuleFor(x => x.EidsrDbReportTemplate.EidsrApiProperties.Url).NotEmpty();
                RuleFor(x => x.EidsrDbReportTemplate.EidsrApiProperties.PasswordHash).NotEmpty();
                RuleFor(x => x.EidsrDbReportTemplate.EidsrApiProperties.UserName).NotEmpty();
            });
        });

        RuleFor(er => er.EidsrDbReportData).NotEmpty().DependentRules(() =>
        {
            RuleFor(x => x.EidsrDbReportData.Gender).NotEmpty();
            RuleFor(x => x.EidsrDbReportData.Location).NotEmpty();
            RuleFor(x => x.EidsrDbReportData.EventDate).NotEmpty();
            RuleFor(x => x.EidsrDbReportData.EventType).NotEmpty();
            RuleFor(x => x.EidsrDbReportData.OrgUnit).NotEmpty();
            RuleFor(x => x.EidsrDbReportData.PhoneNumber).NotEmpty();
            RuleFor(x => x.EidsrDbReportData.SuspectedDisease).NotEmpty();
            RuleFor(x => x.EidsrDbReportData.DateOfOnset).NotEmpty();
        });
    }
}
