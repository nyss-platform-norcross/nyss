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

export const DataCollectorsTable = ({ isListFetching, isRemoving, goToEdition, remove, list, projectId }) => {
  if (isListFetching) {
    return <Loading />;
  }

  return (
    <Table>
      <TableHead>
        <TableRow>
          <TableCell>{strings(stringKeys.dataCollector.list.name)}</TableCell>
          <TableCell style={{ width: "16%" }} />
        </TableRow>
      </TableHead>
      <TableBody>
        {list.map(row => (
          <TableRow key={row.id} hover onClick={() => goToEdition(projectId, row.id)} className={styles.clickableRow}>
            <TableCell>{row.name}</TableCell>
            <TableCell style={{ textAlign: "right", paddingTop: 0, paddingBottom: 0 }}>
              <TableRowAction onClick={() => goToEdition(projectId, row.id)} icon={<EditIcon />} title={"Edit"} />
              <TableRowAction onClick={() => remove(row.id)} confirmationText={strings(stringKeys.dataCollector.list.removalConfirmation)} icon={<ClearIcon />} title={"Delete"} isFetching={isRemoving[row.id]} />
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

DataCollectorsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default DataCollectorsTable;