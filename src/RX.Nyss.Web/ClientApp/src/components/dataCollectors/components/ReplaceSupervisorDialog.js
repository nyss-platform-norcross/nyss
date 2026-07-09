import styles from "./ReplaceSupervisorDialog.module.scss";
import { useState } from "react";
import { strings, stringKeys } from "../../../strings";
import FormActions from "../../forms/formActions/FormActions";
import SubmitButton from "../../common/buttons/submitButton/SubmitButton";
import CancelButton from "../../common/buttons/cancelButton/CancelButton";
import {
  useTheme,
  Grid,
  MenuItem,
  List,
  ListItem,
  Typography,
  Dialog,
  DialogContent,
  useMediaQuery,
} from "@material-ui/core";
import { validators, createForm } from "../../../utils/forms";
import Form from "../../forms/form/Form";
import SelectField from "../../forms/SelectField";
import WarningIcon from "@material-ui/icons/Warning";

export const ReplaceSupervisorDialog = ({
  isOpened,
  close,
  dataCollectors,
  supervisors,
  replaceSupervisor,
}) => {
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down("xs"));

  const [form] = useState(() => {
    const fields = {
      supervisorId: "",
    };

    const validation = {
      supervisorId: [validators.required],
    };
    return createForm(fields, validation);
  });

  const handleSubmit = (event) => {
    event.preventDefault();

    if (!form.isValid()) {
      return;
    }

    const selectedSupervisor = supervisors.find(
      (s) => s.id === parseInt(form.fields.supervisorId.value),
    );
    replaceSupervisor(
      dataCollectors.map((dc) => dc.id),
      parseInt(form.fields.supervisorId.value),
      selectedSupervisor.role,
    );
    close();
  };

  return (
    <Dialog onClose={close} open={isOpened} fullScreen={fullScreen}>
      <DialogContent>
        <Form onSubmit={handleSubmit} fullWidth>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <SelectField
                name="supervisorId"
                field={form.fields.supervisorId}
                label={strings(stringKeys.dataCollectors.form.newSupervisor)}
              >
                {supervisors.map((sup) => (
                  <MenuItem key={sup.id} value={sup.id.toString()}>
                    {sup.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>

            <Grid item xs={10} style={{ alignSelf: "center" }}>
              <Typography variant="body1">
                {strings(
                  stringKeys.dataCollectors.form.replaceSupervisorWarning,
                )}
              </Typography>
            </Grid>
            <Grid item xs={2}>
              <WarningIcon color="primary" style={{ fontSize: 40 }} />
            </Grid>

            <Grid item xs={12}>
              <List className={styles.dataCollectorList}>
                {dataCollectors.map((dc) => (
                  <ListItem key={dc.id}>{dc.name}</ListItem>
                ))}
              </List>
            </Grid>
          </Grid>

          <FormActions>
            <CancelButton onClick={close}>
              {strings(stringKeys.form.cancel)}
            </CancelButton>
            <SubmitButton>
              {strings(stringKeys.dataCollectors.form.replaceSupervisor)}
            </SubmitButton>
          </FormActions>
        </Form>
      </DialogContent>
    </Dialog>
  );
};
