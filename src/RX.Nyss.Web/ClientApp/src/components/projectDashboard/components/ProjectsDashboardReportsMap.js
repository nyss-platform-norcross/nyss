import styles from "./ProjectsDashboardReportsMap.module.scss";

import React from 'react';
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import CardHeader from '@material-ui/core/CardHeader';
import { Loading } from '../../common/loading/Loading';
import Grid from '@material-ui/core/Grid';

export const ProjectsDashboardReportsMap = ({ data }) => {
  return (
    <Card>
      <CardHeader title="Map" />
      <CardContent>
        Map
      </CardContent>
    </Card>
  );
}
