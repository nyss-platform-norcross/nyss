using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Authentication.Policies.BaseAccessHandlers;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class DataConsumerAccessRequirement : IAuthorizationRequirement
    {
    }

    public class DataConsumerAccessHandler : UserAccessHandler<DataConsumerUser, DataConsumerAccessRequirement>
    {
        private const string RouteParameterName = "dataConsumerId";

        public DataConsumerAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService)
            :base(httpContextAccessor, nationalSocietyAccessService, RouteParameterName)
        {
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DataConsumerAccessRequirement requirement) => 
            HandleUserResourceRequirement(context, requirement);
    }

}
