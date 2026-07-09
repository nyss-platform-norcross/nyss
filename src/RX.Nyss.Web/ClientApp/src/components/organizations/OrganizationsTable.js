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
import { strings, stringKeys } from "../../strings";
import { TableRowActions } from "../common/tableRowAction/TableRowActions";

export const OrganizationsTable = ({
  isListFetching,
  isRemoving,
  goToEdition,
  remove,
  list,
  nationalSocietyId,
  canModify,
  rtl,
}) => {
  if (isListFetching) {
    return <Loading />;
  }

  const showDefaultFlag = (row) =>
    list.length > 1 && row.isDefaultOrganization === true;

  return (
    <TableContainer>
      <Table stickyHeader>
        <TableHead>
          <TableRow>
            <TableCell>{strings(stringKeys.common.name)}</TableCell>
            <TableCell style={{ width: "20%", minWidth: 75 }}>
              {strings(stringKeys.organization.list.projects)}
            </TableCell>
            <TableCell style={{ width: "20%", minWidth: 75 }}>
              {strings(stringKeys.organization.list.headManager)}
            </TableCell>
            {canModify && <TableCell style={{ width: "10%" }} />}
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map((row) => (
            <TableRow
              key={row.id}
              hover={canModify}
              onClick={() =>
                canModify && goToEdition(nationalSocietyId, row.id)
              }
              className={canModify ? styles.clickableRow : null}
            >
              <TableCell>
                {row.name}{" "}
                {showDefaultFlag(row) &&
                  strings(stringKeys.organization.list.isDefaultOrganization)}
              </TableCell>
              <TableCell>{row.projects}</TableCell>
              <TableCell>{row.headManager}</TableCell>
              {canModify && (
                <TableCell>
                  <TableRowActions directionRtl={rtl}>
                    <TableRowAction
                      directionRtl={rtl}
                      onClick={() => goToEdition(nationalSocietyId, row.id)}
                      icon={<EditIcon />}
                      title={"Edit"}
                    />
                    <TableRowAction
                      directionRtl={rtl}
                      onClick={() => remove(nationalSocietyId, row.id)}
                      confirmationText={strings(
                        stringKeys.organization.list.removalConfirmation,
                      )}
                      icon={<ClearIcon />}
                      title={"Delete"}
                      isFetching={isRemoving[row.id]}
                    />
                  </TableRowActions>
                </TableCell>
              )}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

OrganizationsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

export default OrganizationsTable;
