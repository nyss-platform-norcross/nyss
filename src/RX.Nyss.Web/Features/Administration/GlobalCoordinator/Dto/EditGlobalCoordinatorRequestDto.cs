namespace RX.Nyss.Web.Features.Administration.GlobalCoordinator.Dto
{
    public class EditGlobalCoordinatorRequestDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Organization { get; set; }

        public override string ToString() => $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(PhoneNumber)}: {PhoneNumber}, {nameof(Organization)}: {Organization}";
    }
}
