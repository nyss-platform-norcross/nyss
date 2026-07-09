import { useState, Fragment, useEffect } from "react";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm } from "../../utils/forms";
import * as projectAlertRecipientsActions from "./logic/projectAlertRecipientsActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import PhoneInputField from "../forms/PhoneInputField";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { ValidationMessage } from "../forms/ValidationMessage";
import AutocompleteTextInputField from "../forms/AutocompleteTextInputField";
import TextInputField from "../forms/TextInputField";
import {
  CardContent,
  Card,
  Typography,
  FormControlLabel,
  Checkbox,
  MenuItem,
  Grid,
} from "@material-ui/core";
import { MultiSelect } from "../forms/MultiSelect";
import SelectField from "../forms/SelectField";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import { SubMenuTitle } from "../layout/SubMenuTitle";
import { trackPageView } from "../../utils/appInsightsHelper";

const ProjectAlertRecipientsEditPageComponent = ({
  formData,
  listData,
  alertRecipient,
  projectId,
  alertRecipientId,
  isSaving,
  error,
  directionRtl,
  openEdition,
  edit,
  goToList,
}) => {
  const [freeTextOrganizations, setFreeTextOrganizations] = useState([]);
  const [selectedSupervisors, setSelectedSupervisors] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);
  const [acceptAnySupervisor, setAcceptAnySupervisor] = useState(false);
  const [acceptAnyHealthRisk, setAcceptAnyHealthRisk] = useState(false);
  const canSelectModem = formData != null && formData.modems.length > 0;

  const [form, setForm] = useState(null);

  useMount(() => {
    openEdition(projectId, alertRecipientId);

    // Track page view
    trackPageView("ProjectAlertRecipientsEditPage");
  });

  useEffect(() => {
    if (alertRecipient === null || formData === null) {
      return;
    }

    const uniqueOrganizations = [
      ...new Set(listData.map((ar) => ar.organization)),
    ];
    setFreeTextOrganizations(uniqueOrganizations.map((o) => ({ title: o })));
    setSelectedSupervisors(
      [...alertRecipient.supervisors, ...alertRecipient.headSupervisors].map(
        (s) => {
          return { label: s.name, value: s };
        },
      ),
    );
    setSelectedHealthRisks(
      alertRecipient.healthRisks.map((hr) => {
        return { label: hr.healthRiskName, value: hr.id };
      }),
    );
    setAcceptAnySupervisor(alertRecipient.supervisors.length === 0);
    setAcceptAnyHealthRisk(alertRecipient.healthRisks.length === 0);

    const fields = {
      role: alertRecipient.role,
      organization: alertRecipient.organization,
      email: alertRecipient.email || "",
      phoneNumber: alertRecipient.phoneNumber || "",
      modemId: alertRecipient.modemId || "",
    };

    const validation = {
      role: [validators.required],
      organization: [validators.required],
      email: [validators.emailWhen((x) => x.phoneNumber === "")],
      phoneNumber: [
        validators.phoneNumber,
        validators.requiredWhen((x) => x.email === ""),
      ],
      modemId: [validators.requiredWhen((_) => canSelectModem)],
    };

    setForm(createForm(fields, validation));
  }, [listData, alertRecipient, formData, canSelectModem]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    const values = form.getValues();
    edit(projectId, {
      id: alertRecipient.id,
      role: values.role,
      organization: values.organization,
      email: values.email,
      phoneNumber: values.phoneNumber,
      supervisors: acceptAnySupervisor
        ? []
        : selectedSupervisors.map((s) => s.value),
      healthRisks: acceptAnyHealthRisk
        ? []
        : selectedHealthRisks.map((hr) => hr.value),
      headSupervisors: [],
      modemId: !!values.modemId ? parseInt(values.modemId) : null,
    });
  };

  const onSupervisorChange = (value, eventData) => {
    if (eventData.action === "select-option") {
      setSelectedSupervisors([...selectedSupervisors, eventData.option]);
    } else if (
      eventData.action === "remove-value" ||
      eventData.action === "pop-value"
    ) {
      setSelectedSupervisors(
        selectedSupervisors.filter(
          (hr) => hr.value !== eventData.removedValue.value,
        ),
      );
    } else if (eventData.action === "clear") {
      setSelectedSupervisors([]);
    }
  };

  const onHealthRiskChange = (value, eventData) => {
    if (eventData.action === "select-option") {
      setSelectedHealthRisks([...selectedHealthRisks, eventData.option]);
    } else if (
      eventData.action === "remove-value" ||
      eventData.action === "pop-value"
    ) {
      setSelectedHealthRisks(
        selectedHealthRisks.filter(
          (hr) => hr.value !== eventData.removedValue.value,
        ),
      );
    } else if (eventData.action === "clear") {
      setSelectedHealthRisks([]);
    }
  };

  if (!formData || !form) {
    return null;
  }

  return (
    <Fragment>
      <SubMenuTitle />
      {error && <ValidationMessage message={error} />}

      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <Card style={{ overflow: "visible" }}>
              <CardContent>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Typography variant="h3">
                      {strings(
                        stringKeys.projectAlertRecipient.form.receiverDetails,
                      )}
                    </Typography>
                  </Grid>

                  <Grid item xs={12}>
                    <TextInputField
                      label={strings(
                        stringKeys.projectAlertRecipient.form.role,
                      )}
                      field={form.fields.role}
                      name="role"
                    />
                  </Grid>

                  <Grid item xs={12}>
                    <AutocompleteTextInputField
                      label={strings(
                        stringKeys.projectAlertRecipient.form.organization,
                      )}
                      field={form.fields.organization}
                      options={freeTextOrganizations}
                      freeSolo
                      autoSelect
                      allowAddingValue
                      name="organization"
                    />
                  </Grid>

                  <Grid item xs={12}>
                    <TextInputField
                      label={strings(stringKeys.common.email)}
                      field={form.fields.email}
                      name="email"
                      inputMode={"email"}
                    />
                  </Grid>

                  <Grid item xs={12}>
                    <PhoneInputField
                      label={strings(
                        stringKeys.projectAlertRecipient.form.phoneNumber,
                      )}
                      field={form.fields.phoneNumber}
                      name="phoneNumber"
                      defaultCountry={formData.countryCode}
                      rtl={directionRtl}
                    />
                  </Grid>

                  {canSelectModem && (
                    <Grid item xs={12}>
                      <SelectField
                        label={strings(
                          stringKeys.projectAlertRecipient.form.modem,
                        )}
                        field={form.fields.modemId}
                        name="modemId"
                      >
                        {formData.modems.map((modem) => (
                          <MenuItem
                            key={`modemId_${modem.id}`}
                            value={modem.id.toString()}
                          >
                            {modem.name}
                          </MenuItem>
                        ))}
                      </SelectField>
                    </Grid>
                  )}
                </Grid>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={6}>
            <Card style={{ overflow: "visible" }}>
              <CardContent>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <Typography variant="h3">
                      {strings(
                        stringKeys.projectAlertRecipient.form.triggerDetails,
                      )}
                    </Typography>
                  </Grid>

                  <Grid item xs={12}>
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={acceptAnySupervisor}
                          onClick={() =>
                            setAcceptAnySupervisor(!acceptAnySupervisor)
                          }
                          color="primary"
                        />
                      }
                      label={strings(
                        stringKeys.projectAlertRecipient.form.anySupervisor,
                      )}
                    />
                  </Grid>

                  {!acceptAnySupervisor && (
                    <Grid item xs={12}>
                      <MultiSelect
                        label={strings(
                          stringKeys.projectAlertRecipient.form.supervisors,
                        )}
                        options={formData.supervisors
                          .filter(
                            (s) =>
                              !selectedSupervisors.some(
                                (ss) => ss.id === s.id,
                              ) &&
                              s.organizationId ===
                                alertRecipient.organizationId,
                          )
                          .map((s) => {
                            return { label: s.name, value: s };
                          })}
                        value={selectedSupervisors}
                        onChange={onSupervisorChange}
                      />
                    </Grid>
                  )}
                  <Grid item xs={12}>
                    <FormControlLabel
                      control={
                        <Checkbox
                          checked={acceptAnyHealthRisk}
                          onClick={() =>
                            setAcceptAnyHealthRisk(!acceptAnyHealthRisk)
                          }
                          color="primary"
                        />
                      }
                      label={strings(
                        stringKeys.projectAlertRecipient.form.anyHealthRisk,
                      )}
                    />
                  </Grid>
                  {!acceptAnyHealthRisk && (
                    <Grid item xs={12}>
                      <MultiSelect
                        label={strings(
                          stringKeys.projectAlertRecipient.form.healthRisks,
                        )}
                        options={formData.healthRisks
                          .filter(
                            (hr) =>
                              !selectedHealthRisks.some(
                                (shr) => shr.id === hr.id,
                              ),
                          )
                          .map((s) => {
                            return { label: s.healthRiskName, value: s.id };
                          })}
                        value={selectedHealthRisks}
                        onChange={onHealthRiskChange}
                      />
                    </Grid>
                  )}
                </Grid>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
        <FormActions>
          <CancelButton onClick={() => goToList(projectId)}>
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton isFetching={isSaving}>
            {strings(stringKeys.common.buttons.update)}
          </SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
};

ProjectAlertRecipientsEditPageComponent.propTypes = {};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  alertRecipientId: ownProps.match.params.alertRecipientId,
  listData: state.projectAlertRecipients.listData,
  alertRecipient: state.projectAlertRecipients.recipient,
  formData: state.projectAlertRecipients.formData,
  isSaving: state.projectAlertRecipients.formSaving,
  error: state.projectAlertRecipients.formError,
  directionRtl: state.appData.user.languageCode === "ar",
});

const mapDispatchToProps = {
  openEdition: projectAlertRecipientsActions.openEdition.invoke,
  edit: projectAlertRecipientsActions.edit.invoke,
  goToList: projectAlertRecipientsActions.goToList,
};

export const ProjectAlertRecipientsEditPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(ProjectAlertRecipientsEditPageComponent),
);
