import styles from '../common/table/Table.module.scss';
import React, { useState, Fragment } from 'react';
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
import { TableContainer } from '../common/table/TableContainer';
import { TableRowActions } from '../common/tableRowAction/TableRowActions';
import { TableRowMenu } from '../common/tableRowAction/TableRowMenu';
import MoreVertIcon from '@material-ui/icons/MoreVert';
import { ConfirmationDialog } from '../common/confirmationDialog/ConfirmationDialog';

export const NationalSocietiesTable = ({ isListFetching, isRemoving, goToEdition, goToDashboard, remove, list, archive, reopen, isArchiving, isReopening }) => {

  const [archiveConfirmationDialog, setArchiveConfirmationDialog] = useState({ isOpen: false, nationalSocietyId: null });
  const [reopenConfirmationDialog, setReopenConfirmationDialog] = useState({ isOpen: false, nationalSocietyId: null });

  const archiveConfirmed = () => {
    archive(archiveConfirmationDialog.nationalSocietyId);
    setArchiveConfirmationDialog({ isOpen: false })
  }

  const reopenConfirmed = () => {
    reopen(reopenConfirmationDialog.nationalSocietyId);
    setReopenConfirmationDialog({ isOpen: false })
  }
 
  if (isListFetching) {
    return <Loading />
  }

  return (
    <Fragment>
      <TableContainer>
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
              <TableRow key={row.id} hover onClick={() => goToDashboard(row.id)} className={row.isArchived ? styles.inactiveRow : styles.clickableRow} >
                <TableCell>{row.name}</TableCell>
                <TableCell>{row.country}</TableCell>
                <TableCell>{dayjs(row.startDate).format("YYYY-MM-DD")}</TableCell>
                <TableCell>{row.headManagerName}</TableCell>
                <TableCell>{row.technicalAdvisor}</TableCell>
                <TableCell>
                  <TableRowActions>                    
                    <TableRowMenu
                      id={row.id}                      
                      items={[{
                        id: `menuItem_${row.id}_1`,
                        title: strings(stringKeys.nationalSociety.list.archive),
                        condition: !row.isArchived,
                        roles: accessMap.nationalSocieties.archive,
                        action: () => setArchiveConfirmationDialog({ isOpen: true, nationalSocietyId: row.id })
                      },
                      {
                        id: `menuItem_${row.id}_2`,
                        title: strings(stringKeys.nationalSociety.list.reopen),
                        condition: row.isArchived,
                        roles: accessMap.nationalSocieties.archive,
                        action: () => setReopenConfirmationDialog({ isOpen: true, nationalSocietyId: row.id })
                      }
                      ]}
                      icon={<MoreVertIcon />}
                      isFetching={isArchiving[row.id] || isReopening[row.id]}
                    />
                    <TableRowAction roles={accessMap.nationalSocieties.edit} onClick={() => goToEdition(row.id)} icon={<EditIcon />} title={"Edit"} />
                    <TableRowAction roles={accessMap.nationalSocieties.delete} onClick={() => remove(row.id)} confirmationText={strings(stringKeys.nationalSociety.list.removalConfirmation)} icon={<ClearIcon />} title={"Delete"} isFetching={isRemoving[row.id]} />
                  </TableRowActions>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <ConfirmationDialog
        isOpened={archiveConfirmationDialog.isOpen}
        titlteText={strings(stringKeys.nationalSociety.archive.title)}
        submit={() => archiveConfirmed()}
        close={() => setArchiveConfirmationDialog({ isOpen: false })}
        contentText = {strings(stringKeys.nationalSociety.archive.content)}
      />
      
      <ConfirmationDialog
        isOpened={reopenConfirmationDialog.isOpen}
        titlteText={strings(stringKeys.nationalSociety.reopen.title)}
        submit={() => reopenConfirmed()}
        close={() => setReopenConfirmationDialog({ isOpen: false })}
        contentText={strings(stringKeys.nationalSociety.reopen.content)}
      />     
    </Fragment>
  );
}

NationalSocietiesTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default NationalSocietiesTable;
