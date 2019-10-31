using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Authentication.Policies.BaseAccessHandlers;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class DataManagerAccessRequirement : IAuthorizationRequirement
    {
    }

    public class DataManagerAccessHandler : UserAccessHandler<DataManagerUser, DataManagerAccessRequirement>
    {
        private const string RouteParameterName = "dataManagerId";

        public DataManagerAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService)
            : base(httpContextAccessor, nationalSocietyAccessService, RouteParameterName)
        {
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DataManagerAccessRequirement requirement) =>
            HandleUserResourceRequirement(context, requirement);
    }
}
