import { stringKeys, strings } from "../../../strings";
import { Button, Checkbox, FormControlLabel } from "@material-ui/core";

export const SelectAll = ({
  styles,
  selectAll,
  toggleSelectAll,
  showResults,
}) => {
  return (
    <div className={styles.filterActionsContainer}>
      <FormControlLabel
        control={
          <Checkbox
            checked={selectAll}
            color="primary"
            onClick={toggleSelectAll}
          />
        }
        label={strings(stringKeys.filters.area.selectAll)}
      />
      <Button variant="outlined" color="primary" onClick={showResults}>
        {strings(stringKeys.filters.area.showResults)}
      </Button>
    </div>
  );
};
