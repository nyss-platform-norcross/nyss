import React from "react";
import { CardContent, Card, CardHeader } from "@material-ui/core";
import Highcharts from "highcharts";
import HighchartsReact from "highcharts-react-official";
import { strings, stringKeys } from "../../strings";

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
  series,
});

export const DashboardDataCollectionPointChart = ({ data }) => {
  const categories = data.map((d) => d.period);

  const series = [
    {
      name: strings(
        stringKeys.dashboard.dataCollectionPointReportsByDate
          .referredToHospitalCount,
        true,
      ),
      data: data.map((d) => d.referredCount),
      color: "#078e5e",
    },
    
  ];

  const chartData = getOptions(
    strings(
      stringKeys.dashboard.dataCollectionPointReportsByDate.numberOfReports,
      true,
    ),
    series,
    categories,
  );

  return (
    <Card data-printable={true}>
      <CardHeader
        title={strings(
          stringKeys.dashboard.dataCollectionPointReportsByDate.title,
        )}
      />
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
