using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataCollectors.DataContracts;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.DataCollectors.Commands
{
    public class ReplaceSupervisorCommand : IRequest<Result>
    {
        public int SupervisorId { get; set; }

        public IEnumerable<int> DataCollectorIds { get; set; }

        public class Handler : IRequestHandler<ReplaceSupervisorCommand, Result>
        {
            private readonly INyssContext _nyssContext;

            private readonly ISmsPublisherService _smsPublisherService;

            private readonly ISmsTextGeneratorService _smsTextGeneratorService;

            private readonly IEmailPublisherService _emailPublisherService;

            private readonly IEmailTextGeneratorService _emailTextGeneratorService;

            public Handler(
                INyssContext nyssContext,
                ISmsPublisherService smsPublisherService,
                ISmsTextGeneratorService smsTextGeneratorService,
                IEmailPublisherService emailPublisherService,
                IEmailTextGeneratorService emailTextGeneratorService)
            {
                _nyssContext = nyssContext;
                _smsPublisherService = smsPublisherService;
                _smsTextGeneratorService = smsTextGeneratorService;
                _emailPublisherService = emailPublisherService;
                _emailTextGeneratorService = emailTextGeneratorService;
            }

            public async Task<Result> Handle(ReplaceSupervisorCommand request, CancellationToken cancellationToken)
            {
                var replaceSupervisorDatas = await _nyssContext.DataCollectors
                    .Where(dc => request.DataCollectorIds.Contains(dc.Id))
                    .Select(dc => new ReplaceSupervisorData
                    {
                        DataCollector = dc,
                        Supervisor = dc.Supervisor,
                        LastReport = dc.RawReports.FirstOrDefault(r => r.ModemNumber.HasValue)
                    })
                    .ToListAsync(cancellationToken);

                var supervisorData = await _nyssContext.Users
                    .Select(u => new
                    {
                        Supervisor = (SupervisorUser)u,
                        NationalSociety = u.UserNationalSocieties.Select(uns => uns.NationalSociety).Single()
                    })
                    .FirstOrDefaultAsync(u => u.Supervisor.Id == request.SupervisorId, cancellationToken);

                var gatewaySetting = await _nyssContext.GatewaySettings
                    .Include(gs => gs.Modems)
                    .Include(gs => gs.NationalSociety)
                    .ThenInclude(ns => ns.ContentLanguage)
                    .FirstOrDefaultAsync(gs => gs.NationalSociety == supervisorData.NationalSociety, cancellationToken);

                foreach (var dc in replaceSupervisorDatas)
                {
                    dc.DataCollector.Supervisor = supervisorData.Supervisor;
                }

                await _nyssContext.SaveChangesAsync(cancellationToken);

                await SendReplaceSupervisorNotification(gatewaySetting, replaceSupervisorDatas, supervisorData.Supervisor);

                return Result.Success();
            }

            private async Task SendReplaceSupervisorNotification(
                GatewaySetting gatewaySetting,
                List<ReplaceSupervisorData> replaceSupervisorDatas,
                SupervisorUser newSupervisor)
            {
                var recipientsDataCollectors = replaceSupervisorDatas
                    .Where(r => r.DataCollector.PhoneNumber != null)
                    .Select(r => new SendSmsRecipient
                    {
                        PhoneNumber = r.DataCollector.PhoneNumber,
                        Modem = r.LastReport != null
                            ? r.LastReport.ModemNumber
                            : r.Supervisor.ModemId,
                    }).ToList();

                // Send SMS to data collectors with phone number
                if (recipientsDataCollectors.Any())
                {
                    var message = (await _smsTextGeneratorService.GenerateReplaceSupervisorSms(gatewaySetting.NationalSociety.ContentLanguage.LanguageCode))
                        .Replace("{{supervisorName}}", newSupervisor.Name)
                        .Replace("{{phoneNumber}}", newSupervisor.PhoneNumber);

                    await _smsPublisherService.SendSms(
                        gatewaySetting.IotHubDeviceName,
                        recipientsDataCollectors,
                        message);
                }

                var dataCollectors = replaceSupervisorDatas.Select(d => d.DataCollector.DisplayName);
                var dataCollectorsWithoutPhoneNumber = replaceSupervisorDatas
                    .Where(d => string.IsNullOrWhiteSpace(d.DataCollector.PhoneNumber))
                    .Select(d => d.DataCollector.DisplayName);

                // Send email to supervisor
                var (subject, body) = await _emailTextGeneratorService.GenerateReplacedSupervisorEmail(
                    gatewaySetting.NationalSociety.ContentLanguage.LanguageCode,
                    newSupervisor.Name,
                    dataCollectors,
                    dataCollectorsWithoutPhoneNumber);

                // Send email notification to supervisor
                await _emailPublisherService.SendEmail(
                    (newSupervisor.EmailAddress, newSupervisor.Name),
                    subject,
                    body);
            }
        }
    }
}
