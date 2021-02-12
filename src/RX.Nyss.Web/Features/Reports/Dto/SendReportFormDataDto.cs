using System.Collections.Generic;
using RX.Nyss.Web.Features.Common.Dto;

namespace RX.Nyss.Web.Features.Reports.Dto
{
    public class SendReportFormDataDto
    {
        public int? CurrentUserModemId { get; set; }
        public List<GatewayModemResponseDto> Modems { get; set; }
    }
}
