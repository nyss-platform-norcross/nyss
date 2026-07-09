import tableStyles from "../common/table/Table.module.scss";
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

export const ProjectAlertRecipientsTable = ({
  isListFetching,
  isRemoving,
  goToEdition,
  remove,
  list,
  projectId,
  isClosed,
  rtl,
}) => {
  if (isListFetching) {
    return <Loading />;
  }

  const renderHealthRisks = (healthRisks) => {
    if (healthRisks.length > 0) {
      return healthRisks.length > 1
        ? `${healthRisks[0].healthRiskName} (+ ${healthRisks.length - 1})`
        : `${healthRisks[0].healthRiskName}`;
    }

    return "Any";
  };

  const renderSupervisors = (supervisors) => {
    if (supervisors.length > 0) {
      return supervisors.length > 1
        ? `${supervisors[0].name} (+ ${supervisors.length - 1})`
        : `${supervisors[0].name}`;
    }

    return "Any";
  };

  return (
    <TableContainer>
      <Table stickyHeader>
        <TableHead>
          <TableRow>
            <TableCell>
              {strings(stringKeys.projectAlertRecipient.list.role)}
            </TableCell>
            <TableCell>
              {strings(stringKeys.projectAlertRecipient.list.organization)}
            </TableCell>
            <TableCell>{strings(stringKeys.common.email)}</TableCell>
            <TableCell>
              {strings(stringKeys.projectAlertRecipient.list.phoneNumber)}
            </TableCell>
            <TableCell>
              {strings(stringKeys.projectAlertRecipient.list.healthRisks)}
            </TableCell>
            <TableCell>
              {strings(stringKeys.projectAlertRecipient.list.supervisors)}
            </TableCell>
            <TableCell style={{ width: "10%" }} />
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map((row) => (
            <TableRow
              key={row.id}
              onClick={() => !isClosed && goToEdition(projectId, row.id)}
              hover={!isClosed}
              className={!isClosed ? tableStyles.clickableRow : null}
            >
              <TableCell>{row.role}</TableCell>
              <TableCell>{row.organization}</TableCell>
              <TableCell>{row.email}</TableCell>
              <TableCell className={rtl ? "ltr-numerals" : ""}>
                {row.phoneNumber}
              </TableCell>
              <TableCell>{renderHealthRisks(row.healthRisks)}</TableCell>
              <TableCell>
                {renderSupervisors([
                  ...row.supervisors,
                  ...row.headSupervisors,
                ])}
              </TableCell>
              <TableCell>
                {!isClosed && (
                  <TableRowActions directionRtl={rtl}>
                    <TableRowAction
                      directionRtl={rtl}
                      onClick={() => goToEdition(projectId, row.id)}
                      icon={<EditIcon />}
                      title={"Edit"}
                    />
                    <TableRowAction
                      directionRtl={rtl}
                      onClick={() => remove(projectId, row.id)}
                      confirmationText={strings(
                        stringKeys.projectAlertRecipient.list
                          .removalConfirmation,
                      )}
                      icon={<ClearIcon />}
                      title={"Delete"}
                      isFetching={isRemoving[row.id]}
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

ProjectAlertRecipientsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

export default ProjectAlertRecipientsTable;
