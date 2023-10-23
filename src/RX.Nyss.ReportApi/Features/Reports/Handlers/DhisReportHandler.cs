using System;
using System.Threading.Tasks;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Services.DhisClient;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data.Repositories;
using RX.Nyss.ReportApi.Configuration;
using EidsrApiProperties = RX.Nyss.Web.Services.EidsrClient.Dto.EidsrApiProperties;
using DhisReport = RX.Nyss.ReportApi.Features.Reports.Models.DhisReport;
using RX.Nyss.Common.Services.DhisClient.Dto;

namespace RX.Nyss.ReportApi.Features.Reports.Handlers;

public interface IDhisReportHandler
{
    Task<bool> Handle(DhisReport dhisReport);
}

public class DhisReportHandler : IDhisReportHandler
{
    private readonly ILoggerAdapter _loggerAdapter;
    private readonly IDhisClient _dhisClient;
    private readonly INyssReportApiConfig _nyssReportApiConfig;
    private readonly ICryptographyService _cryptographyService;
    private readonly IEidsrRepository _eidsrRepository;

    public DhisReportHandler(
        ILoggerAdapter loggerAdapter,
        IDhisClient dhisClient,
        INyssReportApiConfig nyssReportApiConfig,
        ICryptographyService cryptographyService,
        IEidsrRepository eidsrRepository)
    {
        _loggerAdapter = loggerAdapter;
        _dhisClient = dhisClient;
        _nyssReportApiConfig = nyssReportApiConfig;
        _cryptographyService = cryptographyService;
        _eidsrRepository = eidsrRepository;
    }

    public async Task<bool> Handle(DhisReport dhisReport)
    {
        var isSuccess = true;

        try
        {
            if (dhisReport?.ReportId == null)
            {
                throw new ArgumentException($"DhisReport is null");
            }

            var reports = _eidsrRepository.GetReportsForDhis(dhisReport.ReportId.Value);

            foreach (var report in reports)
            {
                var template = new DhisRegisterReportRequestTemplate
                {
                    EidsrApiProperties = new EidsrApiProperties
                    {
                        Url = report.DhisDbReportTemplate.EidsrApiProperties.Url,
                        UserName = report.DhisDbReportTemplate.EidsrApiProperties.UserName,
                        Password = _cryptographyService.Decrypt(
                            report.DhisDbReportTemplate.EidsrApiProperties.PasswordHash,
                            _nyssReportApiConfig.Key,
                            _nyssReportApiConfig.SupplementaryKey),
                    },
                    Program = report.DhisDbReportTemplate.Program,
                    ReportLocationDataElementId = report.DhisDbReportTemplate.ReportLocationDataElementId,
                    ReportHealthRiskDataElementId = report.DhisDbReportTemplate.ReportHealthRiskDataElementId,
                    ReportSuspectedDiseaseDataElementId = report.DhisDbReportTemplate.ReportSuspectedDiseaseDataElementId,
                    ReportStatusDataElementId = report.DhisDbReportTemplate.ReportStatusDataElementId,
                    ReportGenderDataElementId = report.DhisDbReportTemplate.ReportGenderDataElementId,
                    ReportAgeAtLeastFiveDataElementId = report.DhisDbReportTemplate.ReportAgeAtLeastFiveDataElementId,
                    ReportAgeBelowFiveDataElementId = report.DhisDbReportTemplate.ReportAgeBelowFiveDataElementId
                };

                var data = new DhisRegisterReportRequestData
                {
                    OrgUnit = report.DhisDbReportData.OrgUnit,
                    EventDate = report.DhisDbReportData.EventDate,
                    ReportLocation = report.DhisDbReportData.ReportLocation,
                    ReportHealthRisk = report.DhisDbReportData.ReportHealthRisk,
                    ReportSuspectedDisease = report.DhisDbReportData.ReportSuspectedDisease,
                    ReportStatus = report.DhisDbReportData.ReportStatus,
                    ReportGender = report.DhisDbReportData.ReportGender,
                    ReportAgeAtleastFive = report.DhisDbReportData.ReportAgeAtLeastFive,
                    ReportAgeBelowFive = report.DhisDbReportData.ReportAgeBelowFive,
                };

                var request = DhisRegisterReportRequest.CreateDhisRegisterReportRequest(template, data);

                var result = await _dhisClient.RegisterReport(request);

                if (!result.IsSuccess)
                {
                    isSuccess = false;
                }
            }
        }
        catch (Exception e)
        {
            _loggerAdapter.Error(e.Message);
            isSuccess = false;
        }

        return isSuccess;
    }
}



