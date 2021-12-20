using System;
using System.Globalization;
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
            new object[] { new DateTime(2019, 12, 28), 52, 2019, DayOfWeek.Sunday },
            new object[] { new DateTime(2019, 12, 29), 1, 2020, DayOfWeek.Sunday },
            new object[] { new DateTime(2019, 6, 29), 26, 2019, DayOfWeek.Sunday },
            new object[] { new DateTime(2019, 10, 29), 44, 2019, DayOfWeek.Sunday },
            new object[] { new DateTime(2018, 1, 1), 1, 2018, DayOfWeek.Sunday },
            new object[] { new DateTime(2018, 9, 29), 39, 2018, DayOfWeek.Sunday },
            new object[] { new DateTime(2020, 1, 1), 1, 2020, DayOfWeek.Sunday },
            new object[] { new DateTime(2021, 1, 3), 1, 2021, DayOfWeek.Sunday },
            new object[] { new DateTime(2021, 1, 3), 53, 2020, DayOfWeek.Monday }
        };

        public static object[][] EpiWeekToFirstDateTestData =
        {
            new object[] { 1, 2020, new DateTime(2019, 12, 29), DayOfWeek.Sunday },
            new object[] { 15, 2020, new DateTime(2020, 4, 5), DayOfWeek.Sunday },
            new object[] { 40, 2020, new DateTime(2020, 9, 27), DayOfWeek.Sunday },
            new object[] { 53, 2020, new DateTime(2020, 12, 27), DayOfWeek.Sunday },
            new object[] { 1, 2021, new DateTime(2021, 1, 3), DayOfWeek.Sunday },
            new object[] { 18, 2021, new DateTime(2021, 5, 2), DayOfWeek.Sunday },
            new object[] { 27, 2021, new DateTime(2021, 7, 4), DayOfWeek.Sunday },
            new object[] { 1, 2021, new DateTime(2021, 1, 4), DayOfWeek.Monday },
            new object[] { 22, 2021, new DateTime(2021, 5, 31), DayOfWeek.Monday }
        };

        private readonly IDateTimeProvider _dateTimeProvider;


        public DateTimeProviderTests()
        {
            _dateTimeProvider = new DateTimeProvider();
        }

        [Theory, MemberData(nameof(TestData))]
        public void GetEpiWeek_ShouldReturnCorrectWeekNumber(DateTime date, int weekNumber, int year, DayOfWeek epiWeekStartDay)
        {
            // act
            var result = _dateTimeProvider.GetEpiWeek(date, epiWeekStartDay);

            // assert
            result.ShouldBe(weekNumber);
        }

        [Theory, MemberData(nameof(TestData))]
        public void GetEpiDate_ShouldReturnCorrectEpiNumber(DateTime date, int weekNumber, int year, DayOfWeek epiWeekStartDay)
        {
            // act
            var result = _dateTimeProvider.GetEpiDate(date, epiWeekStartDay);
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
            var result = _dateTimeProvider.GetEpiDateRange(startDate, endDate, DayOfWeek.Sunday);

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
            var result = _dateTimeProvider.GetEpiDateRange(startDate, endDate, DayOfWeek.Sunday);

            // assert
            result.Count().ShouldBe(2);
            result.ElementAt(0).ShouldBe(new EpiDate(1, 2020));
            result.ElementAt(1).ShouldBe(new EpiDate(2, 2020));
        }

        [Theory, MemberData(nameof(EpiWeekToFirstDateTestData))]
        public void GetFirstDateOfEpiWeek_ShouldReturnDateOfTheSundayInTheEpiWeek(int weekNumber, int year, DateTime firstDateInWeek, DayOfWeek epiWeekStartDay)
        {
            // Act
            var result = _dateTimeProvider.GetFirstDateOfEpiWeek(year, weekNumber, epiWeekStartDay);

            // Assert
            result.ShouldBe(firstDateInWeek);
        }
    }
}
