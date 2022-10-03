using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Web.Features.Eidsr.Dto;

public class EidsrRequestDto
{
    public EidsrApiProperties EidsrApiProperties { get; set; }

    public string ProgramId { get; set; }
}
