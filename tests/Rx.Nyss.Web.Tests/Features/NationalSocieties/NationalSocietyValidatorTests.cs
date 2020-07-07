using FluentValidation.TestHelper;
using NSubstitute;
using NUnit.Framework;
using RX.Nyss.Web.Features.NationalSocieties.Dto;
using RX.Nyss.Web.Features.NationalSocieties.Validation;

namespace RX.Nyss.Web.Tests.Validators
{
    [TestFixture]
    public class CreateNationalSocietyValidatorTester
    {
        private CreateNationalSocietyRequestDto.Validator CreateValidator { get; set; }
        private EditNationalSocietyRequestDto.Validator EditValidator { get; set; }

        public CreateNationalSocietyValidatorTester()
        {
            var validationService = Substitute.For<INationalSocietyValidationService>();
            validationService.CountryExists(1).Returns(false);
            validationService.LanguageExists(1).Returns(false);
            validationService.NameExists("Test").Returns(true);
            CreateValidator = new CreateNationalSocietyRequestDto.Validator(validationService);
            EditValidator = new EditNationalSocietyRequestDto.Validator(validationService);
        }

        [Test]
        public void Create_WhenCountryDoesntExists_ShouldHaveError()
        {
            CreateValidator.ShouldHaveValidationErrorFor(ns => ns.CountryId, 1);
        }

        [Test]
        public void Create_WhenContentLanguageDoesntExists_ShouldHaveError()
        {
            CreateValidator.ShouldHaveValidationErrorFor(ns => ns.ContentLanguageId, 1);
        }

        [Test]
        public void Create_WhenNameExists_ShouldHaveError()
        {
            CreateValidator.ShouldHaveValidationErrorFor(ns => ns.Name, "Test");
        }

        [Test]
        public void Edit_WhenCountryDoesntExist_ShouldHaveError()
        {
            EditValidator.ShouldHaveValidationErrorFor(ns => ns.CountryId, 1);
        }

        [Test]
        public void Edit_WhenContentLanguageDoesntExist_ShouldHaveError()
        {
            EditValidator.ShouldHaveValidationErrorFor(ns => ns.ContentLanguageId, 1);
        }

        [Test]
        public void Edit_WhenNameExist_ShouldHaveError()
        {
            EditValidator.ShouldHaveValidationErrorFor(ns => ns.Name, "Test");
        }
    }
}