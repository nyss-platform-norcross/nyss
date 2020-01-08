using System;
using System.Threading.Tasks;
using RX.Nyss.Web.Utils;
using Shouldly;
using Xunit;

namespace RX.Nyss.Web.Tests.Utils
{
    public class DateTimeProviderTests
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public static object[][] TestData =
        {
            new object[] { new DateTime(2019, 12, 28), 52 },
            new object[] { new DateTime(2019, 12, 29), 1 },
            new object[] { new DateTime(2019, 6, 29), 26},
            new object[] { new DateTime(2019, 10, 29), 44},
            new object[] { new DateTime(2018, 1, 1), 1},
            new object[] { new DateTime(2018, 9, 29), 39},
            new object[] { new DateTime(2020, 1, 1), 1}
        };


        public DateTimeProviderTests()
        {
            _dateTimeProvider = new DateTimeProvider();
        }

        [Theory, MemberData(nameof(TestData))]
        public async Task GetEpiWeek_ShouldReturnCorrectWeekNumber(DateTime date, int weekNumber)
        {
            // act
            var result = _dateTimeProvider.GetEpiWeek(date);

            // assert
            result.ShouldBe(weekNumber);
        }
    }
}
