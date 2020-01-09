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
    style: {
      fontFamily: 'Poppins,"Helvetica Neue",Arial'
    }
  },
  title: {
    text: ''
  },
  xAxis: {
    type: 'category',
    categories: categories
  },
  yAxis: {
    title: {
      text: valuesLabel
    },
    allowDecimals: false
  },
  legend: {
    enabled: false,
    style: { fontWeight: "regular" }
  },
  credits: {
    enabled: false
  },
  plotOptions: {
    series: {
      groupPadding: 0,
      pointPadding: 0,
      borderWidth: 1
    }
  },
  tooltip: {
    headerFormat: '',
    pointFormat: '{point.name}: <b>{point.y}</b>'
  },
  series
});

export const ProjectsDashboardReportChart = ({ data }) => {
  const resizeChart = element => { element && element.chart.reflow() };

  const series = [
    {
      name: strings(stringKeys.project.dashboard.allReportsChart.periods, true),
      data: data.map(d => ({ name: d.period, y: d.count })),
      color: "#72d5fb"
    }
  ];

  const categories = data.map(d => d.period);

  const chartData = getOptions(strings(stringKeys.project.dashboard.allReportsChart.numberOfReports, true), series, categories)

  return (
    <Card data-printable={true}>
      <CardHeader title={strings(stringKeys.project.dashboard.allReportsChart.title)} />
      <CardContent>
        <HighchartsReact
          highcharts={Highcharts}
          ref={resizeChart}
          options={chartData}
        />
      </CardContent>
    </Card>
  );
}
