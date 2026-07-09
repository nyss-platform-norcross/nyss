import {
  Select,
  MenuItem,
  InputLabel,
  Typography,
  FormHelperText,
  Box,
  Chip,
  Grid,
} from "@material-ui/core";
import { makeStyles } from "@material-ui/core/styles";
import { connect } from "react-redux";
import * as projectSetupActions from "./logic/projectSetupActions";
import { useEffect, useState } from "react";
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
  chip: {
    margin: "5px 2px",
    backgroundColor: "#CDDDE7",
    "&:focus": {
      backgroundColor: "#CDDDE7",
    },
  },
  chipContainer: {
    justifyContent: "center",
    maxWidth: "50rem",
    marginTop: 30,
  },
}));

const ProjectSetupRecipientsComponent = ({
  alertRecipients,
  alertNotHandledNotificationRecipientIds,
  setAlertNotHandledNotificationRecipientIds,
  error,
  setError,
  setIsNextStepInvalid,
  selectedOrganizationId,
}) => {
  const classes = useStyles();
  const errorMessage = strings(stringKeys.projectSetup.projectRecipients.error);
  const [availableRecipients, setAvailableRecipients] = useState([]);
  const [selectedRecipients, setSelectedRecipients] = useState([]);

  useMount(() => {
    if (alertRecipients) {
      const recipientsWithinSelectedOrganization = alertRecipients.filter(
        (recipient) => recipient.organizationId === selectedOrganizationId,
      );

      setAvailableRecipients(
        recipientsWithinSelectedOrganization.filter(
          (recipient) =>
            !alertNotHandledNotificationRecipientIds.includes(recipient.id),
        ),
      );
      setSelectedRecipients(
        recipientsWithinSelectedOrganization.filter((recipient) =>
          alertNotHandledNotificationRecipientIds.includes(recipient.id),
        ),
      );
    }
  });

  const handleChange = (event) => {
    const eventRecipient = availableRecipients.find(
      (recipient) => recipient.id === event.target.value,
    );

    if (eventRecipient) {
      setSelectedRecipients((selectedRecipients) => [
        ...selectedRecipients,
        eventRecipient,
      ]);
      setAvailableRecipients(
        availableRecipients.filter(
          (recipient) => recipient.id !== eventRecipient.id,
        ),
      );
      setError(false);
    }
  };

  const handleDelete = (deselectedRecipientId) => {
    setSelectedRecipients(
      selectedRecipients.filter(
        (recipient) => recipient.id !== deselectedRecipientId,
      ),
    );

    const deselectedRecipient = alertRecipients?.find(
      (recipient) => recipient.id === deselectedRecipientId,
    );
    setAvailableRecipients((availableRecipients) => [
      ...availableRecipients,
      deselectedRecipient,
    ]);
  };

  useEffect(() => {
    setAlertNotHandledNotificationRecipientIds(
      selectedRecipients.map((recipient) => recipient.id),
    );
  }, [selectedRecipients]);

  useEffect(() => {
    alertNotHandledNotificationRecipientIds.length > 0
      ? setIsNextStepInvalid(false)
      : setIsNextStepInvalid(true);
  }, [alertNotHandledNotificationRecipientIds]);

  return (
    <>
      <InputLabel className={classes.inputText}>
        {strings(stringKeys.projectSetup.projectRecipients.title)}
      </InputLabel>
      <Select
        className={`${classes.inputField}`}
        onChange={handleChange}
        value={""}
        displayEmpty
        renderValue={() => {
          return (
            <Typography style={{ color: "#4F4F4F", fontSize: 12 }}>
              {strings(stringKeys.projectSetup.projectRecipients.placeholder)}
            </Typography>
          );
        }}
      >
        {availableRecipients.length > 0 &&
          availableRecipients.map((recipient) => (
            <MenuItem key={recipient.id} value={recipient.id}>
              {recipient.name}
            </MenuItem>
          ))}
      </Select>
      {error && (
        <FormHelperText className={classes.errorMessage}>
          {errorMessage}
        </FormHelperText>
      )}
      <Grid container className={classes.chipContainer} direction="row">
        {selectedRecipients.map((recipient) => (
          <Box key={recipient.id}>
            <Chip
              className={classes.chip}
              label={recipient.name}
              onDelete={() => handleDelete(recipient.id)}
            />
          </Box>
        ))}
      </Grid>
    </>
  );
};

const mapStateToProps = (state) => ({
  alertRecipients: state.projectSetup.formData?.alertNotHandledRecipients,
  alertNotHandledNotificationRecipientIds:
    state.projectSetup.alertNotHandledNotificationRecipientIds,
  selectedOrganizationId: state.projectSetup.organizationId,
});

const mapDispatchToProps = {
  setAlertNotHandledNotificationRecipientIds:
    projectSetupActions.setAlertNotHandledNotificationRecipientIds,
};

export const ProjectSetupRecipients = connect(
  mapStateToProps,
  mapDispatchToProps,
)(ProjectSetupRecipientsComponent);
