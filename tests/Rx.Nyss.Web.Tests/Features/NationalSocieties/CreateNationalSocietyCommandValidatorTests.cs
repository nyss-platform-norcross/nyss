using NSubstitute;
//using RX.Nyss.Web.Features.NationalSocieties.Dto;
using RX.Nyss.Web.Features.NationalSocieties.Validation;
using Xunit;

namespace RX.Nyss.Web.Tests.Features.NationalSocieties
{
    public class CreateNationalSocietyCommandValidatorTests
    {
        //private CreateNationalSocietyCommand.Validator CreateValidator { get; set; }

        public CreateNationalSocietyCommandValidatorTests()
        {
            var validationService = Substitute.For<INationalSocietyValidationService>();
            validationService.CountryExists(1).Returns(false);
            validationService.LanguageExists(1).Returns(false);
            validationService.NameExists("Test").Returns(true);
            validationService.NameExistsToOther("Test", 1).Returns(true);
            //CreateValidator = new CreateNationalSocietyCommand.Validator(validationService);
        }

        [Fact]
        public void Create_WhenCountryDoesntExists_ShouldHaveError()
        {
            //CreateValidator.ShouldHaveValidationErrorFor(ns => ns.CountryId, 1);
            //CreateValidator.ShouldHaveChildValidator(ns => ns.CountryId, typeof(CreateNationalSocietyCommandValidatorTests));
        }

        [Fact]
        public void Create_WhenContentLanguageDoesntExists_ShouldHaveError()
        {
            //CreateValidator.ShouldHaveValidationErrorFor(ns => ns.ContentLanguageId, 1);
            //CreateValidator.ShouldHaveChildValidator(ns => ns.ContentLanguageId, typeof(CreateNationalSocietyCommandValidatorTests));
        }

        [Fact]
        public void Create_WhenNameExists_ShouldHaveError()
        {
            //CreateValidator.ShouldHaveValidationErrorFor(ns => ns.Name, "Test");
            //CreateValidator.ShouldHaveChildValidator(ns => ns.Name, typeof(CreateNationalSocietyCommandValidatorTests));
        }
    }
}
