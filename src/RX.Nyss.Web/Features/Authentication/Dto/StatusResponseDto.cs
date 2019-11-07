namespace RX.Nyss.Web.Features.Authentication.Dto
{
    public class StatusResponseDto
    {
        public bool IsAuthenticated { get; set; }

        public DataDto Data { get; set; }

        public class DataDto
        {
            public string Name { get; set; }
            
            public string Email { get; set; }

            public string[] Roles { get; set; }

            public string LanguageCode { get; set; }
        }
    }
}
