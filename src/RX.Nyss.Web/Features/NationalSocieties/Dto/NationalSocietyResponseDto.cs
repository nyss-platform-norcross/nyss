using System;

namespace RX.Nyss.Web.Features.NationalSocieties.Dto
{
    public class NationalSocietyResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int CountryId { get; set; }

        public string CountryName { get; set; } = string.Empty;

        public int ContentLanguageId { get; set; }

        public string ContentLanguageName { get; set; } = "en";

        public bool IsCurrentUserHeadManager { get; set; }

        public bool IsArchived { get; set; }

        public bool HasCoordinator { get; set; }

        public DayOfWeek EpiWeekStartDay { get; set; }

        public bool EnableEidsrIntegration { get; set; }
    }
}
