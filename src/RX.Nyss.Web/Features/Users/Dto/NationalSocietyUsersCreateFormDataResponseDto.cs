using System.Collections.Generic;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.ProjectAlertRecipients.Dto;

namespace RX.Nyss.Web.Features.Users.Dto
{
    public class NationalSocietyUsersCreateFormDataResponseDto
    {
        public List<ListOpenProjectsResponseDto> Projects { get; set; }

        public List<OrganizationsDto> Organizations { get; set; }

        public List<HeadSupervisorResponseDto> HeadSupervisors { get; set; }

        public List<GatewayModemResponseDto> Modems { get; set; }

        public bool HasCoordinator { get; set; }

        public bool IsHeadManager { get; set; }
        public string CountryCode { get; set; }
    }

    public class NationalSocietyUsersEditFormDataResponseDto
    {
        public string Email { get; set; }
        public string CountryCode { get; set; }

        public Role Role { get; set; }

        public List<ListOpenProjectsResponseDto> Projects { get; set; }

        public List<OrganizationsDto> Organizations { get; set; }

        public List<GatewayModemResponseDto> Modems { get; set; }
    }

    public class OrganizationsDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsDefaultOrganization { get; set; }

        public bool HasHeadManager { get; set; }
    }

    public class ListOpenProjectsResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ProjectAlertRecipientResponseDto> AlertRecipients { get; set; }
    }

    public class HeadSupervisorResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
