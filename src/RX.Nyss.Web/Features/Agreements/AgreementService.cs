using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Services;
using RX.Nyss.Common.Utils;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Data.Queries;
using RX.Nyss.Web.Configuration;
using RX.Nyss.Web.Features.Agreements.Dto;
using RX.Nyss.Web.Services;
using RX.Nyss.Web.Services.Authorization;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.Agreements
{
    public interface IAgreementService
    {
        Task<Result<PendingConsentDto>> GetPendingAgreementDocuments();
        Task<Result> AcceptAgreement(string languageCode);
        Task<Result<AgreementsStatusesDto>> GetPendingAgreements();
    }

    public class AgreementService : IAgreementService
    {
        private readonly INyssWebConfig _config;
        private readonly IAuthorizationService _authorizationService;
        private readonly INyssContext _nyssContext;
        private readonly IGeneralBlobProvider _generalBlobProvider;
        private readonly IDataBlobService _dataBlobService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailPublisherService _emailPublisherService;
        private readonly IEmailTextGeneratorService _emailTextGeneratorService;

        public AgreementService(IAuthorizationService authorizationService, INyssContext nyssContext, IGeneralBlobProvider generalBlobProvider, IDataBlobService dataBlobService,
            IDateTimeProvider dateTimeProvider, IEmailPublisherService emailPublisherService, INyssWebConfig config, IEmailTextGeneratorService emailTextGeneratorService)
        {
            _authorizationService = authorizationService;
            _nyssContext = nyssContext;
            _generalBlobProvider = generalBlobProvider;
            _dataBlobService = dataBlobService;
            _dateTimeProvider = dateTimeProvider;
            _emailPublisherService = emailPublisherService;
            _config = config;
            _emailTextGeneratorService = emailTextGeneratorService;
        }

        public async Task<Result> AcceptAgreement(string languageCode)
        {
            var identityUserName = _authorizationService.GetCurrentUserName();

            var user = await _nyssContext.Users.FilterAvailable()
                .Include(u => u.ApplicationLanguage)
                .SingleOrDefaultAsync(u => u.EmailAddress == identityUserName);

            if (user == null)
            {
                return Error(ResultKey.User.Common.UserNotFound);
            }

            var (pendingSocieties, staleSocieties) = await GetPendingAndStaleNationalSocieties(user);
            var utcNow = _dateTimeProvider.UtcNow;

            var consentDocumentFileName = Guid.NewGuid() + ".pdf";
            var sourceUri = await _generalBlobProvider.GetPlatformAgreementUrl(languageCode);
            await _dataBlobService.StorePlatformAgreement(sourceUri, consentDocumentFileName);

            foreach (var nationalSociety in staleSocieties.Union(pendingSocieties))
            {
                var existingConsent = await _nyssContext.NationalSocietyConsents
                    .Where(consent => consent.NationalSocietyId == nationalSociety.Id && consent.UserEmailAddress == user.EmailAddress && !consent.ConsentedUntil.HasValue).SingleOrDefaultAsync();
                if (existingConsent != null)
                {
                    existingConsent.ConsentedUntil = utcNow;
                }

                if (user.Role == Role.Manager || user.Role == Role.TechnicalAdvisor)
                {
                    var ns = await _nyssContext.NationalSocieties
                        .Include(society => society.DefaultOrganization)
                        .ThenInclude(o => o.HeadManager)
                        .Where(society => society.Id == nationalSociety.Id &&
                            society.DefaultOrganization.NationalSocietyUsers.Any(nsu => nsu.UserId == user.Id)).FirstOrDefaultAsync();

                    if (ns != null)
                    {
                        var oldHeadManagerConsent = ns.DefaultOrganization.HeadManager != null ? await _nyssContext.NationalSocietyConsents
                            .Where(consent => consent.NationalSocietyId == nationalSociety.Id &&
                                consent.UserEmailAddress == ns.DefaultOrganization.HeadManager.EmailAddress && !consent.ConsentedUntil.HasValue)
                            .SingleOrDefaultAsync() : null;

                        if (oldHeadManagerConsent != null)
                        {
                            oldHeadManagerConsent.ConsentedUntil = utcNow;
                        }

                        ns.DefaultOrganization.PendingHeadManager = null;
                        ns.DefaultOrganization.HeadManager = user;
                    }
                }

                await _nyssContext.NationalSocietyConsents.AddAsync(new NationalSocietyConsent
                {
                    ConsentedFrom = utcNow,
                    NationalSocietyId = nationalSociety.Id,
                    UserEmailAddress = user.EmailAddress,
                    UserPhoneNumber = user.PhoneNumber,
                    ConsentDocument = consentDocumentFileName
                });
            }

            await _nyssContext.SaveChangesAsync();

            await SendAgreementDocumentToUser(user, languageCode);

            return Success();
        }

        public async Task<Result<AgreementsStatusesDto>> GetPendingAgreements()
        {
            if (!_authorizationService.IsCurrentUserInAnyRole(Role.GlobalCoordinator, Role.Manager, Role.TechnicalAdvisor, Role.Coordinator))
            {
                return Success(new AgreementsStatusesDto
                {
                    StaleSocieties = new List<string>(),
                    PendingSocieties = new List<string>()
                });
            }

            var identityUserName = _authorizationService.GetCurrentUserName();

            var userEntity = await _nyssContext.Users.FilterAvailable()
                .Include(x => x.ApplicationLanguage)
                .SingleOrDefaultAsync(u => u.EmailAddress == identityUserName);

            var (pending, stale) = await GetPendingAndStaleNationalSocieties(userEntity);
            return Success(new AgreementsStatusesDto
            {
                PendingSocieties = pending.Select(p => p.Name),
                StaleSocieties = stale.Select(p => p.Name)
            });
        }

        public async Task<Result<PendingConsentDto>> GetPendingAgreementDocuments()
        {
            var identityUserName = _authorizationService.GetCurrentUserName();

            var userEntity = await _nyssContext.Users.FilterAvailable()
                .Include(x => x.ApplicationLanguage)
                .SingleOrDefaultAsync(u => u.EmailAddress == identityUserName);

            if (userEntity == null)
            {
                return Error<PendingConsentDto>(ResultKey.User.Common.UserNotFound);
            }

            var (pending, stale) = await GetPendingAndStaleNationalSocieties(userEntity);

            if (!pending.Union(stale).Any())
            {
                return Error<PendingConsentDto>(ResultKey.Consent.NoPendingConsent);
            }

            var docs = await GetApplicationLanguagesWithAgreementDocument();

            var pendingSociety = new PendingConsentDto
            {
                AgreementDocuments = docs,
                PendingSocieties = pending.Select(ns => new PendingNationalSocietyConsentDto
                {
                    NationalSocietyName = ns.Name,
                    NationalSocietyId = ns.Id
                }).ToList(),
                StaleSocieties = stale.Select(ns => new PendingNationalSocietyConsentDto
                {
                    NationalSocietyName = ns.Name,
                    NationalSocietyId = ns.Id
                }).ToList()
            };

            return Success(pendingSociety);
        }

        private async Task<(List<NationalSociety> pendingSocieties, List<NationalSociety> staleSocieties)> GetPendingAndStaleNationalSocieties(User userEntity)
        {
            var applicableNationalSocieties = _nyssContext.NationalSocieties.Where(x =>
                (_authorizationService.IsCurrentUserInRole(Role.Coordinator) && x.NationalSocietyUsers.Any(y => y.UserId == userEntity.Id)) ||
                (_authorizationService.IsCurrentUserInAnyRole(Role.Manager, Role.TechnicalAdvisor) &&
                    (x.DefaultOrganization.HeadManager == userEntity || x.DefaultOrganization.PendingHeadManager == userEntity)));

            var activeAgreements = _nyssContext.NationalSocietyConsents.Where(nsc => !nsc.ConsentedUntil.HasValue && nsc.UserEmailAddress == userEntity.EmailAddress);
            var pendingNationalSocieties = await applicableNationalSocieties.Where(ns => activeAgreements.All(aa => aa.NationalSocietyId != ns.Id)).ToListAsync();

            var staleNationalSocieties = new List<NationalSociety>();
            if (activeAgreements.Any())
            {
                var agreementLastUpdatedTimeStamp = await _generalBlobProvider.GetPlatformAgreementLastModifiedDate(userEntity.ApplicationLanguage.LanguageCode);
                var staleAgreements = activeAgreements.Where(nsc => nsc.ConsentedFrom < agreementLastUpdatedTimeStamp);
                staleNationalSocieties.AddRange(await applicableNationalSocieties.Where(ns => staleAgreements.Any(sa => sa.NationalSocietyId == ns.Id)).ToListAsync());
            }

            return (pendingNationalSocieties, staleNationalSocieties);
        }

        private async Task SendAgreementDocumentToUser(User user, string languageCode)
        {
            var (subject, body) = await _emailTextGeneratorService.GenerateAgreementDocumentEmail(languageCode);
            await _emailPublisherService.SendEmailWithAttachment((user.EmailAddress, user.Name), subject, body, _config.PlatformAgreementBlobObjectName.Replace("{languageCode}", languageCode));
        }

        private async Task<IEnumerable<AgreementDocument>> GetApplicationLanguagesWithAgreementDocument()
        {
            var applicationLanguages = await _nyssContext.ApplicationLanguages.ToListAsync();
            var applicationLanguagesWithAgreementDocument = new List<AgreementDocument>();
            foreach (var apl in applicationLanguages)
            {
                var agreementDocumentUrl = await _generalBlobProvider.GetPlatformAgreementUrl(apl.LanguageCode.ToLower());
                applicationLanguagesWithAgreementDocument.Add(new AgreementDocument
                {
                    Language = apl.DisplayName,
                    LanguageCode = apl.LanguageCode,
                    AgreementDocumentUrl = agreementDocumentUrl
                });
            }

            return applicationLanguagesWithAgreementDocument
                .Where(d => d.AgreementDocumentUrl != null);
        }
    }
}
