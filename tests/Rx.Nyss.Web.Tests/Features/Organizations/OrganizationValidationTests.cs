using FluentValidation.TestHelper;
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
        public void Create_WhenNameExists_ShouldHaveValidationError()
        {
            var result = _organizationValidator.TestValidate(new OrganizationRequestDto
            {
                NationalSocietyId = 1,
                Name = "ifrc",
                Id = null
            });

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Create_WhenNameDoesnotExist_ShouldNotHaveValidationError()
        {
            var result = _organizationValidator.TestValidate(new OrganizationRequestDto
            {
                NationalSocietyId = 1,
                Name = "icrc",
                Id = null
            });

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Edit_WhenNotChangingName_ShouldNotHaveValidationError()
        {
            var result = _organizationValidator.TestValidate(new OrganizationRequestDto
            {
                NationalSocietyId = 1,
                Name = "ifrc",
                Id = 1
            });

            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }
    }
}
