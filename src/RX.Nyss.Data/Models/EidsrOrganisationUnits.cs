using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RX.Nyss.Data.Models;

public class EidsrOrganisationUnits
{
    [Key]
    [ForeignKey("District")]
    public int DistrictId { get; set; }

    public virtual District District { get; set; }

    [Column(TypeName = "varchar(256)")]
    public string OrganisationUnitId { get; set; }

    public string OrganisationUnitName { get; set; }
}