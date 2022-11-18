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
                    Url = report.EidsrDbReportTemplate.EidsrApiProperties.Url,
                    UserName = report.EidsrDbReportTemplate.EidsrApiProperties.UserName,
                    Password = _cryptographyService.Decrypt(
                        report.EidsrDbReportTemplate.EidsrApiProperties.PasswordHash,
                        _nyssReportApiConfig.Key,
                        _nyssReportApiConfig.SupplementaryKey),
                },
                Program = report.EidsrDbReportTemplate.Program,
                LocationDataElementId = report.EidsrDbReportTemplate.LocationDataElementId,
                DateOfOnsetDataElementId = report.EidsrDbReportTemplate.DateOfOnsetDataElementId,
                PhoneNumberDataElementId = report.EidsrDbReportTemplate.PhoneNumberDataElementId,
                SuspectedDiseaseDataElementId = report.EidsrDbReportTemplate.SuspectedDiseaseDataElementId,
                EventTypeDataElementId = report.EidsrDbReportTemplate.EventTypeDataElementId,
                GenderDataElementId = report.EidsrDbReportTemplate.GenderDataElementId
            };

            var data = new EidsrRegisterEventRequestData
            {
                OrgUnit = report.EidsrDbReportData.OrgUnit,
                EventDate = report.EidsrDbReportData.EventDate,
                Location = report.EidsrDbReportData.Location,
                DateOfOnset = report.EidsrDbReportData.DateOfOnset,
                PhoneNumber = report.EidsrDbReportData.PhoneNumber,
                SuspectedDisease = report.EidsrDbReportData.SuspectedDisease,
                EventType = report.EidsrDbReportData.EventType,
                Gender = report.EidsrDbReportData.Gender,
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



