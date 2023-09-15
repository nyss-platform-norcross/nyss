import React from 'react';
import { Card, CardContent, CardHeader } from '@material-ui/core';
import Highcharts from 'highcharts'
import HighchartsReact from 'highcharts-react-official'
import { strings, stringKeys } from '../../strings';

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
    categories: categories
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
      stacking: 'normal',
    }
  },
  tooltip: {
    headerFormat: '',
    pointFormat: '{series.name}: <b>{point.y}</b>'
  },
  colors: ["#00a0dc", "#a175ca", "#47c79a", "#72d5fb", "#c37f8d", "#c3bb7f", "#e4d144", "#078e5e", "#ceb5ba", "#c2b5ce", "#e0c8af"],
  series
});

export const DashboardReportChart = ({ data }) => {
  const resizeChart = element => { element && element.chart.reflow() };

  const categories = data.allPeriods;
  const healthRisks = data.healthRisks.length ? data.healthRisks : [{ healthRiskName: "", periods: [] } ];

  const series = healthRisks.map(healthRisk => ({
    name: healthRisk.healthRiskName === "(rest)" ? strings(stringKeys.dashboard.reportsPerHealthRisk.rest, true) : healthRisk.healthRiskName,
    data: data.allPeriods.map(period => healthRisk.periods.filter(p => p.period === period).map(p => p.count).find(_ => true) || 0),
  }));

  const chartData = getOptions(strings(stringKeys.dashboard.reportsPerHealthRisk.numberOfReports, true), series, categories)

  return (
    <Card data-printable={true}>
      <CardHeader title={strings(stringKeys.dashboard.reportsPerHealthRisk.title)} />
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
