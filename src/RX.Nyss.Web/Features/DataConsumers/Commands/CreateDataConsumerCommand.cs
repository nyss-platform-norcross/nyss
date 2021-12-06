using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.DataConsumers.Commands
{
    public class CreateDataConsumerCommand : IRequest<Result>
    {
        public int NationalSocietyId { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        public string AdditionalPhoneNumber { get; set; }

        public string Organization { get; set; }

        public class Handler : IRequestHandler<CreateDataConsumerCommand, Result>
        {
            private readonly INyssContext _dataContext;

            private readonly IIdentityUserRegistrationService _identityUserRegistrationService;

            private readonly IVerificationEmailService _verificationEmailService;

            private readonly ILoggerAdapter _loggerAdapter;

            public Handler(
                INyssContext dataContext,
                IIdentityUserRegistrationService identityUserRegistrationService,
                IVerificationEmailService verificationEmailService,
                ILoggerAdapter loggerAdapter)
            {
                _dataContext = dataContext;
                _identityUserRegistrationService = identityUserRegistrationService;
                _verificationEmailService = verificationEmailService;
                _loggerAdapter = loggerAdapter;
            }

            public async Task<Result> Handle(CreateDataConsumerCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    string securityStamp;
                    DataConsumerUser user;
                    ICollection<Organization> organizations;

                    using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        var identityUser = await _identityUserRegistrationService.CreateIdentityUser(request.Email, Role.DataConsumer);
                        securityStamp = await _identityUserRegistrationService.GenerateEmailVerification(identityUser.Email);

                        (user, organizations) = await CreateDataConsumerUser(identityUser, request.NationalSocietyId, request);

                        transactionScope.Complete();
                    }

                    await _verificationEmailService.SendVerificationForDataConsumersEmail(user, string.Join(", ", organizations.Select(o => o.Name)), securityStamp);

                    return Result.Success(ResultKey.User.Registration.Success);
                }
                catch (ResultException e)
                {
                    _loggerAdapter.Debug(e);
                    return e.Result;
                }
            }

        private async Task<(DataConsumerUser user, ICollection<Organization> Organizations)> CreateDataConsumerUser(
            IdentityUser identityUser,
            int nationalSocietyId,
            CreateDataConsumerCommand request)
        {
            var nationalSociety = await _dataContext.NationalSocieties
                .Include(ns => ns.ContentLanguage)
                .Include(ns => ns.Organizations)
                .SingleOrDefaultAsync(ns => ns.Id == nationalSocietyId);

            if (nationalSociety == null)
            {
                throw new ResultException(ResultKey.User.Registration.NationalSocietyDoesNotExist);
            }

            if (nationalSociety.IsArchived)
            {
                throw new ResultException(ResultKey.User.Registration.CannotCreateUsersInArchivedNationalSociety);
            }

            var defaultUserApplicationLanguage = await _dataContext.ApplicationLanguages
                .SingleOrDefaultAsync(al => al.LanguageCode == nationalSociety.ContentLanguage.LanguageCode);

            var user = new DataConsumerUser
            {
                IdentityUserId = identityUser.Id,
                EmailAddress = identityUser.Email,
                Name = request.Name,
                PhoneNumber = request.PhoneNumber,
                AdditionalPhoneNumber = request.AdditionalPhoneNumber,
                Organization = request.Organization,
                ApplicationLanguage = defaultUserApplicationLanguage
            };

            var userNationalSociety = CreateUserNationalSocietyReference(nationalSociety, user);

            await _dataContext.AddAsync(userNationalSociety);
            await _dataContext.SaveChangesAsync();

            return (user, nationalSociety.Organizations);
        }

        private UserNationalSociety CreateUserNationalSocietyReference(NationalSociety nationalSociety, User user) =>
            new UserNationalSociety
            {
                NationalSociety = nationalSociety,
                User = user
            };
        }

        public class Validator : AbstractValidator<CreateDataConsumerCommand>
        {
            public Validator()
            {
                RuleFor(m => m.Name).NotEmpty().MaximumLength(100);
                RuleFor(m => m.Email).NotEmpty().MaximumLength(100).EmailAddress();
                RuleFor(m => m.PhoneNumber).NotEmpty().MaximumLength(20).PhoneNumber();
                RuleFor(m => m.AdditionalPhoneNumber).MaximumLength(20).PhoneNumber().Unless(r => string.IsNullOrEmpty(r.AdditionalPhoneNumber));
                RuleFor(m => m.Organization).NotEmpty().MaximumLength(100);
            }
        }
    }
}
