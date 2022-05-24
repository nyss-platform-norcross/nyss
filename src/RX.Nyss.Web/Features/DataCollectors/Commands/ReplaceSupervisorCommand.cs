using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.DataCollectors.DataContracts;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.DataCollectors.Commands
{
    public class ReplaceSupervisorCommand : IRequest<Result>
    {
        public int SupervisorId { get; set; }
        public Role SupervisorRole { get; set; }
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
                var dataCollectorsWithSupervisorData = await _nyssContext.DataCollectors
                    .Where(dc => request.DataCollectorIds.Contains(dc.Id))
                    .Select(dc => new ReplaceSupervisorData
                    {
                        DataCollector = dc,
                        Supervisor = dc.Supervisor,
                        HeadSupervisor = dc.HeadSupervisor,
                        LastReport = dc.RawReports.FirstOrDefault(r => r.ModemNumber.HasValue)
                    })
                    .ToListAsync(cancellationToken);

                var notificationData = request.SupervisorRole switch
                {
                    Role.Supervisor => await ReplaceWithSupervisor(request.SupervisorId, dataCollectorsWithSupervisorData, cancellationToken),
                    Role.HeadSupervisor => await ReplaceWithHeadSupervisor(request.SupervisorId, dataCollectorsWithSupervisorData, cancellationToken),
                    _ => throw new ResultException(ResultKey.DataCollector.ReplaceSupervisor.RoleNotCompatible)
                };

                await _nyssContext.SaveChangesAsync(cancellationToken);

                await SendReplaceSupervisorNotification(notificationData.GatewaySetting, dataCollectorsWithSupervisorData, notificationData.Name, notificationData.PhoneNumber, notificationData.EmailAddress);

                return Result.Success();
            }

            private async Task SendReplaceSupervisorNotification(
                GatewaySetting gatewaySetting,
                List<ReplaceSupervisorData> dataCollectorsWithSupervisorData,
                string newSupervisorName,
                string newSupervisorPhoneNumber,
                string newSupervisorEmailAddress)
            {
                var recipientsDataCollectors = dataCollectorsWithSupervisorData
                    .Where(dc => dc.DataCollector.PhoneNumber != null)
                    .Select(dc => new SendSmsRecipient
                    {
                        PhoneNumber = dc.DataCollector.PhoneNumber,
                        Modem = dc.LastReport != null
                            ? dc.LastReport.ModemNumber
                            : dc.Supervisor != null ? dc.Supervisor.ModemId : dc.HeadSupervisor.ModemId,
                    }).ToList();

                // Send SMS to data collectors with phone number
                if (recipientsDataCollectors.Any())
                {
                    var message = (await _smsTextGeneratorService.GenerateReplaceSupervisorSms(gatewaySetting.NationalSociety.ContentLanguage.LanguageCode))
                        .Replace("{{supervisorName}}", newSupervisorName)
                        .Replace("{{phoneNumber}}", newSupervisorPhoneNumber);


                    if (string.IsNullOrEmpty(gatewaySetting.IotHubDeviceName))
                    {
                        await _emailPublisherService.SendSmsAsEmail((gatewaySetting.EmailAddress, gatewaySetting.Name), recipientsDataCollectors, message);
                    }
                    else
                    {
                        await _smsPublisherService.SendSms(
                            gatewaySetting.IotHubDeviceName,
                            recipientsDataCollectors,
                            message);
                    }
                }

                var dataCollectors = dataCollectorsWithSupervisorData.Select(d => d.DataCollector.DisplayName);
                var dataCollectorsWithoutPhoneNumber = dataCollectorsWithSupervisorData
                    .Where(d => string.IsNullOrWhiteSpace(d.DataCollector.PhoneNumber))
                    .Select(d => d.DataCollector.DisplayName);

                // Send email to supervisor
                var (subject, body) = await _emailTextGeneratorService.GenerateReplacedSupervisorEmail(
                    gatewaySetting.NationalSociety.ContentLanguage.LanguageCode,
                    newSupervisorName,
                    dataCollectors,
                    dataCollectorsWithoutPhoneNumber);

                // Send email notification to supervisor
                await _emailPublisherService.SendEmail(
                    (newSupervisorEmailAddress, newSupervisorName),
                    subject,
                    body);
            }

            private async Task<ReplaceSupervisorNotificationData> ReplaceWithSupervisor(int supervisorId, List<ReplaceSupervisorData> dataCollectorsWithSupervisorData, CancellationToken cancellationToken)
            {
                var supervisorData = await _nyssContext.Users
                    .Select(u => new
                    {
                        Supervisor = (SupervisorUser)u,
                        NationalSociety = u.UserNationalSocieties.Select(uns => uns.NationalSociety).Single()
                    })
                    .SingleAsync(u => u.Supervisor.Id == supervisorId, cancellationToken);

                foreach (var dc in dataCollectorsWithSupervisorData)
                {
                    var linkedToHeadSupervisor = dc.DataCollector.HeadSupervisor != null;
                    dc.DataCollector.Supervisor = supervisorData.Supervisor;

                    if (linkedToHeadSupervisor)
                    {
                        dc.DataCollector.HeadSupervisor = null;
                    }
                }

                var gatewaySetting = await _nyssContext.GatewaySettings
                    .Include(gs => gs.Modems)
                    .Include(gs => gs.NationalSociety)
                    .ThenInclude(ns => ns.ContentLanguage)
                    .SingleAsync(gs => gs.NationalSociety == supervisorData.NationalSociety, cancellationToken);

                return new ReplaceSupervisorNotificationData
                {
                    Name = supervisorData.Supervisor.Name,
                    PhoneNumber = supervisorData.Supervisor.PhoneNumber,
                    EmailAddress = supervisorData.Supervisor.EmailAddress,
                    GatewaySetting = gatewaySetting
                };
            }

            private async Task<ReplaceSupervisorNotificationData> ReplaceWithHeadSupervisor(int supervisorId, List<ReplaceSupervisorData> dataCollectorsWithSupervisorData, CancellationToken cancellationToken)
            {
                var supervisorData = await _nyssContext.Users
                    .Select(u => new
                    {
                        HeadSupervisor = (HeadSupervisorUser)u,
                        NationalSociety = u.UserNationalSocieties.Select(uns => uns.NationalSociety).Single()
                    })
                    .SingleAsync(u => u.HeadSupervisor.Id == supervisorId, cancellationToken);

                foreach (var dc in dataCollectorsWithSupervisorData)
                {
                    var linkedToSupervisor = dc.DataCollector.Supervisor != null;
                    dc.DataCollector.HeadSupervisor = supervisorData.HeadSupervisor;

                    if (linkedToSupervisor)
                    {
                        dc.DataCollector.Supervisor = null;
                    }
                }

                var gatewaySetting = await _nyssContext.GatewaySettings
                    .Include(gs => gs.Modems)
                    .Include(gs => gs.NationalSociety)
                    .ThenInclude(ns => ns.ContentLanguage)
                    .SingleAsync(gs => gs.NationalSociety == supervisorData.NationalSociety, cancellationToken);

                return new ReplaceSupervisorNotificationData
                {
                    Name = supervisorData.HeadSupervisor.Name,
                    PhoneNumber = supervisorData.HeadSupervisor.PhoneNumber,
                    EmailAddress = supervisorData.HeadSupervisor.EmailAddress,
                    GatewaySetting = gatewaySetting
                };
            }
        }
    }
}
