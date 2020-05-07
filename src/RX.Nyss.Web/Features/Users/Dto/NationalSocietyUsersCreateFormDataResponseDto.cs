using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Web.Features.Users.Dto
{
    public class NationalSocietyUsersCreateFormDataResponseDto
    {
        public List<ListOpenProjectsResponseDto> OpenProjects { get; set; }

        public List<OrganizationsDto> Organizations { get; set; }

        public bool HasCoordinator { get; set; }

        public bool IsHeadManager { get; set; }
    }

    public class NationalSocietyUsersEditFormDataResponseDto
    {
        public string Email { get; set; }

        public Role Role { get; set; }

        public List<ListOpenProjectsResponseDto> OpenProjects { get; set; }

        public List<OrganizationsDto> Organizations { get; set; }
    }

    public class OrganizationsDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class ListOpenProjectsResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
