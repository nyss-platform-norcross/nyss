import React, {Fragment, useEffect, useState} from 'react';
import { connect, useSelector } from "react-redux";
import * as eidsrIntegrationActions from './logic/eidsrIntegrationActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import FormActions from "../forms/formActions/FormActions";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import Form from "../forms/form/Form";
import {Loading} from "../common/loading/Loading";
import {createForm, validators} from "../../utils/forms";
import {ValidationMessage} from "../forms/ValidationMessage";
import {Grid, MenuItem, Typography} from "@material-ui/core";
import TextInputField from "../forms/TextInputField";
import styles from "../common/filters/LocationFilter.module.scss";
import {EidsrIntegrationNotEnabled} from "./EidsrIntegrationNotEnabled";

const EidsrIntegrationEditPageComponent = (props) => {
  const [form, setForm] = useState(null);

  useMount(() => {
    props.getEidsrIntegration(props.nationalSocietyId);
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

    const fields = {
      username: props.data.username ?? "",
      password: props.data.password ?? "", // TODO: it doesn't look like hash, you know?
      apiBaseUrl: props.data.apiBaseUrl ?? "",
      trackerProgramId: props.data.trackerProgramId ?? "",
      locationDataElementId: props.data.locationDataElementId ?? "",
      dateOfOnsetDataElementId: props.data.dateOfOnsetDataElementId ?? "",
      phoneNumberDataElementId: props.data.phoneNumberDataElementId ?? "",
      suspectedDiseaseDataElementId: props.data.suspectedDiseaseDataElementId ?? "",
      eventTypeDataElementId: props.data.eventTypeDataElementId ?? "",
      genderDataElementId: props.data.genderDataElementId ?? "",
    };

    const validation = {
      username: [validators.required],
      password: [validators.required],
      apiBaseUrl: [validators.required],
      trackerProgramId: [validators.required],
      locationDataElementId: [validators.required],
      dateOfOnsetDataElementId: [validators.required],
      phoneNumberDataElementId: [validators.required],
      suspectedDiseaseDataElementId: [validators.required],
      eventTypeDataElementId: [validators.required],
      genderDataElementId: [validators.required],
    };

    setForm(createForm(fields, validation));
  }, [props.data]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();

    props.editEidsrIntegration(props.nationalSocietyId, {
      username: values.username,
      password: values.password, // TODO: it doesn't look like hash, you know?
      apiBaseUrl: values.apiBaseUrl,
      trackerProgramId: values.trackerProgramId,
      locationDataElementId: values.locationDataElementId,
      dateOfOnsetDataElementId: values.dateOfOnsetDataElementId,
      phoneNumberDataElementId: values.phoneNumberDataElementId,
      suspectedDiseaseDataElementId: values.suspectedDiseaseDataElementId,
      eventTypeDataElementId: values.eventTypeDataElementId,
      genderDataElementId: values.genderDataElementId,
    });
  };

  if (props.isFetching || !form) {
    return <Loading />;
  }

  if(!props.isEnabled){
    return <EidsrIntegrationNotEnabled/>;
  }

  return (
    <Fragment>
      {props.formError && !props.formError.data && <ValidationMessage message={props.formError.message} />}

        <Form onSubmit={handleSubmit}>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.userName)}
                name="username"
                field={form.fields.username}
                autoFocus
              />
            </Grid>
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.password)}
                name="password"
                field={form.fields.password}
              />
            </Grid>
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.apiBaseUrl)}
                name="apiBaseUrl"
                field={form.fields.apiBaseUrl}
              />
            </Grid>
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.trackerProgramId)}
                name="trackerProgramId"
                field={form.fields.trackerProgramId}
              />
            </Grid>

            {/* TODO: style this to match mockups */}
            <Grid item xs={12}>
              <hr className={styles.divider} />
              <Typography variant="h5">
                {strings(stringKeys.eidsrIntegration.form.dataElements)}
              </Typography>
            </Grid>

            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.locationDataElementId)}
                name="locationDataElementId"
                field={form.fields.locationDataElementId}
              />
            </Grid>
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.dateOfOnsetDataElementId)}
                name="dateOfOnsetDataElementId"
                field={form.fields.dateOfOnsetDataElementId}
              />
            </Grid>
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.phoneNumberDataElementId)}
                name="phoneNumberDataElementId"
                field={form.fields.phoneNumberDataElementId}
              />
            </Grid>
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.suspectedDiseaseDataElementId)}
                name="suspectedDiseaseDataElementId"
                field={form.fields.suspectedDiseaseDataElementId}
              />
            </Grid>
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.eventTypeDataElementId)}
                name="eventTypeDataElementId"
                field={form.fields.eventTypeDataElementId}
              />
            </Grid>
            <Grid item xs={12}>
              <TextInputField
                label={strings(stringKeys.eidsrIntegration.form.genderDataElementId)}
                name="genderDataElementId"
                field={form.fields.genderDataElementId}
              />
            </Grid>

          </Grid>

          <FormActions>
            <CancelButton onClick={() => props.goToEidsrIntegration(props.nationalSocietyId)}>{ strings(stringKeys.form.cancel) }</CancelButton>
            <SubmitButton isFetching={props.formSaving}> { strings(stringKeys.common.buttons.update) } </SubmitButton>
          </FormActions>

        </Form>

    </Fragment>
  );
}

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,

  data: state.eidsrIntegration.data,
  isFetching: state.eidsrIntegration.isFetching,
  formError: state.eidsrIntegration.formError,
  formSaving: state.eidsrIntegration.formSaving,

  isEnabled: state.appData.siteMap.parameters.nationalSocietyEnableEidsrIntegration,
  callingUserRoles: state.appData.user.roles,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived,
  nationalSocietyHasCoordinator: state.appData.siteMap.parameters.nationalSocietyHasCoordinator
});

const mapDispatchToProps = {
  getEidsrIntegration: eidsrIntegrationActions.get.invoke,
  goToEidsrIntegration: eidsrIntegrationActions.goToEidsrIntegration,
  editEidsrIntegration: eidsrIntegrationActions.edit.invoke,
};

export const EidsrIntegrationEditPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(EidsrIntegrationEditPageComponent)
);
