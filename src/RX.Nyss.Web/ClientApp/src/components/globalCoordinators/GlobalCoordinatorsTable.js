import styles from "../common/table/Table.module.scss";
import React from "react";
import PropTypes from "prop-types";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableContainer
} from "@material-ui/core";
import ClearIcon from "@material-ui/icons/Clear";
import EditIcon from "@material-ui/icons/Edit";
import { TableRowAction } from "../common/tableRowAction/TableRowAction";
import { Loading } from "../common/loading/Loading";
import { stringKeys, strings } from "../../strings";
import { accessMap } from "../../authentication/accessMap";
import { TableRowActions } from "../common/tableRowAction/TableRowActions";

export const GlobalCoordinatorsTable = ({
  isListFetching,
  isRemoving,
  goToEdition,
  remove,
  list,
  rtl,
}) => {
  if (isListFetching) {
    return <Loading />;
  }

  return (
    <TableContainer>
      <Table stickyHeader>
        <TableHead>
          <TableRow>
            <TableCell>{strings(stringKeys.common.name)}</TableCell>
            <TableCell style={{ width: "25%", minWidth: 100 }}>
              {strings(stringKeys.common.email)}
            </TableCell>
            <TableCell style={{ width: "8%", minWidth: 75 }}>
              {strings(stringKeys.globalCoordinator.list.phoneNumber)}
            </TableCell>
            <TableCell style={{ width: "16%" }}>
              {strings(stringKeys.globalCoordinator.list.organization)}
            </TableCell>
            <TableCell style={{ width: "16%" }} />
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map((row) => (
            <TableRow
              key={row.id}
              hover
              onClick={() => goToEdition(row.id)}
              className={styles.clickableRow}
            >
              <TableCell>{row.name}</TableCell>
              <TableCell>{row.email}</TableCell>
              <TableCell>{row.phoneNumber}</TableCell>
              <TableCell>{row.organization}</TableCell>
              <TableCell>
                <TableRowActions directionRtl={rtl}>
                  <TableRowAction
                    directionRtl={rtl}
                    roles={accessMap.globalCoordinators.edit}
                    onClick={() => goToEdition(row.id)}
                    icon={<EditIcon />}
                    title={"Edit"}
                  />
                  <TableRowAction
                    directionRtl={rtl}
                    roles={accessMap.globalCoordinators.delete}
                    onClick={() => remove(row.id)}
                    confirmationText={strings(
                      stringKeys.globalCoordinator.list.removalConfirmation,
                    )}
                    icon={<ClearIcon />}
                    title={"Delete"}
                    isFetching={isRemoving[row.id]}
                  />
                </TableRowActions>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

GlobalCoordinatorsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

export default GlobalCoordinatorsTable;
