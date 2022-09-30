using RX.Nyss.Web.Services.EidsrClient.Dto;

namespace RX.Nyss.Web.Features.Eidsr.Dto;

public abstract class EidsrRequestDto
{
    public EidsrApiProperties EidsrApiProperties { get; set; }
}
