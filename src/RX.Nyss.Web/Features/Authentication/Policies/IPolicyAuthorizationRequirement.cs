using Microsoft.AspNetCore.Authorization;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public interface IPolicyAuthorizationRequirement: IAuthorizationRequirement
    {
        Policy Policy { get; }
    }
}
