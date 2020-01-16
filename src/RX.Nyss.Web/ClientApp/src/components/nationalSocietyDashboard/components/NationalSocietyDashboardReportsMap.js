import React from 'react';
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import CardHeader from '@material-ui/core/CardHeader';
import { strings, stringKeys } from "../../../strings";
import { ReportsMap } from "../../maps/ReportsMap";

export const NationalSocietyDashboardReportsMap = ({ data, details, detailsFetching, nationalSocietyId, getReportHealthRisks }) => {
  const handleMarkerClick = (lat, lng) =>
    getReportHealthRisks(nationalSocietyId, lat, lng);

  return (
    <Card>
      <CardHeader title={strings(stringKeys.nationalSociety.dashboard.map.title)} />
      <CardContent>
        <ReportsMap
          data={data}
          details={details}
          detailsFetching={detailsFetching}
          onMarkerClick={handleMarkerClick}
        />
      </CardContent>
    </Card>
  );
}
