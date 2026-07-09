import styles from "./TablePager.module.scss";
import KeyboardArrowLeft from "@material-ui/icons/KeyboardArrowLeft";
import KeyboardArrowRight from "@material-ui/icons/KeyboardArrowRight";
import { Grid, IconButton, MenuItem, Select } from "@material-ui/core";

export const TablePager = ({
  page,
  rowsPerPage,
  totalRows,
  onChangePage,
  rtl,
}) => {
  const numberOfPages = Math.ceil(totalRows / rowsPerPage);
  const pagesRange = [...Array(numberOfPages)].map((_, i) => i + 1);

  const handleBackButtonClick = (event) => {
    onChangePage(event, page - 1);
  };

  const handleNextButtonClick = (event) => {
    onChangePage(event, page + 1);
  };

  const handlePageSelect = (event) => {
    onChangePage(event, event.target.value);
  };

  if (numberOfPages < 2) {
    return null;
  }

  return (
    <Grid container className={styles.pager}>
      <IconButton
        onClick={handleBackButtonClick}
        disabled={page === 1}
        aria-label="previous page"
      >
        {rtl ? <KeyboardArrowRight /> : <KeyboardArrowLeft />}
      </IconButton>

      <Select
        className={`${styles.dropDown}`}
        onChange={handlePageSelect}
        value={page}
      >
        {pagesRange.map((page) => (
          <MenuItem value={page} key={page}>
            {page}
          </MenuItem>
        ))}
      </Select>

      <IconButton
        onClick={handleNextButtonClick}
        disabled={page >= Math.ceil(totalRows / rowsPerPage)}
        aria-label="next page"
      >
        {rtl ? <KeyboardArrowLeft /> : <KeyboardArrowRight />}
      </IconButton>
    </Grid>
  );
};

export default TablePager;
