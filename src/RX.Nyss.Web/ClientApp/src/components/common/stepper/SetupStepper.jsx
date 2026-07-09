import React, { useState } from "react";
import { makeStyles, withStyles } from "@material-ui/core/styles";
import {
  Grid,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  useTheme,
} from "@material-ui/core";
import Stepper from "@material-ui/core/Stepper";
import Step from "@material-ui/core/Step";
import StepLabel from "@material-ui/core/StepLabel";
import Button from "@material-ui/core/Button";
import Typography from "@material-ui/core/Typography";
import StepConnector from "@material-ui/core/StepConnector";
import CheckIcon from "@material-ui/icons/Check";
import { strings, stringKeys } from "../../../strings";
import SubmitButton from "../buttons/submitButton/SubmitButton";

const useStyles = makeStyles((theme) => ({
  stepper: {
    backgroundColor: "inherit",
  },
  step: {
    cursor: "pointer",
  },
  backButton: {
    marginRight: theme.spacing(1),
  },
  dialogTitle: {
    fontSize: 240,
  },
}));

const useColorlibStepIconStyles = makeStyles((theme) => ({
  root: {
    border: "1px solid #E3E3E3",
    zIndex: 1,
    width: 40,
    height: 40,
    display: "flex",
    borderRadius: "50%",
    justifyContent: "center",
    alignItems: "center",
  },
  active: {
    border: `1px solid ${theme.palette.primary.main}`,
    color: theme.palette.primary.main,
  },
  activeText: {
    color: theme.palette.primary.main,
    fontWeight: 700,
  },
  notCompletedText: {
    color: "#B4B4B4",
    fontWeight: 700,
  },
  completed: {
    backgroundColor: theme.palette.primary.main,
    border: `1px solid ${theme.palette.primary.main}`,
    color: theme.palette.primary.main,
  },
}));

const Connector = withStyles((theme) => ({
  alternativeLabel: {
    top: 22,
  },
  active: {
    "& $line": {
      backgroundColor: theme.palette.primary.main,
    },
  },
  completed: {
    "& $line": {
      backgroundColor: theme.palette.primary.main,
    },
  },
  line: {
    margin: "0 10px 0 10px",
    height: 3,
    border: 0,
    backgroundColor: "#eaeaf0",
    borderRadius: 1,
  },
}))(StepConnector);

const getStepContent = (steps, stepIndex) => {
  return steps.find((step) => step.stepNumber === stepIndex).content;
};
const getStep = (steps, stepIndex) => {
  return steps.find((step) => step.stepNumber === stepIndex);
};

export const SetupStepper = ({
  steps,
  error,
  setError,
  isNextStepInvalid,
  setIsNextStepInvalid,
  goToList,
  nationalSocietyId,
}) => {
  const theme = useTheme();
  const classes = useStyles();
  const [activeStep, setActiveStep] = useState(0);
  const sortedSteps = steps.sort(
    (stepA, stepB) => stepA.stepNumber - stepB.stepNumber,
  );
  const [open, setOpen] = useState(false);

  const handleNext = () => {
    if (
      getStep(steps, activeStep).isOptional ||
      (!error && !isNextStepInvalid)
    ) {
      setActiveStep((prevActiveStep) => prevActiveStep + 1);
      setIsNextStepInvalid(true);
    } else {
      setError(true);
    }
  };

  const handleBack = () => {
    setError(false);
    setIsNextStepInvalid(false);
    setActiveStep((prevActiveStep) => prevActiveStep - 1);
  };

  const handleReset = () => {
    setOpen(true);
  };

  const handleFinish = () => {
    // Add api request here to create project
  };
  const handleClose = () => {
    setOpen(false);
  };

  const handleCancel = () => {
    goToList(nationalSocietyId);
  };

  const StepIcon = (props) => {
    const classes = useColorlibStepIconStyles();
    const { active, completed, icon } = props;

    return (
      <div
        className={`${classes.root} ${active && classes.active} ${
          (completed || activeStep === steps.length - 1) && classes.completed
        }`}
      >
        <Typography
          className={`${active && classes.activeText} ${
            !completed && !active && classes.notCompletedText
          }`}
        >
          {completed || activeStep === steps.length - 1 ? (
            <CheckIcon
              fontSize="small"
              style={{ marginTop: 5, color: "#ECECEC" }}
            />
          ) : (
            icon
          )}
        </Typography>
      </div>
    );
  };

  const onStepClick = (stepNumber) => {
    if (stepNumber > activeStep) return;
    setActiveStep(stepNumber);
  };

  return (
    <>
      <Stepper
        activeStep={activeStep}
        alternativeLabel
        className={classes.stepper}
        connector={<Connector />}
      >
        {sortedSteps.map((step) => (
          <Step
            className={classes.step}
            key={step.name}
            onClick={() => onStepClick(step.stepNumber)}
          >
            <StepLabel StepIconComponent={StepIcon}>
              <Typography
                style={{
                  color:
                    step.stepNumber >= activeStep
                      ? "#7C7C7C"
                      : theme.palette.primary.main,
                }}
              >
                {step.name}
              </Typography>
            </StepLabel>
          </Step>
        ))}
      </Stepper>
      <Grid container direction="column" alignItems="center">
        <Grid
          container
          direction="column"
          alignItems="center"
          style={{ margin: "50px 0 50px 0" }}
        >
          {getStepContent(steps, activeStep)}
        </Grid>
        <Grid container direction="column" alignItems="center">
          <Grid item>
            {activeStep !== 0 && (
              <Button
                color="primary"
                variant="outlined"
                disabled={activeStep === 0}
                onClick={handleBack}
                className={classes.backButton}
              >
                {strings(stringKeys.common.buttons.previous)}
              </Button>
            )}
            <Button
              variant={
                activeStep === steps.length - 1 ||
                getStep(steps, activeStep).isOptional ||
                (!error && !isNextStepInvalid)
                  ? "contained"
                  : "outlined"
              }
              color="primary"
              onClick={
                activeStep === steps.length - 1 ? handleFinish : handleNext
              }
            >
              {activeStep === steps.length - 1
                ? strings(stringKeys.common.buttons.finish)
                : strings(stringKeys.common.buttons.next)}
            </Button>
          </Grid>
          <Grid item style={{ marginTop: 10 }}>
            <Button color="primary" onClick={handleReset}>
              {strings(stringKeys.form.cancel)}
            </Button>
          </Grid>
          <Dialog open={open} onClose={handleClose}>
            <DialogTitle classes={{ root: classes.dialogTitle }}>
              <Typography style={{ fontSize: 24, fontWeight: 600 }}>
                {strings(stringKeys.projectSetup.cancelDialog.title)}
              </Typography>
            </DialogTitle>
            <DialogContent>
              <Typography style={{ marginBottom: 20 }} gutterBottom>
                {strings(stringKeys.projectSetup.cancelDialog.description)}
              </Typography>
            </DialogContent>
            <DialogActions style={{ margin: "0 20px 10px 0" }}>
              <Button onClick={handleCancel} color="primary">
                {strings(stringKeys.common.buttons.confirmCancelation)}
              </Button>
              <SubmitButton onClick={handleClose} color="primary" autoFocus>
                {strings(stringKeys.common.buttons.denyCancelation)}
              </SubmitButton>
            </DialogActions>
          </Dialog>
        </Grid>
      </Grid>
    </>
  );
};
