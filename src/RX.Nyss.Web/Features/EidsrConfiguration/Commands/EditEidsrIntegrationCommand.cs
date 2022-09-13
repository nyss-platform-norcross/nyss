using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;

namespace RX.Nyss.Web.Features.EidsrConfiguration.Commands;

public class EditEidsrIntegrationCommand : IRequest<Result>
{
    public EditEidsrIntegrationCommand(int id, RequestBody body)
    {
        Id = id;
        Body = body;
    }

    public int Id { get; }

    public RequestBody Body { get; }

    public class Handler : IRequestHandler<EditEidsrIntegrationCommand, Result>
    {
        private readonly INyssContext _nyssContext;

        public Handler(INyssContext nyssContext)
        {
            _nyssContext = nyssContext;
        }

        public async Task<Result> Handle(EditEidsrIntegrationCommand request, CancellationToken cancellationToken)
        {
            // does request concern existing national society
            var nationalSociety = await _nyssContext.NationalSocieties.FirstOrDefaultAsync(x =>
                x.Id == request.Id, cancellationToken: cancellationToken);

            if (nationalSociety == default)
            {
                return Result.Error(ResultKey.EidsrIntegration.Edit.ErrorNationalSocietyDoesNotExists);
            }

            // Is eidsrIntegration enabled
            if (nationalSociety.EnableEidsrIntegration == false)
            {
                return Result.Error(ResultKey.EidsrIntegration.Edit.ErrorEidsrIntegrationDisabled);
            }

            // get eidsrConfiguration for editing
            var eidsrConfiguration = await _nyssContext.EidsrConfiguration
                .FirstOrDefaultAsync(x =>
                    x.NationalSocietyId == request.Id, cancellationToken: cancellationToken);

            // if EIDSR configuration does not exists, create one
            if (eidsrConfiguration == default)
            {
                var newEidsrConfiguration = new Nyss.Data.Models.EidsrConfiguration

                {
                    Username = request.Body.Username,
                    PasswordHash = request.Body.Password, // TODO: it doesn't look like hash, you know?
                    ApiBaseUrl = request.Body.ApiBaseUrl,
                    TrackerProgramId = request.Body.TrackerProgramId,
                    LocationDataElementId = request.Body.LocationDataElementId,
                    DateOfOnsetDataElementId = request.Body.DateOfOnsetDataElementId,
                    PhoneNumberDataElementId = request.Body.PhoneNumberDataElementId,
                    SuspectedDiseaseDataElementId = request.Body.SuspectedDiseaseDataElementId,
                    EventTypeDataElementId = request.Body.EventTypeDataElementId,
                    GenderDataElementId = request.Body.GenderDataElementId,
                    NationalSocietyId = request.Id,
                };

                await _nyssContext.AddAsync(newEidsrConfiguration, cancellationToken);

                var result = await _nyssContext.SaveChangesAsync(cancellationToken);

                return result <= 0
                    ? Result.Error(ResultKey.SqlExceptions.GeneralError)
                    : Result.SuccessMessage(ResultKey.EidsrIntegration.Edit.Success);
            }
            // if EIDSR configuration exists, update it
            else
            {
                eidsrConfiguration.Username = request.Body.Username;
                eidsrConfiguration.PasswordHash = request.Body.Password;
                eidsrConfiguration.ApiBaseUrl = request.Body.ApiBaseUrl;
                eidsrConfiguration.TrackerProgramId = request.Body.TrackerProgramId;
                eidsrConfiguration.LocationDataElementId = request.Body.LocationDataElementId;
                eidsrConfiguration.DateOfOnsetDataElementId = request.Body.DateOfOnsetDataElementId;
                eidsrConfiguration.PhoneNumberDataElementId = request.Body.PhoneNumberDataElementId;
                eidsrConfiguration.SuspectedDiseaseDataElementId = request.Body.SuspectedDiseaseDataElementId;
                eidsrConfiguration.EventTypeDataElementId = request.Body.EventTypeDataElementId;
                eidsrConfiguration.GenderDataElementId = request.Body.GenderDataElementId;

                await _nyssContext.SaveChangesAsync(cancellationToken);
            }

            return Result.SuccessMessage(ResultKey.EidsrIntegration.Edit.Success);
        }
    }

    public class RequestBody
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public string ApiBaseUrl { get; set; }

        public string TrackerProgramId { get; set; }

        public string LocationDataElementId	{ get; set; }

        public string DateOfOnsetDataElementId { get; set; }

        public string PhoneNumberDataElementId { get; set; }

        public string SuspectedDiseaseDataElementId	{ get; set; }

        public string EventTypeDataElementId { get; set; }

        public string GenderDataElementId { get; set; }
    }

    public class Validator : AbstractValidator<RequestBody>
    {
        public Validator()
        {
            RuleFor(r => r.Username).NotEmpty();
            RuleFor(r => r.Password).NotEmpty();
            RuleFor(r => r.ApiBaseUrl).NotEmpty();
            RuleFor(r => r.TrackerProgramId).NotEmpty();
            RuleFor(r => r.LocationDataElementId).NotEmpty();
            RuleFor(r => r.DateOfOnsetDataElementId).NotEmpty();
            RuleFor(r => r.PhoneNumberDataElementId).NotEmpty();
            RuleFor(r => r.SuspectedDiseaseDataElementId).NotEmpty();
            RuleFor(r => r.EventTypeDataElementId).NotEmpty();
            RuleFor(r => r.GenderDataElementId).NotEmpty();
        }
    }
}
