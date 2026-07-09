import styles from "./ReportsTable.module.scss";
import reportsTableStyles from "./ReportsTable.module.scss";
import { Fragment, useEffect, useRef, useState } from "react";
import PropTypes from "prop-types";
import { Loading } from "../common/loading/Loading";
import { strings, stringKeys } from "../../strings";
import dayjs from "dayjs";
import TablePager from "../common/tablePagination/TablePager";
import {
  DateColumnName,
  ReportErrorType,
  reportDetailedFormatErrors,
} from "./logic/reportsConstants";
import {
  Typography,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TableSortLabel,
  TableContainer
} from "@material-ui/core";
import CorrectedButton from "./CorrectedButton";
import { renderDataCollectorDisplayName } from "./logic/reportsService";

export const IncorrectReportsTable = ({
  isListFetching,
  list,
  page,
  onChangePage,
  rowsPerPage,
  totalRows,
  filters,
  sorting,
  onSort,
  onCorrectToggle,
  rtl,
}) => {
  const [value, setValue] = useState(sorting);
  const tableContainerRef = useRef(null);

  useEffect(() => {
    tableContainerRef.current?.scrollTo({ top: 0, behavior: "auto" });
  }, [page]);

  const updateValue = (change) => {
    const newValue = {
      ...value,
      ...change,
    };

    setValue(newValue);
    return newValue;
  };

  const dashIfEmpty = (text, ...args) => {
    return [text || "-", ...args].filter((x) => !!x).join(", ");
  };

  const createSortHandler = (column) => (event) => {
    handleSortChange(event, column);
  };

  const handleSortChange = (event, column) => {
    const isAscending = sorting.orderBy === column && sorting.sortAscending;
    onSort(updateValue({ orderBy: column, sortAscending: !isAscending }));
  };

  const handlePageChange = (event, page) => {
    tableContainerRef.current?.scrollTo({ top: 0, behavior: "auto" });
    onChangePage(page);
  };

  const errorTypeString = (errorType) => {
    const errorTypeTranslation = strings(
      stringKeys.reports.errorTypes[errorType],
    );

    if (reportDetailedFormatErrors.some((e) => e === errorType)) {
      return (
        <span>
          {strings(stringKeys.reports.errorTypes[ReportErrorType.formatError])}
          <br />
          {errorTypeTranslation}
        </span>
      );
    } else {
      return errorTypeTranslation;
    }
  };

  return (
    <Fragment>
      <TableContainer ref={tableContainerRef}>
        {isListFetching && <Loading absolute />}
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell style={{ width: "6%", minWidth: "80px" }}>
                <TableSortLabel
                  active={sorting.orderBy === DateColumnName}
                  direction={sorting.sortAscending ? "asc" : "desc"}
                  onClick={createSortHandler(DateColumnName)}
                >
                  {strings(stringKeys.reports.list.date)}
                </TableSortLabel>
              </TableCell>
              <TableCell style={{ width: "30%" }}>
                {strings(stringKeys.reports.list.errorType)}
              </TableCell>
              <TableCell style={{ width: "12%" }}>
                {strings(stringKeys.reports.list.message)}
              </TableCell>
              <TableCell style={{ width: "12%" }}>
                {strings(stringKeys.reports.list.dataCollectorDisplayName)}
              </TableCell>
              <TableCell style={{ width: "23%" }}>
                {strings(stringKeys.common.location)}
              </TableCell>
              <TableCell style={{ width: "13%" }}>
                {strings(stringKeys.reports.list.corrected)}
              </TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {list.map((row) => (
              <TableRow key={row.id} hover>
                <TableCell>
                  {dayjs(row.dateTime).format("YYYY-MM-DD HH:mm")}
                </TableCell>
                <TableCell>{errorTypeString(row.reportErrorType)}</TableCell>
                <TableCell>
                  <Typography className={styles.message} title={row.message}>
                    {dashIfEmpty(row.message)}
                  </Typography>
                </TableCell>
                <TableCell className={reportsTableStyles.phoneNumber}>
                  {renderDataCollectorDisplayName(row)}
                </TableCell>
                <TableCell>
                  {dashIfEmpty(row.region, row.district, row.village, row.zone)}
                </TableCell>
                <TableCell>
                  <CorrectedButton
                    isCorrected={row.isCorrected}
                    onClick={() => onCorrectToggle(row)}
                  />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
        <TablePager
          totalRows={totalRows}
          rowsPerPage={rowsPerPage}
          page={page}
          onChangePage={handlePageChange}
          rtl={rtl}
        />
      </TableContainer>
    </Fragment>
  );
};

IncorrectReportsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array,
};

export default IncorrectReportsTable;
