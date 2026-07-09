import { InputLabel, TextField } from "@material-ui/core";
import { makeStyles } from "@material-ui/core/styles";
import * as projectSetupActions from "./logic/projectSetupActions";
import { connect } from "react-redux";
import { strings, stringKeys } from "../../strings";

const useStyles = makeStyles(() => ({
  input: {
    width: 270,
    "& .MuiInput-formControl": {
      marginTop: "0px !important",
    },
  },
  inputLabel: {
    fontSize: "16px",
    fontWeight: 700,
    marginBottom: 8,
  },
}));

const ProjectSetupNameComponent = ({
  projectName,
  setProjectName,
  error,
  setError,
  setIsNextStepInvalid,
}) => {
  const classes = useStyles();

  const handleChange = (event) => {
    setError(event.target.value === "");
    setIsNextStepInvalid(event.target.value === "");
    setProjectName(event.target.value);
  };

  return (
    <>
      <InputLabel className={classes.inputLabel}>
        {strings(stringKeys.projectSetup.projectName.title)}
      </InputLabel>
      <TextField
        value={projectName}
        onChange={handleChange}
        error={error}
        className={classes.input}
        helperText={
          error ? strings(stringKeys.projectSetup.projectName.error) : null
        }
      />
    </>
  );
};

const mapStateToProps = (state) => ({
  projectName: state.projectSetup.projectName,
});

const mapDispatchToProps = {
  setProjectName: projectSetupActions.setProjectName,
};

export const ProjectSetupName = connect(
  mapStateToProps,
  mapDispatchToProps,
)(ProjectSetupNameComponent);
