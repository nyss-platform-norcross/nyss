namespace RX.Nyss.Web.Features.Authentication.Dto
{
    public class StatusResponseDto
    {
        public bool IsAuthenticated { get; set; }
        public UserDataDto UserData { get; set; }


        public class UserDataDto
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Email { get; set; }

            public string[] Roles { get; set; }

            public string LanguageCode { get; set; }

            public HomePageDto HomePage { get; set; }
        }

        public class HomePageDto
        {
            public HomePageType Page { get; set; }
            public int? NationalSocietyId { get; set; }
            public int? ProjectId { get; set; }
        }
    }
}
