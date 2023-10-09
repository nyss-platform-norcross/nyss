import { stringKeys, strings } from "../../../strings";
import { Button, Checkbox, FormControlLabel, Grid } from "@material-ui/core";
import styles from "../filters/LocationFilter.module.scss";

export const SelectAll = ({
  isSelectAllEnabled,
  toggleSelectAll,
  showResults,
}) => {
  return (
    <Grid
      container
      justifyContent="space-between"
      style={{
        paddingLeft: 10,
        paddingRight: 10,
      }}
    >
      <FormControlLabel
        control={
          <Checkbox
            checked={isSelectAllEnabled}
            color="primary"
            onClick={toggleSelectAll}
          />
        }
        label={strings(stringKeys.filters.area.selectAll)}
      />
      <Button size="small" variant="outlined" color="primary" onClick={showResults}>
        {strings(stringKeys.filters.area.showResults)}
      </Button>
      <hr className={styles.divider} />
    </Grid>
  );
};
