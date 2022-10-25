using System;
using System.Threading.Tasks;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Services.EidsrClient;
using RX.Nyss.Common.Services.EidsrClient.Dto;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data.Repositories;
using RX.Nyss.ReportApi.Configuration;
using EidsrApiProperties = RX.Nyss.Web.Services.EidsrClient.Dto.EidsrApiProperties;
using EidsrReport = RX.Nyss.ReportApi.Features.Reports.Models.EidsrReport;

namespace RX.Nyss.ReportApi.Features.Reports.Handlers;

public interface IEidsrReportHandler
{
    Task<bool> Handle(EidsrReport eidsrReport);
}

public class EidsrReportHandler : IEidsrReportHandler
{
    private readonly ILoggerAdapter _loggerAdapter;
    private readonly IEidsrClient _eidsrClient;
    private readonly INyssReportApiConfig _nyssReportApiConfig;
    private readonly ICryptographyService _cryptographyService;
    private readonly IEidsrRepository _eidsrRepository;

    public EidsrReportHandler(
        ILoggerAdapter loggerAdapter,
        IEidsrClient eidsrClient,
        INyssReportApiConfig nyssReportApiConfig,
        ICryptographyService cryptographyService,
        IEidsrRepository eidsrRepository)
    {
        _loggerAdapter = loggerAdapter;
        _eidsrClient = eidsrClient;
        _nyssReportApiConfig = nyssReportApiConfig;
        _cryptographyService = cryptographyService;
        _eidsrRepository = eidsrRepository;
    }

    public async Task<bool> Handle(EidsrReport eidsrReport)
    {
        try
        {
            if (eidsrReport == null || eidsrReport.ReportId == null)
            {
                throw new ArgumentException($"EidsrReport is null");
            }

            var report = _eidsrRepository.GetReportForEidsr(eidsrReport.ReportId.Value);

            var template = new EidsrRegisterEventRequestTemplate
            {
                EidsrApiProperties = new EidsrApiProperties
                {
                    Url = report.EidsrReportTemplate.EidsrApiProperties.Url,
                    UserName = report.EidsrReportTemplate.EidsrApiProperties.UserName,
                    Password = _cryptographyService.Decrypt(
                        report.EidsrReportTemplate.EidsrApiProperties.PasswordHash,
                        _nyssReportApiConfig.Key,
                        _nyssReportApiConfig.SupplementaryKey),
                },
                Program = report.EidsrReportTemplate.Program,
                LocationDataElementId = report.EidsrReportTemplate.LocationDataElementId,
                DateOfOnsetDataElementId = report.EidsrReportTemplate.DateOfOnsetDataElementId,
                PhoneNumberDataElementId = report.EidsrReportTemplate.PhoneNumberDataElementId,
                SuspectedDiseaseDataElementId = report.EidsrReportTemplate.SuspectedDiseaseDataElementId,
                EventTypeDataElementId = report.EidsrReportTemplate.EventTypeDataElementId,
                GenderDataElementId = report.EidsrReportTemplate.GenderDataElementId
            };

            var data = new EidsrRegisterEventRequestData
            {
                OrgUnit = report.EidsrReportData.OrgUnit,
                EventDate = report.EidsrReportData.EventDate,
                Location = report.EidsrReportData.Location,
                DateOfOnset = report.EidsrReportData.DateOfOnset,
                PhoneNumber = report.EidsrReportData.PhoneNumber,
                SuspectedDisease = report.EidsrReportData.SuspectedDisease,
                EventType = report.EidsrReportData.EventType,
                Gender = report.EidsrReportData.Gender,
            };

            var request = EidsrRegisterEventRequest.CreateEidsrRegisterEventRequest(template, data);

            var result = await _eidsrClient.RegisterEvent(request);

            return result.IsSuccess;
        }
        catch (Exception e)
        {
            _loggerAdapter.Error(e.Message);
        }

        return false;
    }
}



