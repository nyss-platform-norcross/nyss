import React from "react";
import { Card, CardContent, CardHeader, Typography } from "@material-ui/core";
import Highcharts from "highcharts";
import HighchartsReact from "highcharts-react-official";
import { strings, stringKeys } from "../../strings";
import useHoverChartTracking from "../../utils/useHoverChartTracking";

const getOptions = (valuesLabel, series, categories) => {
  // Calculate maximum absolute value in the series data
  const maxAbsValue = Math.max(
    ...series.flatMap((s) => s.data.map((d) => Math.abs(d))),
  );
  return {
    chart: {
      type: "bar",
    },
    title: {
      text: "",
    },
    xAxis: {
      categories: categories,
    },
    yAxis: {
      title: {
        text: valuesLabel,
      },
      labels: {
        formatter: function () {
          // Display positive values on the x-axis
          return Math.abs(this.value) + "%";
        },
      },
      allowDecimals: false,
      min: -maxAbsValue,
      max: maxAbsValue,
    },
    credits: {
      enabled: false,
    },
    plotOptions: {
      series: {
        stacking: "normal", // Set to 'normal' for side-by-side bars
      },
    },
    tooltip: {
      formatter: function () {
        // Customize tooltip to display positive values
        return (
          "<b>" +
          this.x +
          "</b><br/>" +
          this.series.name +
          ": " +
          Math.abs(this.y).toFixed(1) +
          "%"
        );
      },
    },
    series: series,
  };
};

export const DashboardReportSexAgePyramidChart = ({ data }) => {
  const trackHoveredChart = useHoverChartTracking();
  const categories = [
    strings(stringKeys.dashboard.reportsPerFeature.above5, true),
    strings(stringKeys.dashboard.reportsPerFeature.below5, true),
    `${strings(
      stringKeys.dashboard.reportsPerFeature.unspecifiedAge,
      true,
    )} + ${strings(
      stringKeys.dashboard.reportsPerFeature.unspecifiedSex,
      true,
    )}`,
  ];
  const totalNumbers = data.reduce(
    (acc, obj) => {
      acc.countFemalesAtLeastFive += obj.countFemalesAtLeastFive;
      acc.countFemalesBelowFive += obj.countFemalesBelowFive;
      acc.countMalesAtLeastFive += obj.countMalesAtLeastFive;
      acc.countMalesBelowFive += obj.countMalesBelowFive;
      acc.countUnspecifiedSexAndAge += obj.countUnspecifiedSexAndAge;
      return acc;
    },
    {
      countFemalesAtLeastFive: 0,
      countFemalesBelowFive: 0,
      countMalesAtLeastFive: 0,
      countMalesBelowFive: 0,
      countUnspecifiedSexAndAge: 0,
    },
  );

  // Calculate total count
  const totalCount =
    totalNumbers.countFemalesAtLeastFive +
    totalNumbers.countFemalesBelowFive +
    totalNumbers.countMalesAtLeastFive +
    totalNumbers.countMalesBelowFive +
    totalNumbers.countUnspecifiedSexAndAge;

  // Calculate percentages if there is data
  const femaleAtLeastFivePercentage = totalCount
    ? (totalNumbers.countFemalesAtLeastFive / totalCount) * 100
    : 0;
  const femaleBelowFivePercentage = totalCount
    ? (totalNumbers.countFemalesBelowFive / totalCount) * 100
    : 0;
  const maleAtLeastFivePercentage = totalCount
    ? (totalNumbers.countMalesAtLeastFive / totalCount) * 100
    : 0;
  const maleBelowFivePercentage = totalCount
    ? (totalNumbers.countMalesBelowFive / totalCount) * 100
    : 0;
  const unspecifiedPercentage = totalCount
    ? (totalNumbers.countUnspecifiedSexAndAge / totalCount) * 100
    : 0;

  const series = [
    {
      name: strings(stringKeys.reports.sexAgeConstants.female, true),
      stack: "stack",
      data: [-femaleAtLeastFivePercentage, -femaleBelowFivePercentage],
      color: "#078e5e",
    },
    {
      name: strings(stringKeys.reports.sexAgeConstants.male, true),
      stack: "stack",
      data: [maleAtLeastFivePercentage, maleBelowFivePercentage],
      color: "#72d5fb",
    },
  ];

  const chartData = getOptions(
    strings(
      stringKeys.dashboard.reportsPerFeatureAndDate.percentageOfReportedPeople,
      true,
    ),
    series,
    categories,
  );

  return (
    <Card
      data-printable={true}
      onMouseEnter={() => trackHoveredChart("hoveredReportSexAgeChart")}
      onTouchStart={() => trackHoveredChart("hoveredReportSexAgeChart")}
    >
      <CardHeader
        title={
          <Typography variant="h5">
            {strings(
              stringKeys.dashboard.reportsPerFeatureAndDate.titlePercentage,
            )}
          </Typography>
        }
      />
      <CardContent>
        <HighchartsReact
          highcharts={Highcharts}
          ref={(element) => element && element.chart.reflow()}
          options={chartData}
        />
        <Typography variant="h6" align="center">
          {strings(
            stringKeys.dashboard.reportsPerFeatureAndDate.unspecifiedSexAndAge,
          )}
          : {unspecifiedPercentage.toFixed(1)}%
        </Typography>
      </CardContent>
    </Card>
  );
};
