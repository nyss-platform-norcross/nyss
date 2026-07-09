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
import { accessMap } from "../../authentication/accessMap";
import * as roles from "../../authentication/roles";

export const SmsGatewaysTable = ({
  isListFetching,
  isRemoving,
  goToEdition,
  remove,
  list,
  nationalSocietyId,
  callingUserRoles,
  rtl,
}) => {
  if (isListFetching) {
    return <Loading />;
  }

  const canModify =
    callingUserRoles.some(
      (r) => r === roles.Administrator,
    );

  return (
    <TableContainer>
      <Table stickyHeader>
        <TableHead>
          <TableRow>
            <TableCell>{strings(stringKeys.common.name)}</TableCell>
            <TableCell style={{ width: "30%", minWidth: 100 }}>
              {strings(stringKeys.smsGateway.list.apiKey)}
            </TableCell>
            <TableCell style={{ width: "20%", minWidth: 75 }}>
              {strings(stringKeys.smsGateway.list.gatewayType)}
            </TableCell>
            <TableCell style={{ width: "25%", minWidth: 100 }}>
              {strings(stringKeys.smsGateway.list.useIotHub)}
            </TableCell>
            <TableCell style={{ width: "10%" }} />
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map((row) => (
            <TableRow
              key={row.id}
              hover
              onClick={
                canModify ? () => goToEdition(nationalSocietyId, row.id) : null
              }
              className={canModify ? styles.clickableRow : null}
            >
              <TableCell>{row.name}</TableCell>
              <TableCell>{row.apiKey}</TableCell>
              <TableCell>
                {strings(`smsGateway.type.${row.gatewayType.toLowerCase()}`)}
              </TableCell>
              <TableCell>{row.iotHubDeviceName}</TableCell>
              <TableCell>
                <TableRowActions directionRtl={rtl}>
                  <TableRowAction
                    onClick={() => goToEdition(nationalSocietyId, row.id)}
                    icon={<EditIcon />}
                    roles={accessMap.smsGateways.edit}
                    condition={canModify}
                    title="Edit"
                    directionRtl={rtl}
                  />
                  <TableRowAction
                    onClick={() => remove(nationalSocietyId, row.id)}
                    confirmationText={strings(
                      stringKeys.smsGateway.list.removalConfirmation,
                    )}
                    icon={<ClearIcon />}
                    title="Delete"
                    roles={accessMap.smsGateways.delete}
                    condition={canModify}
                    isFetching={isRemoving[row.id]}
                    directionRtl={rtl}
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

SmsGatewaysTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

export default SmsGatewaysTable;
