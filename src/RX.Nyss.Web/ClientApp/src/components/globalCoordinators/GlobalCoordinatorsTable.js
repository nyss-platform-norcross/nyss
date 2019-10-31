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

export const GlobalCoordinatorsTable = ({ isListFetching, isRemoving, goToEdition, remove, list }) => {
  if (isListFetching) {
    return <Loading />;
  }

  return (
    <Table>
      <TableHead>
        <TableRow>
          <TableCell>Name</TableCell>
          <TableCell style={{ width: "25%", minWidth: 100 }}>E-mail</TableCell>
          <TableCell style={{ width: "8%", minWidth: 75 }}>Phone number</TableCell>
          <TableCell style={{ width: "16%" }}>Organization</TableCell>
          <TableCell style={{ width: "16%" }} />
        </TableRow>
      </TableHead>
      <TableBody>
        {list.map(row => (
          <TableRow key={row.id} hover onClick={() => goToEdition(row.id)} className={styles.clickableRow}>
            <TableCell>{row.name}</TableCell>
            <TableCell>{row.email}</TableCell>
            <TableCell>{row.phoneNumber}</TableCell>
            <TableCell>{row.organization}</TableCell>
            <TableCell style={{ textAlign: "right", paddingTop: 0, paddingBottom: 0 }}>
              <TableRowAction onClick={() => goToEdition(row.id)} icon={<EditIcon />} title={"Edit"} />
              <TableRowAction onClick={() => remove(row.id)} confirmationText="Are you sure?" icon={<ClearIcon />} title={"Delete"} isFetching={isRemoving[row.id]} />
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

GlobalCoordinatorsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default GlobalCoordinatorsTable;