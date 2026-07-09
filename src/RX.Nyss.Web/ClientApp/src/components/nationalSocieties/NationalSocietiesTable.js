import styles from "../common/table/Table.module.scss";
import React, { useState, Fragment } from "react";
import PropTypes from "prop-types";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableContainer,
  makeStyles,
} from "@material-ui/core";
import EditIcon from "@material-ui/icons/Edit";
import dayjs from "dayjs";
import { TableRowAction } from "../common/tableRowAction/TableRowAction";
import { Loading } from "../common/loading/Loading";
import { strings, stringKeys } from "../../strings";
import { accessMap } from "../../authentication/accessMap";
import { TableRowActions } from "../common/tableRowAction/TableRowActions";
import { TableRowMenu } from "../common/tableRowAction/TableRowMenu";
import MoreVertIcon from "@material-ui/icons/MoreVert";
import { ConfirmationDialog } from "../common/confirmationDialog/ConfirmationDialog";

const useStyles = makeStyles((theme) => ({
  clickableRow: {
    cursor: "pointer",
  },
  inactiveRow: {
    "& .MuiTableCell-body": {
      color: theme.palette.text.disabled,
    },
  },
}));

export const NationalSocietiesTable = ({
  isListFetching,
  goToEdition,
  goToDashboard,
  list,
  archive,
  reopen,
  isArchiving,
  isReopening,
  rtl,
}) => {
  const [archiveConfirmationDialog, setArchiveConfirmationDialog] = useState({
    isOpen: false,
    nationalSocietyId: null,
  });
  const [reopenConfirmationDialog, setReopenConfirmationDialog] = useState({
    isOpen: false,
    nationalSocietyId: null,
  });

  const classes = useStyles();

  const archiveConfirmed = () => {
    archive(archiveConfirmationDialog.nationalSocietyId);
    setArchiveConfirmationDialog({ isOpen: false });
  };

  const reopenConfirmed = () => {
    reopen(reopenConfirmationDialog.nationalSocietyId);
    setReopenConfirmationDialog({ isOpen: false });
  };

  if (isListFetching) {
    return <Loading />;
  }

  const getRowMenu = (row) => [
    {
      title: strings(stringKeys.nationalSociety.list.archive),
      disabled: row.isArchived,
      roles: accessMap.nationalSocieties.archive,
      action: () =>
        setArchiveConfirmationDialog({
          isOpen: true,
          nationalSocietyId: row.id,
        }),
    },
    {
      title: strings(stringKeys.nationalSociety.list.reopen),
      disabled: !row.isArchived,
      roles: accessMap.nationalSocieties.archive,
      action: () =>
        setReopenConfirmationDialog({
          isOpen: true,
          nationalSocietyId: row.id,
        }),
    },
  ];

  return (
    <Fragment>
      <TableContainer>
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell>{strings(stringKeys.common.name)}</TableCell>
              <TableCell style={{ width: "16%", minWidth: 100 }}>
                {strings(stringKeys.nationalSociety.list.country)}
              </TableCell>
              <TableCell style={{ width: "8%", minWidth: 75 }}>
                {strings(stringKeys.nationalSociety.list.startDate)}
              </TableCell>
              <TableCell style={{ width: "16%" }}>
                {strings(stringKeys.nationalSociety.list.coordinator)}
              </TableCell>
              <TableCell style={{ width: "16%" }}>
                {strings(stringKeys.nationalSociety.list.headManager)}
              </TableCell>
              <TableCell style={{ width: "16%" }}>
                {strings(stringKeys.nationalSociety.list.technicalAdvisor)}
              </TableCell>
              <TableCell style={{ width: "16%", minWidth: 75 }} />
            </TableRow>
          </TableHead>
          <TableBody>
            {list.map((row) => (
              <TableRow
                key={row.id}
                hover
                onClick={() => goToDashboard(row.id)}
                className={`${classes.clickableRow} ${
                  row.isArchived ? classes.inactiveRow : ""
                }`}
              >
                <TableCell>{row.name}</TableCell>
                <TableCell>{row.country}</TableCell>
                <TableCell>
                  {dayjs(row.startDate).format("YYYY-MM-DD")}
                </TableCell>
                <TableCell>{row.coordinators}</TableCell>
                <TableCell>{row.headManagers}</TableCell>
                <TableCell>{row.technicalAdvisor}</TableCell>
                <TableCell>
                  <TableRowActions directionRtl={rtl}>
                    <TableRowAction
                      directionRtl={rtl}
                      roles={accessMap.nationalSocieties.edit}
                      onClick={() => goToEdition(row.id)}
                      icon={<EditIcon />}
                      title={"Edit"}
                    />
                    <TableRowMenu
                      directionRtl={rtl}
                      id={row.id}
                      items={getRowMenu(row)}
                      icon={<MoreVertIcon />}
                      isFetching={isArchiving[row.id] || isReopening[row.id]}
                    />
                  </TableRowActions>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <ConfirmationDialog
        isOpened={archiveConfirmationDialog.isOpen}
        titleText={strings(stringKeys.nationalSociety.archive.title)}
        submit={() => archiveConfirmed()}
        close={() => setArchiveConfirmationDialog({ isOpen: false })}
        contentText={strings(stringKeys.nationalSociety.archive.content)}
      />

      <ConfirmationDialog
        isOpened={reopenConfirmationDialog.isOpen}
        titleText={strings(stringKeys.nationalSociety.reopen.title)}
        submit={() => reopenConfirmed()}
        close={() => setReopenConfirmationDialog({ isOpen: false })}
        contentText={strings(stringKeys.nationalSociety.reopen.content)}
      />
    </Fragment>
  );
};

NationalSocietiesTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

export default NationalSocietiesTable;
