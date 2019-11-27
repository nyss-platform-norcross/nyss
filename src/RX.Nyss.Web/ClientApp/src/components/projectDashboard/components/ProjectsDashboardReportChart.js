import styles from "./ProjectsDashboardReportsMap.module.scss";

import React, { useRef } from 'react';
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import CardHeader from '@material-ui/core/CardHeader';
import { Loading } from '../../common/loading/Loading';
import Grid from '@material-ui/core/Grid';
import Highcharts from 'highcharts'
import HighchartsReact from 'highcharts-react-official'
import dayjs from "dayjs"
import { useMount } from "../../../utils/lifecycle";

const options = {
  chart: {
    type: 'column',
    backgroundColor: "transparent",
  },
  title: {
    text: ''
  },
  xAxis: {
    type: 'category',
  },
  yAxis: {
    title: {
      text: 'Number of reports'
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
    series: {
      groupPadding: 0,
      pointPadding: 0,
      borderWidth: 1
    }
  },
  tooltip: {
    headerFormat: '',
    pointFormat: '{point.name}: <b>{point.y}</b>'
  }
}

export const ProjectsDashboardReportChart = ({ data }) => {
  const resizeChart = element => { element && element.chart.reflow() };

  const chartData = {
    ...options,
    series: [
      {
        name: "Periods",
        data: data.map(d => ({ name: d.period, y: d.count }))
      }
    ]
  }

  return (
    <Card>
      <CardHeader title="Number of reports" />
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
