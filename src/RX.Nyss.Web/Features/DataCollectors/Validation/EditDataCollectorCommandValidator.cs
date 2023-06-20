using System.Linq;
using FluentValidation;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.DataCollectors.Commands;
using RX.Nyss.Web.Features.DataCollectors.Dto;
using RX.Nyss.Web.Utils.Extensions;

namespace RX.Nyss.Web.Features.DataCollectors.Validation
{
    public class EditDataCollectorCommandValidator : AbstractValidator<EditDataCollectorCommand>
    {
        public EditDataCollectorCommandValidator(IDataCollectorValidationService dataCollectorValidationService)
        {
            RuleFor(dc => dc.Id)
                .GreaterThan(0);

            RuleFor(dc => dc.DataCollectorType)
                .IsInEnum();

            RuleFor(dc => dc.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(dc => dc.PhoneNumber)
                .MaximumLength(20);

            RuleFor(dc => dc.PhoneNumber)
                .MustAsync(async (model, phoneNumber, t) => !await dataCollectorValidationService.PhoneNumberExistsToOther(model.Id, phoneNumber))
                .WithMessageKey(ResultKey.DataCollector.PhoneNumberAlreadyExists);

            RuleFor(dc => dc.AdditionalPhoneNumber)
                .MaximumLength(20);

            RuleFor(dc => dc.SupervisorId)
                .GreaterThan(0);

            RuleFor(dc => dc.SupervisorId)
                .MustAsync(async (_, supervisorId, _) => await dataCollectorValidationService.IsAllowedToCreateForSupervisor(supervisorId))
                .WithMessageKey(ResultKey.DataCollector.NotAllowedToSelectSupervisor);

            When(dc => dc.DataCollectorType == DataCollectorType.Human, () =>
            {
                RuleFor(dc => dc.DisplayName).NotEmpty().MaximumLength(100);
                RuleFor(dc => dc.Sex).IsInEnum();
            });

            RuleFor(dc => dc.Deployed)
                .NotNull();

            RuleFor(dc => dc.Locations)
                .NotNull()
                .Must(locations => locations.Any());

            RuleForEach(dc => dc.Locations)
                .SetValidator(x => new DataCollectorLocationRequestDto.Validator(x.Id, dataCollectorValidationService));

            RuleFor(dc => dc.LinkedToHeadSupervisor)
                .NotNull();
        }
    }
}
