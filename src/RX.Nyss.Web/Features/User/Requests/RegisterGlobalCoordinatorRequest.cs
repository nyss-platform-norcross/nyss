namespace RX.Nyss.Web.Features.User.Requests
{
    public class RegisterGlobalCoordinatorRequest
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }

        public override string ToString() => $"{nameof(Email)}: {Email}, {nameof(Name)}: {Name}, {nameof(PhoneNumber)}: {PhoneNumber}";
    }
}
