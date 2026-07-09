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
import { TableRowAction } from "../common/tableRowAction/TableRowAction";
import { Loading } from "../common/loading/Loading";
import { strings, stringKeys } from "../../strings";
import { TableRowActions } from "../common/tableRowAction/TableRowActions";
import { accessMap } from "../../authentication/accessMap";

export const ProjectOrganizationsTable = ({
  isListFetching,
  isRemoving,
  remove,
  list,
  projectId,
  isClosed,
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
            <TableCell>
              {strings(stringKeys.projectOrganization.list.name)}
            </TableCell>
            <TableCell style={{ width: "10%" }} />
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map((row) => (
            <TableRow key={row.id} hover>
              <TableCell>
                {row.name}{" "}
                {showDefaultFlag(row) &&
                  strings(stringKeys.organization.list.isDefaultOrganization)}
              </TableCell>
              <TableCell>
                {!isClosed && (
                  <TableRowActions directionRtl={rtl}>
                    <TableRowAction
                      roles={accessMap.projectOrganizations.delete}
                      onClick={() => remove(projectId, row.id)}
                      confirmationText={strings(
                        stringKeys.projectOrganization.list.removalConfirmation,
                      )}
                      icon={<ClearIcon />}
                      title={"Delete"}
                      isFetching={isRemoving[row.id]}
                      directionRtl={rtl}
                    />
                  </TableRowActions>
                )}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

ProjectOrganizationsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

export default ProjectOrganizationsTable;
