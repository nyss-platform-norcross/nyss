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
    categories: categories,
  },
  yAxis: {
    title: {
      text: valuesLabel
    },
    allowDecimals: false
  },
  legend: {
    enabled: true,
    itemStyle: { fontWeight: "regular" }
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

export const ProjectsDashboardDataCollectionPointChart = ({ data }) => {
  const categories = data.map(d => d.period);

  const series = [
    {
      name: strings(stringKeys.project.dashboard.dataCollectionPointReportsByDate.referredToHospitalCount, true),
      data: data.map(d => d.referredCount),
      color: "#078e5e"
    },
    {
      name: strings(stringKeys.project.dashboard.dataCollectionPointReportsByDate.fromOtherVillagesCount, true),
      data: data.map(d => d.fromOtherVillagesCount),
      color: "#00a0dc"
    },
    {
      name: strings(stringKeys.project.dashboard.dataCollectionPointReportsByDate.deathCount, true),
      data: data.map(d => d.deathCount),
      color: "#47c79a"
    }
  ];

  const chartData = getOptions(strings(stringKeys.project.dashboard.dataCollectionPointReportsByDate.numberOfReports, true), series, categories);

  return (
    <Card data-printable={true}>
      <CardHeader title={strings(stringKeys.project.dashboard.dataCollectionPointReportsByDate.title)} />
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
