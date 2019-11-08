import React, { useEffect, useState, Fragment } from 'react';
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
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { sexValues } from './logic/dataCollectorsConstants';

const DataCollectorsEditPageComponent = (props) => {
  const [form, setForm] = useState(null);

  useMount(() => {
    props.openEdition(props.dataCollectorId);
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

    const fields = {
      id: props.data.id,
      name: props.data.name,
      displayName: props.data.displayName,
      sex: props.data.sex,
      supervisorId: props.data.supervisorId,
      dataCollectorType: props.data.dataCollectorType,
      birthYearGroup: props.data.birthYearGroup,
      additionalPhoneNumber: props.data.additionalPhoneNumber,
      latitude: props.data.latitude,
      longitude: props.data.longitude,
      phoneNumber: props.data.phoneNumber,
      village: props.data.village,
      district: props.data.district,
      region: props.data.region,
      zone: props.data.zone
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
      zone: []
    };

    setForm(createForm(fields, validation));
  }, [props.data, props.match]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    props.edit(props.projectId, form.getValues());
  };

  if (props.isFetching) {
    return <Loading />;
  }

  if (!form) {
    return null;
  }

  return (
    <Fragment>
      <Typography variant="h2">{strings(stringKeys.dataCollector.form.editionTitle)}</Typography>

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
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.dataCollector.form.update)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

DataCollectorsEditPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  dataCollectorId: ownProps.match.params.dataCollectorId,
  projectId: ownProps.match.params.projectId,
  isFetching: state.dataCollectors.formFetching,
  isSaving: state.dataCollectors.formSaving,
  data: state.dataCollectors.formData,
  error: state.dataCollectors.formError
});

const mapDispatchToProps = {
  openEdition: dataCollectorsActions.openEdition.invoke,
  edit: dataCollectorsActions.edit.invoke,
  goToList: dataCollectorsActions.goToList
};

export const DataCollectorsEditPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsEditPageComponent)
);
