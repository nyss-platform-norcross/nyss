namespace RX.Nyss.Web.Features.Projects.Dto
{
    public class ProjectBasicDataResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public NationalSocietyIdDto NationalSociety { get; set; }
        public bool IsClosed { get; set; }
        public bool AllowMultipleOrganizations { get; set; }

        public class NationalSocietyIdDto
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string CountryName { get; set; }
        }
    }
}
