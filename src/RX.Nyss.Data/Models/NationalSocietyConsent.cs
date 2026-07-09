using System;

namespace RX.Nyss.Data.Models
{
    public class NationalSocietyConsent
    {
        public int Id { get; set; }

        public int NationalSocietyId { get; set; }

        public string UserEmailAddress { get; set; }

        public string UserPhoneNumber { get; set; }

        public DateTime ConsentedFrom { get; set; }

        public DateTime? ConsentedUntil { get; set; }

        public string ConsentDocument { get; set; }
    }
}
