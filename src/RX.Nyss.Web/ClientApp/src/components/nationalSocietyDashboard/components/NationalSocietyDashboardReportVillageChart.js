import React from "react";
import { Card, CardContent, CardHeader } from "@material-ui/core";
import Highcharts from "highcharts";
import HighchartsReact from "highcharts-react-official";
import { strings, stringKeys } from "../../../strings";

const getOptions = (valuesLabel, series, categories) => ({
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

export const NationalSocietyDashboardReportVillageChart = ({ data }) => {
  const moduleStrings = stringKeys.dashboard.reportsPerVillageAndDate;
  const categories = data.allPeriods;
  const villages = data.villages.length
    ? data.villages
    : [{ name: "", periods: [] }];

  const series = villages.map((village) => ({
    name:
      village.name === "(rest)"
        ? strings(moduleStrings.rest, true)
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
    strings(moduleStrings.numberOfReports, true),
    series,
    categories,
  );

  return (
    <Card data-printable={true}>
      <CardHeader title={strings(moduleStrings.title)} />
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
