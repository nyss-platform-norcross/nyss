using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.EidsrConfiguration.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.EidsrClient.Dto;

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
        private readonly ICryptographyService _cryptographyService;
        private readonly IDistrictsOrgUnitsService _districtsOrgUnitsService;
        private readonly INyssWebConfig _nyssWebConfig;

        public Handler(
            INyssContext nyssContext,
            ICryptographyService cryptographyService,
            IDistrictsOrgUnitsService districtsOrgUnitsService,
            INyssWebConfig nyssWebConfig)
        {
            _nyssContext = nyssContext;
            _cryptographyService = cryptographyService;
            _districtsOrgUnitsService = districtsOrgUnitsService;
            _nyssWebConfig = nyssWebConfig;
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

            var passwordHash = _cryptographyService.Encrypt(
                request.Body.Password,
                _nyssWebConfig.Key,
                _nyssWebConfig.SupplementaryKey);

            if (eidsrConfiguration == default)
            {
                return await CreateNewEidsrConfiguration(request: request, cancellationToken: cancellationToken, passwordHash: passwordHash);
            }
            else
            {
                await UpdateEidsrConfiguration(request: request, cancellationToken: cancellationToken, eidsrConfiguration: eidsrConfiguration, passwordHash: passwordHash);
            }

            return Result.SuccessMessage(ResultKey.EidsrIntegration.Edit.Success);
        }

        private async Task<Result> CreateNewEidsrConfiguration(EditEidsrIntegrationCommand request, CancellationToken cancellationToken, string passwordHash)
        {
            var newEidsrConfiguration = new Nyss.Data.Models.EidsrConfiguration
            {
                Username = request.Body.Username,
                PasswordHash = passwordHash,
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

            await UpdateEidsrOrganisationUnits(request.Id, request.Body.DistrictsWithOrganizationUnits, cancellationToken);

            var result = await _nyssContext.SaveChangesAsync(cancellationToken);

            return result <= 0
                ? Result.Error(ResultKey.SqlExceptions.GeneralError)
                : Result.SuccessMessage(ResultKey.EidsrIntegration.Edit.Success);
        }

        private async Task UpdateEidsrConfiguration(EditEidsrIntegrationCommand request, CancellationToken cancellationToken, Nyss.Data.Models.EidsrConfiguration eidsrConfiguration, string passwordHash)
        {
            eidsrConfiguration.Username = request.Body.Username;
            eidsrConfiguration.PasswordHash = passwordHash;
            eidsrConfiguration.ApiBaseUrl = request.Body.ApiBaseUrl;
            eidsrConfiguration.TrackerProgramId = request.Body.TrackerProgramId;
            eidsrConfiguration.LocationDataElementId = request.Body.LocationDataElementId;
            eidsrConfiguration.DateOfOnsetDataElementId = request.Body.DateOfOnsetDataElementId;
            eidsrConfiguration.PhoneNumberDataElementId = request.Body.PhoneNumberDataElementId;
            eidsrConfiguration.SuspectedDiseaseDataElementId = request.Body.SuspectedDiseaseDataElementId;
            eidsrConfiguration.EventTypeDataElementId = request.Body.EventTypeDataElementId;
            eidsrConfiguration.GenderDataElementId = request.Body.GenderDataElementId;

            await UpdateEidsrOrganisationUnits(request.Id, request.Body.DistrictsWithOrganizationUnits, cancellationToken);

            await _nyssContext.SaveChangesAsync(cancellationToken);
        }

        private async Task UpdateEidsrOrganisationUnits(
            int nationalSocietyId,
            List<DistrictsWithOrganizationUnits> newDistrictsWithOrganizationUnits,
            CancellationToken cancellationToken)
        {
            var nationalSocietyDistricts = await _districtsOrgUnitsService.GetNationalSocietyDistricts(nationalSocietyId);

            var dbEidsrOrganisationUnits = await _nyssContext.EidsrOrganisationUnits.Where(eidsrOrganisationUnit =>
                nationalSocietyDistricts.Select(x=>x.Id).Contains(eidsrOrganisationUnit.DistrictId)
            ).ToListAsync(cancellationToken: cancellationToken);

            foreach (var nationalSocietyDistrict in nationalSocietyDistricts)
            {
                var reqEidsrOrganisationUnit = newDistrictsWithOrganizationUnits
                    .FirstOrDefault(x => x.DistrictId == nationalSocietyDistrict.Id);

                if(reqEidsrOrganisationUnit == default)
                    continue;

                var dbEidsrOrganisationUnit = dbEidsrOrganisationUnits
                    .FirstOrDefault(x => x.DistrictId == nationalSocietyDistrict.Id);

                if (dbEidsrOrganisationUnit == default)
                {
                    await _nyssContext.AddAsync(new EidsrOrganisationUnits
                    {
                        DistrictId = nationalSocietyDistrict.Id,
                        OrganisationUnitId = reqEidsrOrganisationUnit.OrganisationUnitId,
                        OrganisationUnitName = reqEidsrOrganisationUnit.OrganisationUnitName,
                    }, cancellationToken);
                }
                else
                {
                    dbEidsrOrganisationUnit.OrganisationUnitId = reqEidsrOrganisationUnit.OrganisationUnitId;
                    dbEidsrOrganisationUnit.OrganisationUnitName = reqEidsrOrganisationUnit.OrganisationUnitName;
                }
            }
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

        public List<DistrictsWithOrganizationUnits> DistrictsWithOrganizationUnits { get; set; }
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
            RuleFor(r => r.DistrictsWithOrganizationUnits).Must(districtsWithOrganizationUnits =>
            {
                foreach (var districtWithOrganizationUnit in districtsWithOrganizationUnits)
                {
                    if (string.IsNullOrEmpty(districtWithOrganizationUnit.DistrictName))
                        return false;

                    if (string.IsNullOrEmpty(districtWithOrganizationUnit.OrganisationUnitName))
                        return false;
                }

                return true;
            });
        }
    }
}
