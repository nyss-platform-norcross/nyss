using System.Collections.Generic;

namespace RX.Nyss.Web.Features.ProjectOrganizations.Dto
{
    public class ProjectOrganizationCreateDataResponseDto
    {
        public IEnumerable<CreateProjectOrganizationDto> Organizations { get; set; }

        public class CreateProjectOrganizationDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
