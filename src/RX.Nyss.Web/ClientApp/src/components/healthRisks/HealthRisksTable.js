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

export const HealthRisksTable = ({ isListFetching, isRemoving, goToEdition, remove, list }) => {
  if (isListFetching) {
    return <Loading />;
  }

  return (
    <Table>
      <TableHead>
        <TableRow>
          <TableCell style={{ width: "10%", minWidth: 100 }}>Number</TableCell>
          <TableCell>Health risk / event</TableCell>
          <TableCell style={{ width: "25%", minWidth: 100 }}>Type</TableCell>
          <TableCell style={{ width: "25%" }} />
        </TableRow>
      </TableHead>
      <TableBody>
        {list.map(row => (
          <TableRow key={row.id} hover onClick={() => goToEdition(row.id)} className={styles.clickableRow}>
            <TableCell>{row.healthRiskCode}</TableCell>
            <TableCell>{row.name}</TableCell>
            <TableCell>{row.healthRiskType}</TableCell>
            <TableCell style={{ textAlign: "right", paddingTop: 0, paddingBottom: 0 }}>
              <TableRowAction onClick={() => goToEdition(row.id)} icon={<EditIcon />} title={"Edit"} />
              <TableRowAction onClick={() => remove(row.id)} confirmationText="Confirm removing the item" icon={<ClearIcon />} title={"Delete"} isFetching={isRemoving[row.id]} />
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

HealthRisksTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default HealthRisksTable;