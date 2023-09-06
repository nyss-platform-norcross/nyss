import { stringKeys, strings } from "../../../strings";
import { Button, Checkbox, FormControlLabel } from "@material-ui/core";
import styles from "../filters/LocationFilter.module.scss";

export const SelectAll = ({
  isSelectAllEnabled,
  toggleSelectAll,
  showResults,
}) => {
  return (
    <>
      <hr className={styles.divider} />
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
      <Button variant="outlined" color="primary" onClick={showResults}>
        {strings(stringKeys.filters.area.showResults)}
      </Button>
    </>
  );
};
