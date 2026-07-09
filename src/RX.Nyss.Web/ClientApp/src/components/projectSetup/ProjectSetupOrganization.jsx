import {
  Select,
  MenuItem,
  InputLabel,
  Typography,
  FormHelperText,
  Checkbox,
  FormControlLabel,
} from "@material-ui/core";
import { makeStyles } from "@material-ui/core/styles";
import { connect } from "react-redux";
import * as projectSetupActions from "./logic/projectSetupActions";
import { useState } from "react";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";

const useStyles = makeStyles((theme) => ({
  inputText: {
    fontSize: "16px",
    fontWeight: 700,
    marginBottom: 8,
  },
  inputField: {
    width: 270,
  },
  errorMessage: {
    color: theme.palette.error.main,
    textAlign: "left",
    width: 270,
  },
}));

export const ProjectSetupOrganizationComponent = ({
  organizations,
  rtl,
  setOrganizationId,
  selectedOrganizationId,
  error,
  setError,
  setIsNextStepInvalid,
  allowMultipleOrganizations,
  setAllowMultipleOrganizations,
}) => {
  const classes = useStyles();
  const [selectedOrganization, setSelectedOrganization] = useState(undefined);
  const errorMessage = strings(
    stringKeys.projectSetup.projectOrganization.error,
  );

  useMount(() => {
    if (selectedOrganizationId) {
      setSelectedOrganization(
        organizations.find((org) => org.id === selectedOrganizationId),
      );
      setIsNextStepInvalid(false);
    }
  });

  const handleChange = (event) => {
    const eventOrganization = organizations.find(
      (org) => org.id === event.target.value,
    );
    if (eventOrganization) {
      setOrganizationId(event.target.value);
      setIsNextStepInvalid(false);
      setSelectedOrganization(eventOrganization);
      setError(false);
    }
  };

  return (
    <>
      <InputLabel className={classes.inputText}>
        {strings(stringKeys.projectSetup.projectOrganization.title)}
      </InputLabel>
      <Select
        className={`${classes.inputField}`}
        onChange={handleChange}
        value={selectedOrganizationId ? selectedOrganizationId : ""}
        displayEmpty
        renderValue={(selectedId) => {
          if (selectedId === "") {
            return (
              <Typography style={{ color: "#4F4F4F", fontSize: 12 }}>
                {strings(
                  stringKeys.projectSetup.projectOrganization.placeholder,
                )}
              </Typography>
            );
          }

          return selectedOrganization ? selectedOrganization.name : "";
        }}
      >
        {organizations?.map((organization) => (
          <MenuItem key={organization.name} value={organization.id}>
            {organization.name}
          </MenuItem>
        ))}
      </Select>
      {error && (
        <FormHelperText className={classes.errorMessage}>
          {errorMessage}
        </FormHelperText>
      )}
      <FormControlLabel
        style={{ marginTop: 20 }}
        control={
          <Checkbox
            checked={allowMultipleOrganizations}
            onChange={(e) => setAllowMultipleOrganizations(e.target.checked)}
            name="allowMultipleOrganizations"
            color="primary"
          />
        }
        label={strings(stringKeys.project.form.allowMultipleOrganizations)}
      />
    </>
  );
};

const mapStateToProps = (state) => ({
  organizations: state.projectSetup.formData?.organizations,
  selectedOrganizationId: state.projectSetup.organizationId,
  allowMultipleOrganizations: state.projectSetup.allowMultipleOrganizations,
});

const mapDispatchToProps = {
  setOrganizationId: projectSetupActions.setOrganizationId,
  setAllowMultipleOrganizations:
    projectSetupActions.setAllowMultipleOrganizations,
};

export const ProjectSetupOrganization = connect(
  mapStateToProps,
  mapDispatchToProps,
)(ProjectSetupOrganizationComponent);
