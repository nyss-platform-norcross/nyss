using FluentValidation.TestHelper;
using NSubstitute;
using RX.Nyss.Web.Features.NationalSocieties.Commands;
using RX.Nyss.Web.Features.NationalSocieties.Validation;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.NationalSocieties
{
    public class EditNationalSocietyCommandValidatorTests
    {
        private readonly INationalSocietyValidationService _mockValidationService;

        private readonly EditNationalSocietyCommand.Validator _validator;

        public EditNationalSocietyCommandValidatorTests()
        {
            _mockValidationService = Substitute.For<INationalSocietyValidationService>();

            _mockValidationService.CountryExists(1).Returns(false);
            _mockValidationService.LanguageExists(1).Returns(false);
            _mockValidationService.NameExists("Test").Returns(true);
            _mockValidationService.NameExistsToOther("Test", 1).Returns(true);

            _validator = new EditNationalSocietyCommand.Validator(_mockValidationService);
        }

        [Fact]
        public void Edit_WhenCountryDoesntExist_ShouldHaveError()
        {
            _validator.ShouldHaveValidationErrorFor(ns => ns.CountryId, 1);
        }

        [Fact]
        public void Edit_WhenContentLanguageDoesntExist_ShouldHaveError()
        {
            _validator.ShouldHaveValidationErrorFor(ns => ns.ContentLanguageId, 1);
        }

        [Fact]
        public void Edit_WhenNameExist_ShouldHaveError()
        {
            var result = _validator.TestValidate(new EditNationalSocietyCommand.RequestBody
            {
                Name = "Test",
                Id = 1
            });

            result.ShouldHaveValidationErrorFor(x => x.Name);
        }
    }
}
