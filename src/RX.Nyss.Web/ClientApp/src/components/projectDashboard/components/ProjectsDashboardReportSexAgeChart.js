import React from 'react';
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import CardHeader from '@material-ui/core/CardHeader';
import Highcharts from 'highcharts'
import HighchartsReact from 'highcharts-react-official'
import { strings, stringKeys } from '../../../strings';

const getOptions = (valuesLabel, series, categories) => ({
  chart: {
    type: 'column',
    backgroundColor: "transparent",
  },
  title: {
    text: ''
  },
  xAxis: {
    categories: categories,
  },
  yAxis: {
    title: {
      text: valuesLabel
    },
    allowDecimals: false
  },
  legend: {
    enabled: false
  },
  credits: {
    enabled: false
  },
  plotOptions: {
    column: {
      stacking: 'normal'
    }
  },
  tooltip: {
    headerFormat: '',
    pointFormat: '{series.name}: <b>{point.y}</b>'
  },
  series
});

export const ProjectsDashboardReportSexAgeChart = ({ data }) => {
  const categories = data.map(d => d.period);

  const series = [
    {
      name: strings(stringKeys.project.dashboard.reportsPerFeatureAndDate.femalesAbove5, true),
      data: data.map(d => d.countFemalesAtLeastFive),
      color: "#078e5e"
    },
    {
      name: strings(stringKeys.project.dashboard.reportsPerFeatureAndDate.femalesBelow5, true),
      data: data.map(d => d.countFemalesBelowFive),
      color: "#47c79a"
    },
    {
      name: strings(stringKeys.project.dashboard.reportsPerFeatureAndDate.malesAbove5, true),
      data: data.map(d => d.countMalesAtLeastFive),
      color: "#00a0dc"
    },
    {
      name: strings(stringKeys.project.dashboard.reportsPerFeatureAndDate.malesBelow5, true),
      data: data.map(d => d.countMalesBelowFive),
      color: "#72d5fb"
    }
  ];

  const chartData = getOptions(strings(stringKeys.project.dashboard.reportsPerFeatureAndDate.numberOfReports, true), series, categories);

  return (
    <Card>
      <CardHeader title={strings(stringKeys.project.dashboard.reportsPerFeatureAndDate.title)} />
      <CardContent>
        <HighchartsReact
          highcharts={Highcharts}
          ref={element => element && element.chart.reflow()}
          options={chartData}
        />
      </CardContent>
    </Card>
  );
}
