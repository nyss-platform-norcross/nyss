using FluentValidation;
using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Web.Features.Eidsr.Dto;

public class EidsrRequestDto
{
    public EidsrApiProperties EidsrApiProperties { get; set; }

    public string ProgramId { get; set; }
}

public class ValidatorEidsrRequestDtoValidator : AbstractValidator<EidsrRequestDto>
{
    public ValidatorEidsrRequestDtoValidator()
    {
        RuleFor(r => r.ProgramId).NotEmpty();
        RuleFor(r => r.EidsrApiProperties).NotEmpty();
        RuleFor(r => r.EidsrApiProperties.UserName).NotEmpty();
        RuleFor(r => r.EidsrApiProperties.Url).NotEmpty();
        RuleFor(r => r.EidsrApiProperties.Password).NotEmpty();
    }
}
