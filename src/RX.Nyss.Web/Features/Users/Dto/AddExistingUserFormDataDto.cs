using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Users.Dto
{
    public class AddExistingUserFormDataDto
    {
        public IReadOnlyList<OrganizationsDto> Organizations { get; set; } = new List<OrganizationsDto>();

        public IReadOnlyList<GatewayModemResponseDto> Modems { get; set; } = new List<GatewayModemResponseDto>();
    }
}
