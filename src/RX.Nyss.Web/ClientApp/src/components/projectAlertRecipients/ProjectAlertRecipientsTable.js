import tableStyles from '../common/table/Table.module.scss';
import React from 'react';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import ClearIcon from '@material-ui/icons/Clear';
import EditIcon from '@material-ui/icons/Edit';
import { TableRowAction } from '../common/tableRowAction/TableRowAction';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import { TableContainer } from '../common/table/TableContainer';
import { TableRowActions } from '../common/tableRowAction/TableRowActions';

export const ProjectAlertRecipientsTable = ({ isListFetching, isRemoving, goToEdition, remove, list, projectId, isClosed }) => {
  if (isListFetching) {
    return <Loading />;
  }

  return (
    <TableContainer sticky>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>{strings(stringKeys.projectAlertRecipient.list.role)}</TableCell>
            <TableCell>{strings(stringKeys.projectAlertRecipient.list.organization)}</TableCell>
            <TableCell>{strings(stringKeys.projectAlertRecipient.list.email)}</TableCell>
            <TableCell>{strings(stringKeys.projectAlertRecipient.list.phoneNumber)}</TableCell>
            <TableCell>{strings(stringKeys.projectAlertRecipient.list.healthRisks)}</TableCell>
            <TableCell>{strings(stringKeys.projectAlertRecipient.list.supervisors)}</TableCell>
            <TableCell style={{ width: "10%" }} />
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(row => (
            <TableRow key={row.id} onClick={() => !isClosed && goToEdition(projectId, row.id)} hover={!isClosed} className={!isClosed ? tableStyles.clickableRow : null}>
              <TableCell>{row.role}</TableCell>
              <TableCell>{row.organization}</TableCell>
              <TableCell>{row.email}</TableCell>
              <TableCell>{row.phoneNumber}</TableCell>
              <TableCell>{row.healthRisks.length > 0 ? row.healthRisks.join(", ") : "Any"}</TableCell>
              <TableCell>{row.supervisors.length > 0 ? row.supervisors.join(", ") : "Any"}</TableCell>
              <TableCell>
                {!isClosed &&
                  <TableRowActions>
                    <TableRowAction onClick={() => goToEdition(projectId, row.id)} icon={<EditIcon />} title={"Edit"} />
                    <TableRowAction onClick={() => remove(projectId, row.id)} confirmationText={strings(stringKeys.projectAlertRecipient.list.removalConfirmation)} icon={<ClearIcon />} title={"Delete"} isFetching={isRemoving[row.id]} />
                  </TableRowActions>}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

ProjectAlertRecipientsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default ProjectAlertRecipientsTable;