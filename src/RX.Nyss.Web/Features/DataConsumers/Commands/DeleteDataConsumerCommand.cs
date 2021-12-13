using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using MediatR;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Common.Utils.Logging;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.DataConsumers.Commands
{
    public class DeleteDataConsumerCommand : IRequest<Result>
    {
        public DeleteDataConsumerCommand(int id, int nationalSocietyId)
        {
            Id = id;
            NationalSocietyId = nationalSocietyId;
        }

        public int Id { get; }

        public int NationalSocietyId { get; }

        public class Handler : IRequestHandler<DeleteDataConsumerCommand, Result>
        {
            private readonly INyssContext _context;

            private readonly IIdentityUserRegistrationService _identityUserRegistrationService;

            private readonly INationalSocietyUserService _nationalSocietyUserService;

            private readonly IDeleteUserService _deleteService;

            private readonly ILoggerAdapter _loggerAdapter;

            public Handler(
                INyssContext context,
                ILoggerAdapter loggerAdapter,
                IIdentityUserRegistrationService identityUserRegistrationService,
                INationalSocietyUserService nationalSocietyUserService,
                IDeleteUserService deleteService)
            {
                _context = context;
                _loggerAdapter = loggerAdapter;
                _identityUserRegistrationService = identityUserRegistrationService;
                _nationalSocietyUserService = nationalSocietyUserService;
                _deleteService = deleteService;
            }

            public async Task<Result> Handle(DeleteDataConsumerCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    await _deleteService.EnsureCanDeleteUser(request.Id, Role.DataConsumer);

                    using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    var dataConsumerUser = await _nationalSocietyUserService
                        .GetNationalSocietyUserIncludingNationalSocieties<DataConsumerUser>(request.Id);
                    var userNationalSocieties = dataConsumerUser.UserNationalSocieties;

                    var nationalSocietyReferenceToRemove = userNationalSocieties
                        .SingleOrDefault(uns => uns.NationalSocietyId == request.NationalSocietyId);

                    if (nationalSocietyReferenceToRemove == null)
                    {
                        return Result.Error(ResultKey.User.Registration.UserIsNotAssignedToThisNationalSociety);
                    }

                    var isUsersLastNationalSociety = userNationalSocieties.Count == 1;

                    _context.UserNationalSocieties.Remove(nationalSocietyReferenceToRemove);

                    if (isUsersLastNationalSociety)
                    {
                        _nationalSocietyUserService.DeleteNationalSocietyUser(dataConsumerUser);

                        await _identityUserRegistrationService.DeleteIdentityUser(dataConsumerUser.IdentityUserId);
                    }


                    await _context.SaveChangesAsync(cancellationToken);

                    transactionScope.Complete();

                    return Result.Success();
                }
                catch (ResultException e)
                {
                    _loggerAdapter.Debug(e);
                    return e.Result;
                }
            }
        }
    }
}
