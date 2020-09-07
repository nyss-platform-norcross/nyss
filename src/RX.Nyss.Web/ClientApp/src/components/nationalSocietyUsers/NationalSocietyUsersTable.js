import styles from '../common/table/Table.module.scss';
import React, { Fragment } from 'react';
import MoreVertIcon from '@material-ui/icons/MoreVert';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import ClearIcon from '@material-ui/icons/Clear';
import EditIcon from '@material-ui/icons/Edit';
import CheckIcon from '@material-ui/icons/Check';
import MoreHorizIcon from '@material-ui/icons/MoreHoriz';
import * as Roles from '../../authentication/roles';
import { TableRowAction } from '../common/tableRowAction/TableRowAction';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import { TableRowMenu } from '../common/tableRowAction/TableRowMenu';
import Tooltip from '@material-ui/core/Tooltip';
import { accessMap } from '../../authentication/accessMap';
import { TableContainer } from '../common/table/TableContainer';
import { TableRowActions } from '../common/tableRowAction/TableRowActions';

export const NationalSocietyUsersTable = ({ isListFetching, isRemoving, goToEdition, remove, list, nationalSocietyId, setAsHeadManager, isSettingAsHead, user }) => {
  if (isListFetching) {
    return <Loading />;
  }

  const headManagers = list.filter((u) => { return u.isHeadManager; });
  const hasSimilarAccess = user.roles.filter((r) => { return accessMap.nationalSocietyUsers.headManagerAccess.indexOf(r) !== -1; }).length > 0;
  const hasHeadManagerAccess = hasSimilarAccess || user.email === (headManagers.length > 0 && headManagers[0].email);

  const canBeSetAsHeadManager = (row) => {
    return (hasHeadManagerAccess && !row.isHeadManager &&
      (Roles.TechnicalAdvisor.toLowerCase() === row.role.toLowerCase() || Roles.Manager.toLowerCase() === row.role.toLowerCase())) || false;
  }

  const getRowMenu = (row) => [
    {
      condition: canBeSetAsHeadManager(row),
      title: strings(stringKeys.nationalSocietyConsents.setAsHeadManager),
      action: () => setAsHeadManager(row.organizationId, row.id)
    }
  ];

  const canBeEdited = (row) => row.role !== Roles.DataConsumer || row.isVerified || user.roles.some(r => r === Roles.Administrator);

  return (
    <TableContainer sticky>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>{strings(stringKeys.nationalSocietyUser.list.name)}</TableCell>
            <TableCell>{strings(stringKeys.nationalSocietyUser.form.email)}</TableCell>
            <TableCell>{strings(stringKeys.nationalSocietyUser.list.phoneNumber)}</TableCell>
            <TableCell>{strings(stringKeys.nationalSocietyUser.list.role)}</TableCell>
            <TableCell>{strings(stringKeys.nationalSocietyUser.list.organization)}</TableCell>
            <TableCell>{strings(stringKeys.nationalSocietyUser.list.project)}</TableCell>
            <TableCell align="center">{strings(stringKeys.nationalSocietyUser.list.headManager)}</TableCell>
            <TableCell style={{ width: "16%" }} />
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(row => (
            <TableRow key={row.id} onClick={() => canBeEdited(row) && goToEdition(nationalSocietyId, row.id)} hover className={(canBeEdited(row) && styles.clickableRow) || ''}>
              <TableCell>{row.name}</TableCell>
              <TableCell>{(row.role !== Roles.DataConsumer || row.isVerified) ? row.email : strings(stringKeys.nationalSocietyUser.list.notVerified)}</TableCell>
              <TableCell>{(row.role !== Roles.DataConsumer || row.isVerified) ? row.phoneNumber : strings(stringKeys.nationalSocietyUser.list.notVerified)}</TableCell>
              <TableCell>{strings(`role.${row.role.toLowerCase()}`)}</TableCell>
              <TableCell>{row.organizationName}</TableCell>
              <TableCell>{row.project}</TableCell>
              <TableCell align="center">{
                (row.isHeadManager && <CheckIcon fontSize="small" />) ||
                (row.isPendingHeadManager && <Tooltip title={strings(stringKeys.nationalSocietyConsents.pendingHeadManager)}><MoreHorizIcon fontSize="small" /></Tooltip>)
              }
              </TableCell>
              <TableCell>
                <TableRowActions>
                  <TableRowMenu id={row.id} items={getRowMenu(row)} icon={<MoreVertIcon />} isFetching={isSettingAsHead[row.id]} />
                  {canBeEdited(row) && (
                    <Fragment>
                      <TableRowAction onClick={() => goToEdition(nationalSocietyId, row.id)} icon={<EditIcon />} title={"Edit"} />
                      <TableRowAction onClick={() => remove(row.id, row.role, nationalSocietyId)} confirmationText={strings(stringKeys.nationalSocietyUser.list.removalConfirmation)} icon={<ClearIcon />} title={"Delete"} isFetching={isRemoving[row.id]} />
                    </Fragment>
                  )}
                </TableRowActions>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

NationalSocietyUsersTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default NationalSocietyUsersTable;
