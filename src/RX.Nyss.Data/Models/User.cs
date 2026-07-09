using System;
using System.Collections.Generic;
using RX.Nyss.Data.Concepts;

namespace RX.Nyss.Data.Models
{
    public abstract class User
    {
        public int Id { get; set; }

        public string? IdentityUserId { get; set; }

        public string? Name { get; set; }

        public Role Role { get; set; }

        public string? EmailAddress { get; set; }

        public string? PhoneNumber { get; set; }

        public string? AdditionalPhoneNumber { get; set; }

        public string? Organization { get; set; }

        public bool IsFirstLogin { get; set; } = true;

        public DateTime? DeletedAt { get; set; }

        public virtual ApplicationLanguage? ApplicationLanguage { get; set; }

        public virtual ICollection<UserNationalSociety>? UserNationalSocieties { get; set; }
    }
}
