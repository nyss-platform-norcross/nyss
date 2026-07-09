import styles from "../common/table/Table.module.scss";
import React from "react";
import PropTypes from "prop-types";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  useTheme,
  useMediaQuery,
  TableContainer
} from "@material-ui/core";
import ClearIcon from "@material-ui/icons/Clear";
import EditIcon from "@material-ui/icons/Edit";
import { TableRowAction } from "../common/tableRowAction/TableRowAction";
import { Loading } from "../common/loading/Loading";
import { stringKeys, strings } from "../../strings";
import { TableRowActions } from "../common/tableRowAction/TableRowActions";

export const HealthRisksTable = ({
  isListFetching,
  isRemoving,
  goToEdition,
  remove,
  list,
  rtl,
}) => {
  const theme = useTheme();
  const isSmallScreen = useMediaQuery(theme.breakpoints.down("sm"));
  
  if (isListFetching) {
    return <Loading />;
  }
  return (
    <TableContainer>
      <Table stickyHeader>
        <TableHead>
          <TableRow>
            <TableCell style={{ width: "10%", minWidth: 100 }}>
              {strings(stringKeys.healthRisk.list.healthRiskCode)}
            </TableCell>
            <TableCell>{strings(stringKeys.healthRisk.list.name)}</TableCell>
            <TableCell style={{ width: "25%", minWidth: 100 }}>
              {strings(stringKeys.healthRisk.list.healthRiskType)}
            </TableCell>
            <TableCell style={{ width: "25%" }} />
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map((row) => (
            <TableRow
              key={row.id}
              hover
              onClick={() => goToEdition(row.id)}
              className={`${styles.clickableRow} ${isSmallScreen ? styles.smallDeviceTableRow : ""}`}
            >
              <TableCell>{row.healthRiskCode}</TableCell>
              <TableCell>{row.name}</TableCell>
              <TableCell>
                {strings(
                  stringKeys.healthRisk.constants.healthRiskType[
                    row.healthRiskType.toLowerCase()
                  ],
                )}
              </TableCell>
              <TableCell>
                <TableRowActions directionRtl={rtl}>
                  <TableRowAction
                    directionRtl={rtl}
                    onClick={() => goToEdition(row.id)}
                    icon={<EditIcon />}
                    title={"Edit"}
                  />
                  <TableRowAction
                    directionRtl={rtl}
                    onClick={() => remove(row.id)}
                    confirmationText={strings(
                      stringKeys.healthRisk.list.removalConfirmation,
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

HealthRisksTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

export default HealthRisksTable;
