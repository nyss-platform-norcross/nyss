using System;
using System.Linq;
using RX.Nyss.Common.Utils;
using Shouldly;
using Xunit;

namespace RX.Nyss.Common.Tests.Utils
{
    public class DateTimeProviderTests
    {
        public static object[][] TestData =
        {
            new object[] { new DateTime(2019, 12, 28), 52, 2019 },
            new object[] { new DateTime(2019, 12, 29), 1, 2020 },
            new object[] { new DateTime(2019, 6, 29), 26, 2019 },
            new object[] { new DateTime(2019, 10, 29), 44, 2019 },
            new object[] { new DateTime(2018, 1, 1), 1, 2018 },
            new object[] { new DateTime(2018, 9, 29), 39, 2018 },
            new object[] { new DateTime(2020, 1, 1), 1, 2020 }
        };

        private readonly IDateTimeProvider _dateTimeProvider;


        public DateTimeProviderTests()
        {
            _dateTimeProvider = new DateTimeProvider();
        }

        [Theory, MemberData(nameof(TestData))]
        public void GetEpiWeek_ShouldReturnCorrectWeekNumber(DateTime date, int weekNumber)
        {
            // act
            var result = _dateTimeProvider.GetEpiWeek(date);

            // assert
            result.ShouldBe(weekNumber);
        }

        [Theory, MemberData(nameof(TestData))]
        public void GetEpiDate_ShouldReturnCorrectEpiNumber(DateTime date, int weekNumber, int year)
        {
            // act
            var result = _dateTimeProvider.GetEpiDate(date);

            // assert
            result.EpiWeek.ShouldBe(weekNumber);
            result.EpiYear.ShouldBe(year);
        }

        [Fact]
        public void GetEpiWeeksRange_ShouldReturnCorrectEpiDatesRangeFromDifferentYears()
        {
            var startDate = new DateTime(2019, 12, 28);
            var endDate = new DateTime(2020, 1, 6);

            // act
            var result = _dateTimeProvider.GetEpiWeeksRange(startDate, endDate);

            // assert
            result.Count().ShouldBe(3);
            result.ElementAt(0).ShouldBe(new EpiDate(52, 2019));
            result.ElementAt(1).ShouldBe(new EpiDate(1, 2020));
            result.ElementAt(2).ShouldBe(new EpiDate(2, 2020));
        }


        [Fact]
        public void GetEpiWeeksRange_ShouldReturnCorrectEpiDatesRangeFromTheSameYear()
        {
            var startDate = new DateTime(2019, 12, 29);
            var endDate = new DateTime(2020, 1, 6);

            // act
            var result = _dateTimeProvider.GetEpiWeeksRange(startDate, endDate);

            // assert
            result.Count().ShouldBe(2);
            result.ElementAt(0).ShouldBe(new EpiDate(1, 2020));
            result.ElementAt(1).ShouldBe(new EpiDate(2, 2020));
        }
    }
}
