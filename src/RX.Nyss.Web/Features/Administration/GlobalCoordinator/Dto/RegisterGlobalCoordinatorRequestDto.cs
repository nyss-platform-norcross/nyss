namespace RX.Nyss.Web.Features.Administration.GlobalCoordinator.Dto
{
    public class RegisterGlobalCoordinatorRequestDto
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }

        public override string ToString() => $"{nameof(Email)}: {Email}, {nameof(Name)}: {Name}, {nameof(PhoneNumber)}: {PhoneNumber}";
    }
}
