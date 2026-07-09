using System.Collections.Generic;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Web.Features.Common.Dto;
using RX.Nyss.Web.Features.ProjectAlertRecipients.Dto;

namespace RX.Nyss.Web.Features.Users.Dto
{
    public class NationalSocietyUsersCreateFormDataResponseDto
    {
        public List<ListOpenProjectsResponseDto> Projects { get; set; } = new List<ListOpenProjectsResponseDto>();

        public List<OrganizationsDto> Organizations { get; set; } = new List<OrganizationsDto>();

        public List<HeadSupervisorResponseDto> HeadSupervisors { get; set; } = new List<HeadSupervisorResponseDto>();

        public List<GatewayModemResponseDto> Modems { get; set; } = new List<GatewayModemResponseDto>();

        public bool HasCoordinator { get; set; }

        public bool IsHeadManager { get; set; }
        public string CountryCode { get; set; } = string.Empty;
    }

    public class NationalSocietyUsersEditFormDataResponseDto
    {
        //public string Email { get; set; }
        public string CountryCode { get; set; } = string.Empty;

        public Role Role { get; set; }

        public List<ListOpenProjectsResponseDto> Projects { get; set; } = new List<ListOpenProjectsResponseDto>();

        public List<OrganizationsDto> Organizations { get; set; } = new List<OrganizationsDto>();

        public List<GatewayModemResponseDto> Modems { get; set; } = new List<GatewayModemResponseDto>();
    }

    public class OrganizationsDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsDefaultOrganization { get; set; }

        public bool HasHeadManager { get; set; }
    }

    public class ListOpenProjectsResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<ProjectAlertRecipientResponseDto> AlertRecipients { get; set; } = new List<ProjectAlertRecipientResponseDto>();
    }

    public class HeadSupervisorResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
