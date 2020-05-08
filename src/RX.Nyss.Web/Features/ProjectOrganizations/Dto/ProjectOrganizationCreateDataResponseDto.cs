using System.Collections.Generic;

namespace RX.Nyss.Web.Features.ProjectOrganizations.Dto
{
    public class ProjectOrganizationCreateDataResponseDto
    {
        public IEnumerable<OrganizationDto> Organizations { get; set; }

        public class OrganizationDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
