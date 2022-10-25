using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Services.EidsrClient;
using RX.Nyss.Common.Services.EidsrClient.Dto;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.ReportApi.Configuration;
using RX.Nyss.ReportApi.Features.Reports.Models;
using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.ReportApi.Features.Reports.Handlers;

public interface IEidsrReportHandler
{
    Task<bool> Handle(EidsrReport eidsrReport);
}

public class EidsrReportHandler : IEidsrReportHandler
{
    private readonly INyssContext _nyssContext;
    private readonly ILoggerAdapter _loggerAdapter;
    private readonly IEidsrClient _eidsrClient;
    private readonly INyssReportApiConfig _nyssReportApiConfig;
    private readonly ICryptographyService _cryptographyService;

    public EidsrReportHandler(
        INyssContext nyssContext,
        ILoggerAdapter loggerAdapter,
        IEidsrClient eidsrClient,
        INyssReportApiConfig nyssReportApiConfig,
        ICryptographyService cryptographyService)
    {
        _nyssContext = nyssContext;
        _loggerAdapter = loggerAdapter;
        _eidsrClient = eidsrClient;
        _nyssReportApiConfig = nyssReportApiConfig;
        _cryptographyService = cryptographyService;
    }

    public async Task<bool> Handle(EidsrReport eidsrReport)
    {
        try
        {
            if (eidsrReport == null || eidsrReport.ReportId == null)
            {
                throw new ArgumentException($"EidsrReport is null");
            }

            var reportWithNationalSocietyAndConfig = _nyssContext.RawReports
                .Include(r => r.NationalSociety)
                .Include(r => r.Village)
                .Include(r => r.Report)
                .FirstOrDefault(x=>x.Id == eidsrReport.ReportId);

            if (reportWithNationalSocietyAndConfig == default)
            {
                throw new ArgumentException($"RawReport not found for Report {eidsrReport.ReportId}");
            }

            var ns = reportWithNationalSocietyAndConfig.NationalSociety;

            if (ns == default)
            {
                throw new ArgumentException($"NationalSociety not found for Report {eidsrReport.ReportId}");
            }

            var config = ns?.EidsrConfiguration;

            if (config == default)
            {
                throw new ArgumentException($"EidsrConfiguration not found for NationalSociety {ns.Id}");
            }

            var organizationUnit = _nyssContext.EidsrOrganisationUnits
                .FirstOrDefault(x => x.DistrictId == reportWithNationalSocietyAndConfig.Village.District.Id);

            if (organizationUnit == default)
            {
                throw new ArgumentException($"OrganizationUnit not found for District {reportWithNationalSocietyAndConfig.Village.District.Id}");
            }

            var template = new EidsrRegisterEventRequestTemplate
            {
                EidsrApiProperties = new EidsrApiProperties
                {
                    Url = config.ApiBaseUrl,
                    UserName = config.Username,
                    Password = _cryptographyService.Decrypt(
                        config.PasswordHash,
                        _nyssReportApiConfig.Key,
                        _nyssReportApiConfig.SupplementaryKey),
                },
                Program = config.TrackerProgramId,
                LocationDataElementId = config.LocationDataElementId,
                DateOfOnsetDataElementId = config.DateOfOnsetDataElementId,
                PhoneNumberDataElementId = config.PhoneNumberDataElementId,
                SuspectedDiseaseDataElementId = config.SuspectedDiseaseDataElementId,
                EventTypeDataElementId = config.EventTypeDataElementId,
                GenderDataElementId = config.GenderDataElementId
            };

            var data = new EidsrRegisterEventRequestData //TODO: verify how to fill that fields
            {
                OrgUnit = organizationUnit?.OrganisationUnitId,
                EventDate = reportWithNationalSocietyAndConfig?.Timestamp,
                Location = reportWithNationalSocietyAndConfig?.Report.Location.ToString(),
                DateOfOnset = reportWithNationalSocietyAndConfig?.Timestamp,
                PhoneNumber = reportWithNationalSocietyAndConfig?.Report.PhoneNumber,
                SuspectedDisease = "some disease",
                EventType = "some type",
                Gender = "Male",
            };

            var request = EidsrRegisterEventRequest.CreateEidsrRegisterEventRequest(template, data);

            // TODO: make a request
            // _eidsrClient.RegisterEvent(request);

            return true;
        }
        catch (Exception e)
        {
            _loggerAdapter.Error(e.Message);
        }

        return false;
    }
}



