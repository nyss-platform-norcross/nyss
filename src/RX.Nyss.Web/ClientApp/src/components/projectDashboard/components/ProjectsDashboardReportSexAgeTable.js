import styles from "./ProjectsDashboardReportSexAgeTable.module.scss";

import React from 'react';
import { Card, CardContent, CardHeader, Table, TableBody, TableCell, TableRow } from '@material-ui/core';
import { strings, stringKeys } from '../../../strings';

export const ProjectsDashboardReportSexAgeTable = ({ data }) => {
  return (
    <Card data-printable={true}>
      <CardHeader title={strings(stringKeys.dashboard.reportsPerFeature.title)} />
      <CardContent>
        <Table className={styles.table}>
          <TableBody>
            <TableRow>
              <TableCell className={styles.rowHeader}></TableCell>
              <TableCell className={styles.rowHeader}>{strings(stringKeys.dashboard.reportsPerFeature.female)}</TableCell>
              <TableCell className={styles.rowHeader}>{strings(stringKeys.dashboard.reportsPerFeature.male)}</TableCell>
              <TableCell className={styles.rowHeader}>{strings(stringKeys.dashboard.reportsPerFeature.unspecifiedSex)}</TableCell>
              <TableCell className={styles.rowHeader}>{strings(stringKeys.dashboard.reportsPerFeature.total)}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell className={styles.columnHeader}>{strings(stringKeys.dashboard.reportsPerFeature.below5)}</TableCell>
              <TableCell>{data.countFemalesBelowFive}</TableCell>
              <TableCell>{data.countMalesBelowFive}</TableCell>
              <TableCell>-</TableCell>
              <TableCell>{data.countFemalesBelowFive + data.countMalesBelowFive}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell className={styles.columnHeader}>{strings(stringKeys.dashboard.reportsPerFeature.above5)}</TableCell>
              <TableCell>{data.countFemalesAtLeastFive}</TableCell>
              <TableCell>{data.countMalesAtLeastFive}</TableCell>
              <TableCell>-</TableCell>
              <TableCell>{data.countFemalesAtLeastFive + data.countMalesAtLeastFive}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell className={styles.columnHeader}>{strings(stringKeys.dashboard.reportsPerFeature.unspecifiedAge)}</TableCell>
              <TableCell>-</TableCell>
              <TableCell>-</TableCell>
              <TableCell>{data.countUnspecifiedSexAndAge}</TableCell>
              <TableCell>{data.countUnspecifiedSexAndAge}</TableCell>
            </TableRow>
            <TableRow>
              <TableCell className={styles.columnHeader}>{strings(stringKeys.dashboard.reportsPerFeature.total)}</TableCell>
              <TableCell>{data.countFemalesBelowFive + data.countFemalesAtLeastFive}</TableCell>
              <TableCell>{data.countMalesBelowFive + data.countMalesAtLeastFive}</TableCell>
              <TableCell>{data.countUnspecifiedSexAndAge}</TableCell>
              <TableCell>{data.countFemalesBelowFive + data.countFemalesAtLeastFive + data.countMalesBelowFive + data.countMalesAtLeastFive + data.countUnspecifiedSexAndAge}</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}
