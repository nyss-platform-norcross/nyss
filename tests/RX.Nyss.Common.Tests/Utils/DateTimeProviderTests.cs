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

        public static object[][] EpiWeekToFirstDateTestData =
        {
            new object[] { 1, 2020, new DateTime(2019, 12, 29) },
            new object[] { 15, 2020, new DateTime(2020, 4, 5) },
            new object[] { 40, 2020, new DateTime(2020, 9, 27) },
            new object[] { 53, 2020, new DateTime(2020, 12, 27) },
            new object[] { 1, 2021, new DateTime(2021, 1, 3) },
            new object[] { 18, 2021, new DateTime(2021, 5, 2) },
            new object[] { 27, 2021, new DateTime(2021, 7, 4) }
        };

        private readonly IDateTimeProvider _dateTimeProvider;


        public DateTimeProviderTests()
        {
            _dateTimeProvider = new DateTimeProvider();
        }

        [Theory, MemberData(nameof(TestData))]
        public void GetEpiWeek_ShouldReturnCorrectWeekNumber(DateTime date, int weekNumber, int year)
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

        [Theory, MemberData(nameof(EpiWeekToFirstDateTestData))]
        public void GetFirstDateOfEpiWeek_ShouldReturnDateOfTheSundayInTheEpiWeek(int weekNumber, int year, DateTime firstDateInWeek)
        {
            // Act
            var result = _dateTimeProvider.GetFirstDateOfEpiWeek(year, weekNumber);

            // Assert
            result.ShouldBe(firstDateInWeek);
        }
    }
}
