import styles from '../common/table/Table.module.scss';
import React from 'react';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import dayjs from "dayjs"
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import { TableContainer } from '../common/table/TableContainer';

export const ProjectsTable = ({ isListFetching, goToDashboard, list, nationalSocietyId }) => {
  if (isListFetching) {
    return <Loading />;
  }

  return (
    <TableContainer>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell style={{ minWidth: 160 }}>{strings(stringKeys.project.list.name)}</TableCell>
            <TableCell style={{ width: "10%", minWidth: 80 }}>{strings(stringKeys.project.list.startDate)}</TableCell>
            <TableCell style={{ width: "10%", minWidth: 80 }}>{strings(stringKeys.project.list.endDate)}</TableCell>
            <TableCell style={{ width: "13%" }}>{strings(stringKeys.project.list.totalReportCount)}</TableCell>
            <TableCell style={{ width: "13%" }}>{strings(stringKeys.project.list.totalDataCollectorCount)}</TableCell>
            <TableCell style={{ width: "13%" }}>{strings(stringKeys.project.list.escalatedAlertCount)}</TableCell>
            <TableCell style={{ width: "13%" }}>{strings(stringKeys.project.list.supervisorCount)}</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(project => (
            <TableRow key={project.id} hover onClick={() => goToDashboard(nationalSocietyId, project.id)} className={styles.clickableRow}>
              <TableCell>{project.name}</TableCell>
              <TableCell>{dayjs(project.startDate).format("YYYY-MM-DD")}</TableCell>
              <TableCell>{project.endDate ? dayjs(project.endDate).format("YYYY-MM-DD") : strings(stringKeys.project.list.ongoing)}</TableCell>
              <TableCell>{project.totalReportCount}</TableCell>
              <TableCell>{project.totalDataCollectorCount}</TableCell>
              <TableCell>{project.escalatedAlertCount}</TableCell>
              <TableCell>{project.supervisorCount}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

ProjectsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default ProjectsTable;
