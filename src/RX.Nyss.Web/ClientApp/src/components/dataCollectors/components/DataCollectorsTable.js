import styles from "./DataCollectorsTable.module.scss";
import tableStyles from "../../common/table/Table.module.scss";
import { useEffect, useState, useMemo, useCallback } from "react";
import PropTypes from "prop-types";
import ClearIcon from "@material-ui/icons/Clear";
import EditIcon from "@material-ui/icons/Edit";
import MoreVertIcon from "@material-ui/icons/MoreVert";
import { TableRowAction } from "../../common/tableRowAction/TableRowAction";
import { strings, stringKeys } from "../../../strings";
import { TableRowMenu } from "../../common/tableRowAction/TableRowMenu";
import { TableRowActions } from "../../common/tableRowAction/TableRowActions";
import { accessMap } from "../../../authentication/accessMap";
import {
  trainingStatusInTraining,
  trainingStatusTrained,
} from "../logic/dataCollectorsConstants";
import {
  Checkbox,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableContainer,
} from "@material-ui/core";
import TablePager from "../../common/tablePagination/TablePager";
import { Loading } from "../../common/loading/Loading";

export const DataCollectorsTable = ({
  isListFetching,
  listSelectedAll,
  isRemoving,
  goToEdition,
  remove,
  list,
  page,
  rowsPerPage,
  totalRows,
  projectId,
  setTrainingState,
  isUpdatingDataCollector,
  selectDataCollector,
  selectAllDataCollectors,
  replaceSupervisor,
  onChangePage,
  setDeployedState,
  rtl,
}) => {
  const [isSelected, setIsSelected] = useState(false);
  useEffect(() => setIsSelected(list.some((i) => i.isSelected)), [list]);

  const getRowMenu = (row) => [
    {
      title: strings(stringKeys.dataCollectors.list.takeOutOfTraining),
      action: () => setTrainingState([row.id], false),
      disabled: !row.isInTrainingMode,
      roles: accessMap.dataCollectors.list,
    },
    {
      title: strings(stringKeys.dataCollectors.list.setToInTraining),
      action: () => setTrainingState([row.id], true),
      disabled: row.isInTrainingMode,
      roles: accessMap.dataCollectors.list,
    },
    {
      title: strings(stringKeys.dataCollectors.list.setToDeployed),
      action: () => setDeployedState([row.id], true),
      disabled: row.isDeployed,
      roles: accessMap.dataCollectors.list,
    },
    {
      title: strings(stringKeys.dataCollectors.list.setToNotDeployed),
      action: () => setDeployedState([row.id], false),
      disabled: !row.isDeployed,
      roles: accessMap.dataCollectors.list,
    },
  ];

  const getSelectedIds = useCallback(
    () => list.filter((i) => i.isSelected).map((i) => i.id),
    [list],
  );

  const getSelectedDataCollectors = useCallback(
    () =>
      list.filter((i) => i.isSelected).map((i) => ({ name: i.name, id: i.id })),
    [list],
  );

  const multipleSelectionMenu = useMemo(
    () => [
      {
        title: strings(stringKeys.dataCollectors.list.takeOutOfTraining),
        action: () => setTrainingState(getSelectedIds(), false),
        roles: accessMap.dataCollectors.list,
      },
      {
        title: strings(stringKeys.dataCollectors.list.setToInTraining),
        action: () => setTrainingState(getSelectedIds(), true),
        roles: accessMap.dataCollectors.list,
      },
      {
        title: strings(stringKeys.dataCollectors.list.replaceSupervisor),
        action: () => replaceSupervisor(getSelectedDataCollectors()),
        roles: accessMap.dataCollectors.replaceSupervisor,
      },
      {
        title: strings(stringKeys.dataCollectors.list.setToDeployed),
        action: () => setDeployedState(getSelectedIds(), true),
        roles: accessMap.dataCollectors.list,
      },
      {
        title: strings(stringKeys.dataCollectors.list.setToNotDeployed),
        action: () => setDeployedState(getSelectedIds(), false),
        roles: accessMap.dataCollectors.list,
      },
    ],
    [
      getSelectedIds,
      getSelectedDataCollectors,
      setTrainingState,
      replaceSupervisor,
      setDeployedState,
    ],
  );

  const handleSelect = (e, id, value) => {
    e.stopPropagation();
    selectDataCollector(id, value);
  };

  const handleSelectAll = (e) => {
    e.stopPropagation();
    selectAllDataCollectors(!listSelectedAll);
  };

  const renderLocation = (locations) => {
    const firstLocation = locations[0];
    if (locations.length > 1) {
      return `${firstLocation.region}, ${firstLocation.district}, ${
        firstLocation.village
      } (+ ${locations.length - 1})`;
    }

    return `${firstLocation.region}, ${firstLocation.district}, ${firstLocation.village}`;
  };

  if (isListFetching) {
    return <Loading />;
  }

  return (
    <TableContainer>
      <Table stickyHeader>
        <TableHead>
          <TableRow>
            <TableCell className={styles.checkCell}>
              <Checkbox
                onClick={handleSelectAll}
                checked={listSelectedAll}
                color="primary"
              />
            </TableCell>
            <TableCell>
              {strings(stringKeys.dataCollectors.list.dataCollectorType)}
            </TableCell>
            <TableCell>{strings(stringKeys.common.name)}</TableCell>
            <TableCell>
              {strings(stringKeys.dataCollectors.list.displayName)}
            </TableCell>
            <TableCell>
              {strings(stringKeys.dataCollectors.list.phoneNumber)}
            </TableCell>
            <TableCell>{strings(stringKeys.dataCollectors.list.sex)}</TableCell>
            <TableCell>{strings(stringKeys.common.location)}</TableCell>
            <TableCell>
              {strings(stringKeys.dataCollectors.list.trainingStatus)}
            </TableCell>
            <TableCell>
              {strings(stringKeys.dataCollectors.list.supervisor)}
            </TableCell>
            <TableCell>
              {isSelected && (
                <TableRowActions directionRtl={rtl} style={{ marginRight: 70 }}>
                  <TableRowMenu
                    directionRtl={rtl}
                    icon={<MoreVertIcon />}
                    items={multipleSelectionMenu}
                    alwaysHighlighted
                  />
                </TableRowActions>
              )}
            </TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map((row) => (
            <TableRow
              key={row.id}
              hover
              onClick={() => goToEdition(projectId, row.id)}
              className={tableStyles.clickableRow}
            >
              <TableCell className={styles.checkCell}>
                <Checkbox
                  checked={!!row.isSelected}
                  onClick={(e) => handleSelect(e, row.id, !row.isSelected)}
                  color="primary"
                />
              </TableCell>
              <TableCell>
                {strings(
                  stringKeys.dataCollectors.constants.dataCollectorType[
                    row.dataCollectorType
                  ],
                )}
              </TableCell>
              <TableCell>{row.name}</TableCell>
              <TableCell>{row.displayName}</TableCell>
              <TableCell className={rtl ? "ltr-numerals" : ""}>
                {row.phoneNumber}
              </TableCell>
              <TableCell>{row.sex}</TableCell>
              <TableCell>{renderLocation(row.locations)}</TableCell>
              <TableCell>
                {row.isInTrainingMode
                  ? strings(
                      stringKeys.dataCollectors.constants.trainingStatus[
                        trainingStatusInTraining
                      ],
                    )
                  : strings(
                      stringKeys.dataCollectors.constants.trainingStatus[
                        trainingStatusTrained
                      ],
                    )}
              </TableCell>
              <TableCell>{row.supervisor.name}</TableCell>
              <TableCell>
                <TableRowActions directionRtl={rtl}>
                  <TableRowMenu
                    directionRtl={rtl}
                    id={row.id}
                    items={getRowMenu(row)}
                    icon={<MoreVertIcon />}
                    isFetching={isUpdatingDataCollector[row.id]}
                  />
                  <TableRowAction
                    directionRtl={rtl}
                    onClick={() => goToEdition(projectId, row.id)}
                    icon={<EditIcon />}
                    title={"Edit"}
                  />
                  <TableRowAction
                    directionRtl={rtl}
                    onClick={() => remove(row.id)}
                    confirmationText={strings(
                      stringKeys.dataCollectors.list.removalConfirmation,
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
      {!!list.length && (
        <TablePager
          totalRows={totalRows}
          rowsPerPage={rowsPerPage}
          page={page}
          onChangePage={onChangePage}
          rtl={rtl}
        />
      )}
    </TableContainer>
  );
};

DataCollectorsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

export default DataCollectorsTable;
