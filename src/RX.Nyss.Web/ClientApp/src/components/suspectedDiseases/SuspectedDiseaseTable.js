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
import { TableRowActions } from "../common/tableRowAction/TableRowActions";

export const SuspectedDiseaseTable = ({
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
            <TableCell style={{ width: "10%", minWidth: 100 }}>
              {strings(stringKeys.suspectedDisease.list.suspectedDiseaseCode)}
            </TableCell>
            <TableCell>
              {strings(stringKeys.suspectedDisease.list.suspectedDiseaseName)}
            </TableCell>
            <TableCell style={{ width: "25%" }} />
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map((row) => (
            <TableRow
              key={row.suspectedDiseaseId}
              hover
              onClick={() => goToEdition(row.suspectedDiseaseId)}
              className={styles.clickableRow}
            >
              <TableCell>{row.suspectedDiseaseCode}</TableCell>
              <TableCell>{row.suspectedDiseaseName}</TableCell>
              <TableCell>
                <TableRowActions directionRtl={rtl}>
                  <TableRowAction
                    directionRtl={rtl}
                    onClick={() => goToEdition(row.suspectedDiseaseId)}
                    icon={<EditIcon />}
                    title={"Edit"}
                  />
                  <TableRowAction
                    directionRtl={rtl}
                    onClick={() => remove(row.suspectedDiseaseId)}
                    confirmationText={strings(
                      stringKeys.suspectedDisease.list.removalConfirmation,
                    )}
                    icon={<ClearIcon />}
                    title={"Delete"}
                    isFetching={isRemoving[row.suspectedDiseaseId]}
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

SuspectedDiseaseTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

export default SuspectedDiseaseTable;
