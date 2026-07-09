import {
  Checkbox,
  FormControl,
  FormControlLabel,
  FormGroup,
  FormLabel,
  makeStyles,
} from "@material-ui/core";
import { Fragment } from "react";
import { stringKeys, strings } from "../../../strings";

const useStyles = makeStyles(() => ({
  checkbox: {
    height: "23px ",
    "& .MuiTypography-body1": {
      lineHeight: "0.8",
    },
  },
  filterCheckboxGroup: {
    maxHeight: "50px",
    paddingTop: "5px",
  },
  noFlexWrap: {
    flexWrap: "nowrap",
  },
}));

export const ReportStatusFilter = ({
  filter,
  onChange,
  correctReports,
  showDismissedFilter,
  doNotWrap,
}) => {
  const classes = useStyles();

  return (
    <Fragment>
      <FormControl className={classes.filterItem}>
        <FormLabel component="legend">
          {strings(stringKeys.filters.report.status)}
        </FormLabel>
        <FormGroup
          className={
            doNotWrap
              ? `${classes.filterCheckboxGroup} ${classes.noFlexWrap}`
              : classes.filterCheckboxGroup
          }
        >
          {correctReports && (
            <Fragment>
              <FormControlLabel
                className={classes.checkbox}
                control={
                  <Checkbox
                    checked={filter.kept}
                    onChange={onChange}
                    name="kept"
                    color="primary"
                  />
                }
                label={strings(stringKeys.filters.report.kept)}
              />
              <FormControlLabel
                className={classes.checkbox}
                control={
                  <Checkbox
                    checked={filter.notCrossChecked}
                    onChange={onChange}
                    name="notCrossChecked"
                    color="primary"
                  />
                }
                label={strings(stringKeys.filters.report.notCrossChecked)}
              />
              {showDismissedFilter && (
                <FormControlLabel
                  className={classes.checkbox}
                  control={
                    <Checkbox
                      checked={filter.dismissed}
                      onChange={onChange}
                      name="dismissed"
                      color="primary"
                    />
                  }
                  label={strings(stringKeys.filters.report.dismissed)}
                />
              )}
            </Fragment>
          )}
          {!correctReports && (
            <Fragment>
              <FormControlLabel
                className={classes.checkbox}
                control={
                  <Checkbox
                    checked={filter.real}
                    onChange={onChange}
                    name="real"
                    color="primary"
                  />
                }
                label={strings(stringKeys.filters.report.nonTrainingReports)}
              />
              <FormControlLabel
                className={classes.checkbox}
                control={
                  <Checkbox
                    checked={filter.corrected}
                    onChange={onChange}
                    name="corrected"
                    color="primary"
                  />
                }
                label={strings(stringKeys.filters.report.correctedReports)}
              />
            </Fragment>
          )}
        </FormGroup>
      </FormControl>
    </Fragment>
  );
};
