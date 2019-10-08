namespace RX.Nyss.Data.Models
{
    public class User
    {
        public int Id { get; set; }

        public string IdentityUserId { get; set; }

        public string Name { get; set; }

        public string Role { get; set; }

        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string AdditionalPhoneNumber { get; set; }

        public string Organization  { get; set; }

        public bool IsFirstLogin { get; set; }
        
        public ApplicationLanguage ApplicationLanguage { get; set; }
    }
}
