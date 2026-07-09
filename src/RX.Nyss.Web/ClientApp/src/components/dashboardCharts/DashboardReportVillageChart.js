import React from "react";
import { Card, CardContent, CardHeader, Typography } from "@material-ui/core";
import Highcharts from "highcharts";
import HighchartsReact from "highcharts-react-official";
import { strings, stringKeys } from "../../strings";
import useHoverChartTracking from "../../utils/useHoverChartTracking";

const getOptions = (valuesLabel, series, categories, groupingType) => ({
  chart: {
    type: "column",
    backgroundColor: "transparent",
    style: {
      fontFamily: "Arial",
    },
  },
  title: {
    text: "",
  },
  xAxis: {
    categories: categories,
    labels: {
      formatter: function () {
        // Display only week, input data format "yyyy/MM", output format "MM"
        if (groupingType === "Week") {
          let weekWithoutYear = this.value.split("/")[1]
          return weekWithoutYear;
        }
        return this.value;
      },
    },
  },
  yAxis: {
    title: {
      text: valuesLabel,
    },
    allowDecimals: false,
  },
  legend: {
    enabled: true,
    itemStyle: { fontWeight: "regular" },
  },
  credits: {
    enabled: false,
  },
  plotOptions: {
    column: {
      stacking: "normal",
    },
  },
  tooltip: {
    headerFormat: "",
    pointFormat: "{series.name}: <b>{point.y}</b>",
  },
  colors: [
    "#00a0dc",
    "#a175ca",
    "#47c79a",
    "#72d5fb",
    "#c37f8d",
    "#c3bb7f",
    "#e4d144",
    "#078e5e",
    "#ceb5ba",
    "#c2b5ce",
    "#e0c8af",
  ],
  series,
});

export const DashboardReportVillageChart = ({ data, groupingType }) => {
  const trackHoveredChart = useHoverChartTracking();
  const categories = data.allPeriods;
  const villages = data.villages.length
    ? data.villages
    : [{ name: "", periods: [] }];

  const series = villages.map((village) => ({
    name:
      village.name === "(rest)"
        ? strings(stringKeys.dashboard.reportsPerVillageAndDate.rest, true)
        : village.name,
    data: data.allPeriods.map(
      (period) =>
        village.periods
          .filter((p) => p.period === period)
          .map((p) => p.count)
          .find((_) => true) || 0,
    ),
  }));

  const chartData = getOptions(
    strings(
      stringKeys.dashboard.reportsPerVillageAndDate.numberOfReports,
      true,
    ),
    series,
    categories,
    groupingType,
  );
  return (
    <Card data-printable={true} onMouseEnter={() => trackHoveredChart("hoveredReportVillageChart")} onTouchStart={() => trackHoveredChart("hoveredReportVillageChart")}>
      <CardHeader title={<Typography variant="h5">{strings(stringKeys.dashboard.reportsPerVillageAndDate.title)}</Typography>}/>
      <CardContent>
        <HighchartsReact
          highcharts={Highcharts}
          ref={(element) => element && element.chart.reflow()}
          options={chartData}
        />
      </CardContent>
    </Card>
  );
};
