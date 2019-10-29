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
import dayjs from "dayjs"
import { TableRowAction } from '../common/tableRowAction/TableRowAction';
import { Loading } from '../common/loading/Loading';

export const NationalSocietiesTable = ({ isListFetching, isRemoving, goToEdition, goToDashboard, remove, list }) => {
  if (isListFetching) {
    return <Loading />
  }

  return (
    <Table>
      <TableHead>
        <TableRow>
          <TableCell>Name</TableCell>
          <TableCell style={{ width: 140 }}>Country</TableCell>
          <TableCell style={{ width: 80 }}>Start date</TableCell>
          <TableCell style={{ width: 130 }}>Data owner</TableCell>
          <TableCell style={{ width: 130 }}>Technical advisor</TableCell>
          <TableCell style={{ width: 100 }} />
        </TableRow>
      </TableHead>
      <TableBody>
        {list.map(row => (
          <TableRow key={row.id} hover onClick={() => goToDashboard(row.id)} className={styles.clickableRow}>
            <TableCell>{row.name}</TableCell>
            <TableCell>{row.country}</TableCell>
            <TableCell>{dayjs(row.startDate).format("YYYY-MM-DD")}</TableCell>
            <TableCell>{row.dataOwner}</TableCell>
            <TableCell>{row.technicalAdvisor}</TableCell>
            <TableCell style={{ textAlign: "right", paddingTop: 0, paddingBottom: 0 }}>
              <TableRowAction onClick={() => goToEdition(row.id)} icon={<EditIcon />} title={"Edit"} />
              <TableRowAction onClick={() => remove(row.id)} icon={<ClearIcon />} title={"Delete"} isFetching={isRemoving[row.id]} />
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

NationalSocietiesTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default NationalSocietiesTable;