using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using RX.Nyss.Data;
using RX.Nyss.Data.Concepts;
using RX.Nyss.Data.Models;
using RX.Nyss.TestData.TestDataGeneration;

namespace RX.Nyss.Web.Tests.Features.NationalSocieties.TestData
{
    public class BasicNationalSocietyServiceTestData
    {
        private readonly EntityNumerator _nationalSocietyNumerator;
        private readonly TestCaseDataProvider _testCaseDataProvider;

        public const string NationalSocietyName = "Norway";
        public const string ExistingNationalSocietyName = "Poland";
        public const int NationalSocietyId = 1;
        public const int CountryId = 1;
        public const int ContentLanguageId = 1;
        public const int ConsentId = 1;

        public BasicNationalSocietyServiceTestData(INyssContext nyssContext, EntityNumerator nationalSocietyNumerator)
        {
            _nationalSocietyNumerator = nationalSocietyNumerator;
            _testCaseDataProvider = new TestCaseDataProvider(nyssContext);
        }

        public TestCaseData Data =>
            _testCaseDataProvider.GetOrCreate(nameof(Data), (data) =>
            {
                data.Users = new List<User> { new ManagerUser { EmailAddress = "yo" } };

                data.NationalSocieties = new List<NationalSociety>
                {
                    new NationalSociety { Id = _nationalSocietyNumerator.Next, Name = ExistingNationalSocietyName, PendingHeadManager = data.Users[0] }
                };
                data.ContentLanguages = new List<ContentLanguage>
                {
                    new ContentLanguage { Id = ContentLanguageId }
                };
                data.Countries = new List<Country>
                {
                    new Country { Id = CountryId }
                };
                data.HeadManagerConsents = new List<HeadManagerConsent>
                {
                    new HeadManagerConsent
                    {
                        Id = ConsentId,
                        NationalSocietyId = NationalSocietyId
                    }
                };

                data.NyssContextMockedMethods = (nyssContext) =>
                {
                    nyssContext.NationalSocieties.FindAsync(NationalSocietyId).Returns(data.NationalSocieties[0]);
                    nyssContext.ContentLanguages.FindAsync(ContentLanguageId).Returns(data.ContentLanguages[0]);
                    nyssContext.Countries.FindAsync(CountryId).Returns(data.Countries[0]);
                };
            });
    }
}
