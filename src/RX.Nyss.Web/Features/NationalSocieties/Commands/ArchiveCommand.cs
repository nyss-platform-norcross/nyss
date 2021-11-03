using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RX.Nyss.Common.Utils.DataContract;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Managers;
using RX.Nyss.Web.Features.SmsGateways;
using RX.Nyss.Web.Features.TechnicalAdvisors;
using static RX.Nyss.Common.Utils.DataContract.Result;

namespace RX.Nyss.Web.Features.NationalSocieties.Commands
{
    public class ArchiveCommand : IRequest<Result>
    {
        public ArchiveCommand(int id)
        {
            Id = id;
        }

        public int Id { get; }

        public class Handler : IRequestHandler<ArchiveCommand, Result>
        {
            private readonly INyssContext _nyssContext;

            private readonly IManagerService _managerService;

            private readonly ITechnicalAdvisorService _technicalAdvisorService;

            private readonly ISmsGatewayService _smsGatewayService;

            public Handler(
                INyssContext nyssContext,
                IManagerService managerService,
                ITechnicalAdvisorService technicalAdvisorService,
                ISmsGatewayService smsGatewayService)
            {
                _nyssContext = nyssContext;
                _managerService = managerService;
                _technicalAdvisorService = technicalAdvisorService;
                _smsGatewayService = smsGatewayService;
            }

            public async Task<Result> Handle(ArchiveCommand request, CancellationToken cancellationToken)
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var openedProjectsQuery = _nyssContext.Projects.Where(p => p.State == ProjectState.Open);
                var nationalSocietyData = await _nyssContext.NationalSocieties
                    .Where(ns => ns.Id == request.Id)
                    .Where(ns => !ns.IsArchived)
                    .Select(ns => new
                    {
                        NationalSociety = ns,
                        HasRegisteredUsers = ns.NationalSocietyUsers
                            .Any(uns => uns.UserId != ns.DefaultOrganization.HeadManager.Id),
                        HasOpenedProjects = openedProjectsQuery.Any(p => p.NationalSocietyId == ns.Id),
                        HeadManagerId = ns.DefaultOrganization.HeadManager != null
                            ? (int?)ns.DefaultOrganization.HeadManager.Id
                            : null,
                        HeadManagerRole = ns.DefaultOrganization.HeadManager != null
                            ? (Role?)ns.DefaultOrganization.HeadManager.Role
                            : null
                    })
                    .SingleAsync(cancellationToken);

                if (nationalSocietyData.HasOpenedProjects)
                {
                    return Error(ResultKey.NationalSociety.Archive.ErrorHasOpenedProjects);
                }

                if (nationalSocietyData.HasRegisteredUsers)
                {
                    return Error(ResultKey.NationalSociety.Archive.ErrorHasRegisteredUsers);
                }

                if (nationalSocietyData.HeadManagerId.HasValue)
                {
                    await DeleteHeadManager(nationalSocietyData.NationalSociety.Id, nationalSocietyData.HeadManagerId.Value, nationalSocietyData.HeadManagerRole.Value);
                }

                var removeResult = await RemoveApiKeys(nationalSocietyData.NationalSociety.Id);

                if (!removeResult.IsSuccess)
                {
                    return removeResult;
                }

                nationalSocietyData.NationalSociety.IsArchived = true;

                await _nyssContext.SaveChangesAsync(cancellationToken);

                transactionScope.Complete();

                return SuccessMessage(ResultKey.NationalSociety.Archive.Success);
            }

            private async Task<Result> RemoveApiKeys(int nationalSocietyId)
            {
                var gatewaysResult = await _smsGatewayService.List(nationalSocietyId);

                foreach (var gateway in gatewaysResult.Value)
                {
                    var deleteResult = await _smsGatewayService.Delete(gateway.Id);
                    if (!deleteResult.IsSuccess)
                    {
                        return deleteResult;
                    }
                }

                return Success();
            }

            private async Task DeleteHeadManager(int nationalSocietyId, int headManagerId, Role headManagerRole)
            {
                if (headManagerRole == Role.Manager)
                {
                    await _managerService.DeleteIncludingHeadManagerFlag(headManagerId);
                }
                else if (headManagerRole == Role.TechnicalAdvisor)
                {
                    await _technicalAdvisorService.DeleteIncludingHeadManagerFlag(nationalSocietyId, headManagerId);
                }
            }
        }
    }
}
