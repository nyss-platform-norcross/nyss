import React from 'react';
import { Card, CardContent, CardHeader } from '@material-ui/core';
import { strings, stringKeys } from "../../strings";
import { ReportsMap } from "../maps/ReportsMap";

export const DashboardReportsMap = ({ data, details, detailsFetching, getReportHealthRisks }) => {

  return (
    <Card data-printable={true}>
      <CardHeader title={strings(stringKeys.dashboard.map.title)} />
      <CardContent>
        <ReportsMap
          data={data}
          details={details}
          detailsFetching={detailsFetching}
          onMarkerClick={getReportHealthRisks}
        />
      </CardContent>
    </Card>
  );
}
