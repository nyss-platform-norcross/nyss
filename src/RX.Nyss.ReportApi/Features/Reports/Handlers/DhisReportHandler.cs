using System;
using System.Threading.Tasks;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Services.DhisClient;
using RX.Nyss.Common.Services.DhisClient.Dto;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data.Repositories;
using RX.Nyss.ReportApi.Configuration;
using DhisReport = RX.Nyss.ReportApi.Features.Reports.Models.DhisReport;
using EidsrApiProperties = RX.Nyss.Web.Services.EidsrClient.Dto.EidsrApiProperties;

namespace RX.Nyss.ReportApi.Features.Reports.Handlers;

public interface IDhisReportHandler
{
    bool Handle(DhisReport dhisReport);
}

public class DhisReportHandler : IDhisReportHandler
{
    private readonly ILoggerAdapter _loggerAdapter;
    private readonly IDhisClient _dhisClient;
    private readonly INyssReportApiConfig _nyssReportApiConfig;
    private readonly ICryptographyService _cryptographyService;
    private readonly IDhisRepository _dhisRepository;

    public DhisReportHandler(
        ILoggerAdapter loggerAdapter,
        IDhisClient dhisClient,
        INyssReportApiConfig nyssReportApiConfig,
        ICryptographyService cryptographyService,
        IDhisRepository dhisRepository)
    {
        _loggerAdapter = loggerAdapter;
        _dhisClient = dhisClient;
        _nyssReportApiConfig = nyssReportApiConfig;
        _cryptographyService = cryptographyService;
        _dhisRepository = dhisRepository;
    }

    public bool Handle(DhisReport dhisReport)
    {
        var isSuccess = true;
        return isSuccess;
    }
}



