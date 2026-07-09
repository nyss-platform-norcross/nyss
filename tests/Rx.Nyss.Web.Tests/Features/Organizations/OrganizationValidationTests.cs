using NSubstitute;
using RX.Nyss.Web.Features.Organizations.Dto;
using RX.Nyss.Web.Features.Organizations.Validation;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.Organizations
{
    public class OrganizationValidationTests
    {
        private readonly OrganizationRequestDto.OrganizationValidator _organizationValidator;

        public OrganizationValidationTests()
        {
            var validationService = Substitute.For<IOrganizationValidationService>();
            validationService.NameExists(1, null, "ifrc").Returns(true);
            validationService.NameExists(1, 1, "ifrc").Returns(false);
            validationService.NameExists(1, null, "icrc").Returns(false);

            _organizationValidator = new OrganizationRequestDto.OrganizationValidator(validationService);
        }

        [Fact]
        public async void Create_WhenNameExists_ShouldHaveValidationError()
        {
            var result = await _organizationValidator.ValidateAsync(new OrganizationRequestDto
            {
                NationalSocietyId = 1,
                Name = "ifrc",
                Id = null
            });

        }

        [Fact]
        public async void Create_WhenNameDoesnotExist_ShouldNotHaveValidationError()
        {
            var result = await _organizationValidator.ValidateAsync(new OrganizationRequestDto
            {
                NationalSocietyId = 1,
                Name = "icrc",
                Id = null
            });
        }

        [Fact]
        public async void Edit_WhenNotChangingName_ShouldNotHaveValidationError()
        {
            var result = await _organizationValidator.ValidateAsync(new OrganizationRequestDto
            {
                NationalSocietyId = 1,
                Name = "ifrc",
                Id = 1
            });
        }
    }
}
