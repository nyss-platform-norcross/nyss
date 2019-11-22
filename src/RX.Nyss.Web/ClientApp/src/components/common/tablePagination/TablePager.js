import styles from "./TablePager.module.scss";
import React from 'react';
import IconButton from '@material-ui/core/IconButton';
import TableCell from '@material-ui/core/TableCell';
import MenuItem from '@material-ui/core/MenuItem';
import Select from '@material-ui/core/Select';
import KeyboardArrowLeft from '@material-ui/icons/KeyboardArrowLeft';
import KeyboardArrowRight from '@material-ui/icons/KeyboardArrowRight';
import Grid from "@material-ui/core/Grid";

export const TablePager = (props) => {
  
    const { page, rowsPerPage, totalRows, onChangePage } = props;
    
    const handleBackButtonClick = event => {
      onChangePage(event, page - 1);
    };
  
    const handleNextButtonClick = event => {
      onChangePage(event, page + 1);
    };

    const handlePageSelect = event => {
      onChangePage(event, event.target.value);
    };

    const pagesRange = (rowsPerPage, totalRows) => {
      let range = [];
      let numberOfPages = Math.ceil(totalRows/rowsPerPage);
      for (let i = 1; i <= numberOfPages; i++) {
        range.push(i);
      }
      return range;
    };

    return (
      <Grid
        container
        direction="row"
        justify="flex-end"
        alignItems="center"
        className={styles.pager}
      >  
        <IconButton onClick={handleBackButtonClick} disabled={page === 1} aria-label="previous page">
          <KeyboardArrowLeft />
        </IconButton>
        
        <Select className={`${styles.dropDown}`} onChange={handlePageSelect}  value={page}>
          {pagesRange(rowsPerPage, totalRows).map(page => ( <MenuItem value={page} key={page}>{page}</MenuItem> ) ) }
        </Select>
        
        <IconButton
          onClick={handleNextButtonClick}
          disabled={page >= Math.ceil(totalRows / rowsPerPage)}
          aria-label="next page">
           <KeyboardArrowRight />
        </IconButton>
      </Grid>
    );
};

export default TablePager;