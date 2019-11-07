import React, { useState, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as dataCollectorsActions from './logic/dataCollectorsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import SelectInput from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import SnackbarContent from '@material-ui/core/SnackbarContent';
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { sexValues } from './logic/dataCollectorsConstants';

const DataCollectorsCreatePageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      name: "",
      displayName: "",
      sex: "",
      supervisorId: "",
      dataCollectorType: "",
      birthYearGroup: "",
      additionalPhoneNumber: "",
      latitude: "",
      longitude: "",
      phoneNumber: "",
      village: "",
      district: "",
      region: "",
      zone: ""
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      displayName: [validators.required, validators.minLength(1), validators.maxLength(100)],
      sex: [validators.required],
      supervisorId: [validators.required],
      dataCollectorType: [validators.required],
      birthYearGroup: [validators.required],
      additionalPhoneNumber: [validators.required],
      latitude: [validators.required],
      longitude: [validators.required],
      phoneNumber: [validators.required],
      village: [validators.required],
      district: [validators.required],
      region: [validators.required],
      zone: [validators.required]
    };

    return createForm(fields, validation);
  });

  useMount(() => {
    props.openCreation(props.projectId);
  })

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    props.create(props.projectId, form.getValues());
  };

  return (
    <Fragment>
      <Typography variant="h2">{strings(stringKeys.dataCollector.form.creationTitle)}</Typography>

      {props.error &&
        <SnackbarContent
          message={props.error}
        />
      }

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.displayName)}
              name="displayName"
              field={form.fields.displayName}
            />
          </Grid>
          <Grid item xs={12}>
            <SelectInput
              label={strings(stringKeys.dataCollector.form.sex)}
              field={form.fields.sex}
              name="sex"
            >
              {sexValues.map(type => (
                <MenuItem key={`sex${type}`} value={type}>
                  {strings(stringKeys.dataCollector.constants.sex[type.toLowerCase()])}
                </MenuItem>
              ))}
            </SelectInput>
          </Grid>

          <Grid item xs={6}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.latitude)}
              name="latitude"
              field={form.fields.latitude}
            />
          </Grid>

          <Grid item xs={6}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.longitude)}
              name="longitude"
              field={form.fields.longitude}
            />
          </Grid>
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList(props.projectId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.dataCollector.form.create)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

DataCollectorsCreatePageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  isSaving: state.dataCollectors.formSaving,
  error: state.dataCollectors.formError
});

const mapDispatchToProps = {
  openCreation: dataCollectorsActions.openCreation.invoke,
  create: dataCollectorsActions.create.invoke,
  goToList: dataCollectorsActions.goToList
};

export const DataCollectorsCreatePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsCreatePageComponent)
);
