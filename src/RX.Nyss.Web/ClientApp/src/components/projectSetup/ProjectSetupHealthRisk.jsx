import {
  Box,
  Chip,
  FormControl,
  Grid,
  MenuItem,
  Select,
  FormHelperText,
  Typography,
  makeStyles,
} from "@material-ui/core";
import { strings, stringKeys } from "../../strings";
import { connect } from "react-redux";
import * as projectSetupActions from "./logic/projectSetupActions";
import { HealthRiskCardEditable } from "../common/healthRisk/HealthRiskCardEditable";
import { useMount } from "../../utils/lifecycle";
import { useEffect } from "react";

const useStyle = makeStyles((theme) => ({
  container: {
    width: "90%",
    justifyContent: "center",
    alignItems: "center",
  },
  chipContainer: {
    justifyContent: "center",
    width: "70%",
  },
  select: {
    width: "300px",
    minWidth: "200px",
    textOverflow: "ellipsis",
    marginTop: "0px",
  },
  input: {
    marginTop: "0px",
    backgroundColor: "blue",
  },
  header: {
    display: "inline",
    "&.healthRiskName": {
      marginLeft: "10px",
    },
    "&.healthRiskNameRtl": {
      marginRight: "10px",
    },
  },
  chip: {
    margin: "5px 5px",
    backgroundColor: "#CDDDE7",
    "&:focus": {
      backgroundColor: "#CDDDE7",
    },
  },
  chipDisabled: {
    margin: "5px 5px",
    backgroundColor: "#E3E3E3",
    "&:focus": {
      backgroundColor: "#E3E3E3",
    },
  },
  label: {
    top: -15,
  },
  errorMessage: {
    color: theme.palette.error.main,
    textAlign: "center",
    width: "100%",
  },
}));

export const ProjectSetupHealthRiskComponent = ({
  formHealthRisks,
  projectHealthRisks,
  setProjectHealthRisks,
  requiredHealthRisks,
  error,
  setError,
  setIsNextStepInvalid,
}) => {
  useMount(() => {
    setIsNextStepInvalid(false);
  }, []);

  useEffect(() => {
    setIsNextStepInvalid(projectHealthRisks.length === 0);
  }, [projectHealthRisks, error, setIsNextStepInvalid]);

  const classes = useStyle();

  const handleSelect = (e) => {
    setProjectHealthRisks(
      e.target.value.sort((a, b) => a.healthRiskId - b.healthRiskId),
    );

    if (e.target.value.length > 0) {
      setError(false);
    }
  };

  const handleDelete = (healthRiskId) => {
    setProjectHealthRisks(
      projectHealthRisks.filter(
        (healthRisk) => healthRisk.healthRiskId !== healthRiskId,
      ),
    );
  };

  const updateHealthRisk = (healthRiskId, newValue) => {
    if (projectHealthRisks.find((i) => i.healthRiskId === healthRiskId)) {
      setProjectHealthRisks(
        projectHealthRisks.map((healthRisk) =>
          healthRisk.healthRiskId === healthRiskId ? newValue : healthRisk,
        ),
      );
    } else {
      throw new Error(`HealthRiskId ${healthRiskId} not found`);
    }
  };

  //Filter out health risks that are already selected or required
  const selectFieldOptions = formHealthRisks.filter(
    (healthRisk) =>
      !projectHealthRisks.find(
        (i) => i.healthRiskId === healthRisk.healthRiskId,
      ) &&
      !requiredHealthRisks.find(
        (i) => i.healthRiskId === healthRisk.healthRiskId,
      ),
  );

  return (
    <Grid
      container
      direction="column"
      className={classes.container}
      spacing={1}
    >
      <Grid item>
        <Typography variant="h3">
          {strings(stringKeys.projectSetup.projectHealthRisks.title)}
        </Typography>
      </Grid>

      <Grid item style={{ marginTop: "-30px" }}>
        <Typography variant="subtitle1" style={{ color: "#7C7C7C" }}>
          {strings(stringKeys.projectSetup.projectHealthRisks.description)}
        </Typography>
      </Grid>

      {/* Select field to add new health risks to project */}
      <Grid item>
        <FormControl error={error}>
          <Select
            multiple
            name="healthRisk"
            id="healthRisk"
            value={projectHealthRisks}
            classes={{ root: classes.select }}
            onChange={handleSelect}
            displayEmpty
            style={{ marginTop: 0 }}
            renderValue={() => (
              <Typography
                style={{ color: "#4F4F4F", fontSize: 14, opacity: "80%" }}
              >
                {strings(
                  stringKeys.projectSetup.projectHealthRisks.placeholder,
                )}
              </Typography>
            )}
          >
            <MenuItem value={-1} disabled>
              Choose Health risks below
            </MenuItem>
            {selectFieldOptions.map((healthRisk) => (
              <MenuItem key={healthRisk.healthRiskId} value={healthRisk}>
                {healthRisk.healthRiskName}
              </MenuItem>
            ))}
          </Select>
          {error && (
            <FormHelperText className={classes.errorMessage}>
              {strings(stringKeys.projectSetup.projectHealthRisks.error)}
            </FormHelperText>
          )}
        </FormControl>
      </Grid>

      {/* Display selected Health risks as chips */}
      <Grid
        container
        className={classes.chipContainer}
        style={{ marginTop: "30px" }}
        direction="row"
      >
        {projectHealthRisks.map((healthRisk) => (
          <Box key={healthRisk.healthRiskId}>
            <Chip
              className={classes.chip}
              label={healthRisk.healthRiskName}
              onDelete={() => handleDelete(healthRisk.healthRiskId)}
            />
          </Box>
        ))}
        {requiredHealthRisks.map((healthRisk) => (
          <Box key={healthRisk.healthRiskId}>
            <Chip
              className={classes.chipDisabled}
              label={healthRisk.healthRiskName}
            />
          </Box>
        ))}
      </Grid>

      {/* Display selected health risk cards */}
      <Grid
        container
        direction="column"
        justifyContent="center"
        alignContent="center"
        style={{ marginTop: "45px" }}
      >
        {projectHealthRisks.map((healthRisk) => (
          <HealthRiskCardEditable
            key={healthRisk.healthRiskId}
            style={{ marginBottom: "20px" }}
            healthRisk={healthRisk}
            setHealthRisk={(newValue) =>
              updateHealthRisk(healthRisk.healthRiskId, newValue)
            }
          />
        ))}
        {requiredHealthRisks.map((healthRisk) => (
          <HealthRiskCardEditable
            key={healthRisk.healthRiskId}
            style={{ marginBottom: "20px" }}
            healthRisk={healthRisk}
            disabled
          />
        ))}
      </Grid>
    </Grid>
  );
};

const mapStateToProps = (state) => ({
  formHealthRisks: state.projectSetup.formData.healthRisks,
  projectHealthRisks: state.projectSetup.healthRisks,
  requiredHealthRisks: state.projectSetup.requiredHealthRisks,
});

const mapDispatchToProps = {
  setProjectHealthRisks: projectSetupActions.setHealthRisks,
};

export const ProjectSetupHealthRisk = connect(
  mapStateToProps,
  mapDispatchToProps,
)(ProjectSetupHealthRiskComponent);
