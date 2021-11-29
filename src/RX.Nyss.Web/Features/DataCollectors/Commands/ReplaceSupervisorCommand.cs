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

            private readonly IEmailToSMSService _emailToSMSService;

            private readonly ISmsPublisherService _smsPublisherService;

            private readonly ISmsTextGeneratorService _smsTextGeneratorService;

            public Handler(
                INyssContext nyssContext,
                IEmailToSMSService emailToSmsService,
                ISmsPublisherService smsPublisherService,
                ISmsTextGeneratorService smsTextGeneratorService)
            {
                _nyssContext = nyssContext;
                _emailToSMSService = emailToSmsService;
                _smsPublisherService = smsPublisherService;
                _smsTextGeneratorService = smsTextGeneratorService;
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

                await SendReplaceSupervisorSms(gatewaySetting, replaceSupervisorDatas, supervisorData.Supervisor);

                return Result.Success();
            }

            private async Task SendReplaceSupervisorSms(
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

                var recipientsSupervisors = replaceSupervisorDatas
                    .Where(r => r.DataCollector.PhoneNumber == null)
                    .Select(r => new SendSmsRecipient
                    {
                        PhoneNumber = r.Supervisor.PhoneNumber,
                        Modem = r.LastReport != null
                            ? r.LastReport.ModemNumber
                            : r.Supervisor.ModemId,
                    }).ToList();

                var messagesToSent = new List<(string Message, List<SendSmsRecipient> Recipients)>();

                if (recipientsDataCollectors.Any())
                {
                    var message = (await _smsTextGeneratorService.GenerateReplaceSupervisorSms(gatewaySetting.NationalSociety.ContentLanguage.LanguageCode))
                        .Replace("{{supervisorName}}", newSupervisor.Name)
                        .Replace("{{phoneNumber}}", newSupervisor.PhoneNumber);

                    messagesToSent.Add((message, recipientsDataCollectors));
                }

                if (recipientsSupervisors.Any())
                {
                    var messageToSupervisors = await _smsTextGeneratorService.GenerateReplaceSupervisorSmsForSupervisors(gatewaySetting.NationalSociety.ContentLanguage.LanguageCode);

                    messagesToSent.Add((messageToSupervisors, recipientsSupervisors));
                }

                var tasks = messagesToSent.Select(m =>
                {
                    var (message, recipients) = m;

                    return !string.IsNullOrEmpty(gatewaySetting.IotHubDeviceName)
                        ? _smsPublisherService.SendSms(gatewaySetting.IotHubDeviceName, recipients, message, gatewaySetting.Modems.Any())
                        : _emailToSMSService.SendMessage(gatewaySetting, recipients.Select(r => r.PhoneNumber).ToList(), message);
                });

                await Task.WhenAll(tasks);
            }
        }
    }
}
