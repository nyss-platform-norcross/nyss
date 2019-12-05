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
import { strings, stringKeys } from '../../strings';
import { accessMap } from '../../authentication/accessMap';
import { HasAccess } from '../common/hasAccess/HasAccess';
import { hslToRgb } from '@material-ui/core/styles';

export const NationalSocietiesTable = ({ isListFetching, isRemoving, goToEdition, goToDashboard, remove, list }) => {
  if (isListFetching) {
    return <Loading />
  }

  return (
    <Table>
      <TableHead>
        <TableRow>
          <TableCell>{strings(stringKeys.nationalSociety.list.name)}</TableCell>
          <TableCell style={{ width: "16%", minWidth: 100 }}>{strings(stringKeys.nationalSociety.list.country)}</TableCell>
          <TableCell style={{ width: "8%", minWidth: 75 }}>{strings(stringKeys.nationalSociety.list.startDate)}</TableCell>
          <TableCell style={{ width: "16%" }}>{strings(stringKeys.nationalSociety.list.headManager)}</TableCell>
          <TableCell style={{ width: "16%" }}>{strings(stringKeys.nationalSociety.list.technicalAdvisor)}</TableCell>
          <TableCell style={{ width: "16%", minWidth: 75 }} />
        </TableRow>
      </TableHead>
      <TableBody>
        {list.map(row => (
          <TableRow key={row.id} hover onClick={() => goToDashboard(row.id)} className={styles.clickableRow}>
            <TableCell>{row.name}</TableCell>
            <TableCell>{row.country}</TableCell>
            <TableCell>{dayjs(row.startDate).format("YYYY-MM-DD")}</TableCell>
            <TableCell>{row.headManagerName}</TableCell>
            <TableCell>{row.technicalAdvisor}</TableCell>
            <TableCell style={{ textAlign: "right", paddingTop: 0, paddingBottom: 0 }}>
              <HasAccess roles={accessMap.nationalSocieties.edit}>
                <TableRowAction onClick={() => goToEdition(row.id)} icon={<EditIcon />} title={"Edit"} />
              </HasAccess>
              <HasAccess roles={accessMap.nationalSocieties.delete}>
                <TableRowAction onClick={() => remove(row.id)} confirmationText={strings(stringKeys.nationalSociety.list.removalConfirmation)} icon={<ClearIcon />} title={"Delete"} isFetching={isRemoving[row.id]} />
              </HasAccess>
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
