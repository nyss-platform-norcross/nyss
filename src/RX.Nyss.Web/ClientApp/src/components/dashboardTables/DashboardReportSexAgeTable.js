import styles from "./DashboardReportSexAgeTable.module.scss";

import React from "react";
import {
  Card,
  CardContent,
  CardHeader,
  Table,
  TableBody,
  TableCell,
  TableRow,
  Typography,
  TableContainer,
} from "@material-ui/core";
import { strings, stringKeys } from "../../strings";
import useHoverChartTracking from "../../utils/useHoverChartTracking";

export const DashboardReportSexAgeTable = ({ data }) => {
  const trackHoveredChart = useHoverChartTracking();

  return (
    <Card
      data-printable={true}
      onMouseEnter={() => trackHoveredChart("hoveredReportSexAgeTable")}
      onTouchStart={() => trackHoveredChart("hoveredReportSexAgeTable")}
    >
      <CardHeader
        title={
          <Typography variant="h5">
            {strings(stringKeys.dashboard.reportsPerFeature.title)}
          </Typography>
        }
      />
      <CardContent>
        <TableContainer>
          <Table className={styles.table} stickyHeader>
            <TableBody>
              <TableRow>
                <TableCell className={styles.rowHeader}></TableCell>
                <TableCell className={styles.rowHeader}>
                  {strings(stringKeys.dashboard.reportsPerFeature.female)}
                </TableCell>
                <TableCell className={styles.rowHeader}>
                  {strings(stringKeys.dashboard.reportsPerFeature.male)}
                </TableCell>
                <TableCell className={styles.rowHeader}>
                  {strings(
                    stringKeys.dashboard.reportsPerFeature.unspecifiedSex,
                  )}
                </TableCell>
                <TableCell className={styles.rowHeader}>
                  {strings(stringKeys.dashboard.reportsPerFeature.total)}
                </TableCell>
              </TableRow>
              <TableRow>
                <TableCell className={styles.columnHeader}>
                  {strings(stringKeys.dashboard.reportsPerFeature.below5)}
                </TableCell>
                <TableCell>{data.countFemalesBelowFive}</TableCell>
                <TableCell>{data.countMalesBelowFive}</TableCell>
                <TableCell>-</TableCell>
                <TableCell>
                  {data.countFemalesBelowFive + data.countMalesBelowFive}
                </TableCell>
              </TableRow>
              <TableRow>
                <TableCell className={styles.columnHeader}>
                  {strings(stringKeys.dashboard.reportsPerFeature.above5)}
                </TableCell>
                <TableCell>{data.countFemalesAtLeastFive}</TableCell>
                <TableCell>{data.countMalesAtLeastFive}</TableCell>
                <TableCell>-</TableCell>
                <TableCell>
                  {data.countFemalesAtLeastFive + data.countMalesAtLeastFive}
                </TableCell>
              </TableRow>
              <TableRow>
                <TableCell className={styles.columnHeader}>
                  {strings(
                    stringKeys.dashboard.reportsPerFeature.unspecifiedAge,
                  )}
                </TableCell>
                <TableCell>-</TableCell>
                <TableCell>-</TableCell>
                <TableCell>{data.countUnspecifiedSexAndAge}</TableCell>
                <TableCell>{data.countUnspecifiedSexAndAge}</TableCell>
              </TableRow>
              <TableRow>
                <TableCell className={styles.columnHeader}>
                  {strings(stringKeys.dashboard.reportsPerFeature.total)}
                </TableCell>
                <TableCell>
                  {data.countFemalesBelowFive + data.countFemalesAtLeastFive}
                </TableCell>
                <TableCell>
                  {data.countMalesBelowFive + data.countMalesAtLeastFive}
                </TableCell>
                <TableCell>{data.countUnspecifiedSexAndAge}</TableCell>
                <TableCell>
                  {data.countFemalesBelowFive +
                    data.countFemalesAtLeastFive +
                    data.countMalesBelowFive +
                    data.countMalesAtLeastFive +
                    data.countUnspecifiedSexAndAge}
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </TableContainer>
      </CardContent>
    </Card>
  );
};
