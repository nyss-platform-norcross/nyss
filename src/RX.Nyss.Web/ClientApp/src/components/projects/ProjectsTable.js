import styles from '../common/table/Table.module.scss';
import React, { Fragment, useState } from 'react';
import PropTypes from "prop-types";
import dayjs from "dayjs"
import ArchiveIcon from '@material-ui/icons/Archive';
import WarningIcon from '@material-ui/icons/Warning';
import * as roles from '../../authentication/roles';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import { TableContainer } from '../common/table/TableContainer';
import { TableRowActions } from '../common/tableRowAction/TableRowActions';
import { TableRowMenu } from '../common/tableRowAction/TableRowMenu';
import { ConfirmationDialog } from '../common/confirmationDialog/ConfirmationDialog';
import { Typography, Grid, Table, TableBody, TableCell, TableHead, TableRow } from '@material-ui/core';


export const ProjectsTable = ({ isListFetching, goToDashboard, list, nationalSocietyId, close, isClosing, callingUserRoles, isHeadManager, rtl }) => {
  const [closeConfirmationDialog, setRemoveConfirmationDialog] = useState({ isOpen: false, projectId: null });

  const closeConfirmed = () => {
    close(nationalSocietyId, closeConfirmationDialog.projectId);
    setRemoveConfirmationDialog({ isOpen: false })
  }

  const userCanCloseProject = callingUserRoles.some(r => r === roles.Administrator || r === roles.Coordinator) || isHeadManager;

  if (isListFetching) {
    return <Loading />;
  }

  const getRowMenu = (project) => [
    {
      title: strings(stringKeys.project.list.close),
      disabled: project.isClosed || !userCanCloseProject,
      action: () => setRemoveConfirmationDialog({ isOpen: true, projectId: project.id })
    }
  ];

  return (
    <Fragment>
      <TableContainer sticky>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell style={{ minWidth: 160 }}>{strings(stringKeys.project.list.name)}</TableCell>
              <TableCell style={{ width: "20%", minWidth: 80 }}>{strings(stringKeys.project.list.startDate)}</TableCell>
              <TableCell style={{ width: "20%", minWidth: 80 }}>{strings(stringKeys.project.list.endDate)}</TableCell>
              <TableCell style={{ width: "10%" }}>{strings(stringKeys.project.list.supervisorCount)}</TableCell>
              <TableCell style={{ width: "10%" }}>{strings(stringKeys.project.list.totalDataCollectorCount)}</TableCell>
              <TableCell style={{ width: "10%" }}>{strings(stringKeys.project.list.totalReportCount)}</TableCell>
              <TableCell style={{ width: "10%" }}>{strings(stringKeys.project.list.escalatedAlertCount)}</TableCell>
              <TableCell style={{ width: "10%" }} />
            </TableRow>
          </TableHead>
          <TableBody>
            {list.map(project => (
              <TableRow
                key={project.id}
                hover
                onClick={() => goToDashboard(nationalSocietyId, project.id)}
                className={project.isClosed ? styles.inactiveRow : styles.clickableRow}>
                <TableCell>{project.name}</TableCell>
                <TableCell>{dayjs(project.startDate).format("YYYY-MM-DD")}</TableCell>
                <TableCell>{project.endDate ? dayjs(project.endDate).format("YYYY-MM-DD") : strings(stringKeys.project.list.ongoing)}</TableCell>
                <TableCell>{project.supervisorCount}</TableCell>
                <TableCell>{project.totalDataCollectorCount}</TableCell>
                <TableCell>{project.totalReportCount}</TableCell>
                <TableCell>{project.escalatedAlertCount}</TableCell>
                <TableCell>
                  <TableRowActions directionRtl={rtl}>
                    <TableRowMenu
                      id={project.id}
                      items={getRowMenu(project)}
                      icon={<ArchiveIcon />}
                      isFetching={isClosing[project.id]}
                      directionRtl={rtl}
                    />
                  </TableRowActions>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
      <ConfirmationDialog
        isOpened={closeConfirmationDialog.isOpen}
        titleText={strings(stringKeys.project.list.removalConfirmation)}
        submit={() => closeConfirmed()}
        close={() => setRemoveConfirmationDialog({ isOpen: false })}
      >
        <Grid container spacing={2} alignItems="center">
          <Grid item>
            <Grid container direction="row" spacing={2} alignItems="center">
              <Grid item xs={2} style={{ textAlign: "center" }}>
                <WarningIcon color="error" style={{ fontSize: "45px", verticalAlign: "bottom" }} />
              </Grid>
              <Grid item xs={10}>
                <Typography variant="body1">{strings(stringKeys.project.list.removalConfirmationText)}</Typography>
              </Grid>
            </Grid>
          </Grid>
          <Grid item>
            <Typography variant="body1" color="error">{strings(stringKeys.project.list.removalConfirmationTextTwo)}</Typography>
          </Grid>
        </Grid>
      </ConfirmationDialog>
    </Fragment>
  );
}

ProjectsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default ProjectsTable;
