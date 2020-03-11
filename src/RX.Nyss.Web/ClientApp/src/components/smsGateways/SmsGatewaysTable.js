import styles from '../common/table/Table.module.scss';
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

export const SmsGatewaysTable = ({ isListFetching, isRemoving, goToEdition, remove, list, nationalSocietyId }) => {
  if (isListFetching) {
    return <Loading />;
  }

  return (
    <TableContainer sticky>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>{strings(stringKeys.smsGateway.list.name)}</TableCell>
            <TableCell style={{ width: "25%", minWidth: 100 }}>{strings(stringKeys.smsGateway.list.apiKey)}</TableCell>
            <TableCell style={{ width: "16%", minWidth: 75 }}>{strings(stringKeys.smsGateway.list.gatewayType)}</TableCell>
            <TableCell style={{ width: "16%" }} />
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(row => (
            <TableRow key={row.id} hover onClick={() => goToEdition(nationalSocietyId, row.id)} className={styles.clickableRow}>
              <TableCell>{row.name}</TableCell>
              <TableCell>{row.apiKey}</TableCell>
              <TableCell>{strings(`smsGateway.type.${row.gatewayType.toLowerCase()}`)}</TableCell>
              <TableCell>
                <TableRowActions>
                  <TableRowAction onClick={() => goToEdition(nationalSocietyId, row.id)} icon={<EditIcon />} title={"Edit"} />
                  <TableRowAction onClick={() => remove(nationalSocietyId, row.id)} confirmationText={strings(stringKeys.smsGateway.list.confirmationText)} icon={<ClearIcon />} title={"Delete"} isFetching={isRemoving[row.id]} />
                </TableRowActions>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

SmsGatewaysTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default SmsGatewaysTable;