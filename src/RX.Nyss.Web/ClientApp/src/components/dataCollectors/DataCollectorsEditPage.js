import formStyles from "../forms/form/Form.module.scss";

import React, { useEffect, useState, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as dataCollectorsActions from './logic/dataCollectorsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import MenuItem from "@material-ui/core/MenuItem";
import Button from "@material-ui/core/Button";
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { sexValues, humanDataCollector } from './logic/dataCollectorsConstants';
import { GeoStructureSelect } from './GeoStructureSelect';
import SelectField from '../forms/SelectField';
import { getBirthDecades } from './logic/dataCollectorsService';
import { DataCollectorMap } from './DataCollectorMap';
import { ValidationMessage } from "../forms/ValidationMessage";

const DataCollectorsEditPageComponent = (props) => {
  const [birthDecades] = useState(getBirthDecades());
  const [form, setForm] = useState(null);

  useMount(() => {
    props.openEdition(props.dataCollectorId);
  });

  useEffect(() => {
    if (!props.data) {
      setForm(null);
      return;
    }

    const fields = {
      id: props.data.id,
      name: props.data.name,
      displayName: props.data.dataCollectorType === humanDataCollector ? props.data.displayName : null,
      sex: props.data.dataCollectorType === humanDataCollector ? props.data.sex : null,
      supervisorId: props.data.supervisorId.toString(),
      birthGroupDecade: props.data.dataCollectorType === humanDataCollector ? props.data.birthGroupDecade.toString(): null,
      phoneNumber: props.data.phoneNumber,
      additionalPhoneNumber: props.data.additionalPhoneNumber,
      latitude: props.data.latitude,
      longitude: props.data.longitude,
      villageId: props.data.villageId.toString(),
      districtId: props.data.districtId.toString(),
      regionId: props.data.regionId.toString(),
      zoneId: props.data.zoneId ? props.data.zoneId.toString() : ""
    };

    const validation = {
      name: [validators.required, validators.maxLength(100)],
      displayName: [validators.requiredWhen(x => props.data.dataCollectorType === humanDataCollector), validators.maxLength(100)],
      sex: [validators.requiredWhen(x => props.data.dataCollectorType === humanDataCollector)],
      supervisorId: [validators.required],
      birthGroupDecade: [validators.requiredWhen(x => props.data.dataCollectorType === humanDataCollector)],
      phoneNumber: [validators.required, validators.phoneNumber, validators.maxLength(20)],
      additionalPhoneNumber: [validators.maxLength(20), validators.phoneNumber],
      villageId: [validators.required],
      districtId: [validators.required],
      regionId: [validators.required]
    };

    setForm(createForm(fields, validation));
  }, [props.data, props.match]);

  const onLocationChange = (e) => {
    form.fields.latitude.update(e.lat);
    form.fields.longitude.update(e.lng);
  }

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();

    props.edit(props.projectId, {
      id: values.id,
      dataCollectorType: props.data.dataCollectorType,
      name: values.name,
      displayName: values.displayName,
      sex: values.sex,
      supervisorId: parseInt(values.supervisorId),
      birthGroupDecade: parseInt(values.birthGroupDecade),
      additionalPhoneNumber: values.additionalPhoneNumber,
      latitude: parseFloat(values.latitude),
      longitude: parseFloat(values.longitude),
      phoneNumber: values.phoneNumber,
      villageId: parseInt(values.villageId),
      districtId: parseInt(values.districtId),
      regionId: parseInt(values.regionId),
      zoneId: values.zoneId ? parseInt(values.zoneId) : null
    });
  };

  if (props.isFetching) {
    return <Loading />;
  }

  if (!form || !props.data) {
    return null;
  }

  return (
    <Fragment>
      {props.error &&
        <ValidationMessage
          message={props.error}
        />
      }

      <Form onSubmit={handleSubmit} fullWidth>
        <Grid container spacing={3} className={formStyles.shrinked}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          {props.data.dataCollectorType === humanDataCollector && (<Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.displayName)}
              name="displayName"
              field={form.fields.displayName}
            />
          </Grid>)}

          {props.data.dataCollectorType === humanDataCollector && (<Grid item xs={12}>
            <SelectField
              label={strings(stringKeys.dataCollector.form.sex)}
              field={form.fields.sex}
              name="sex"
            >
              {sexValues.map(type => (
                <MenuItem key={`sex${type}`} value={type}>
                  {strings(stringKeys.dataCollector.constants.sex[type.toLowerCase()])}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>)}

          {props.data.dataCollectorType === humanDataCollector && (<Grid item xs={12}>
            <SelectField
              label={strings(stringKeys.dataCollector.form.birthYearGroup)}
              field={form.fields.birthGroupDecade}
              name="birthGroupDecade"
            >
              {birthDecades.map(decade => (
                <MenuItem key={`birthDecade_${decade}`} value={decade}>
                  {decade}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>)}

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.phoneNumber)}
              name="phoneNumber"
              field={form.fields.phoneNumber}
            />
          </Grid>

          {props.data.dataCollectorType === humanDataCollector && (<Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.additionalPhoneNumber)}
              name="additionalPhoneNumber"
              field={form.fields.additionalPhoneNumber}
            />
          </Grid>)}
        </Grid>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <div>Location</div>
            <DataCollectorMap
              onChange={onLocationChange}
              location={props.data ? { lat: props.data.latitude, lng: props.data.longitude } : null}
              zoom={15}
            />
          </Grid>
        </Grid>
        <Grid container spacing={3} className={formStyles.shrinked}>
          <GeoStructureSelect
            regions={props.data.formData.regions}
            regionIdField={form.fields.regionId}
            districtIdField={form.fields.districtId}
            villageIdField={form.fields.villageId}
            zoneIdField={form.fields.zoneId}
            initialDistricts={props.data.formData.districts}
            initialVillages={props.data.formData.villages}
            initialZones={props.data.formData.zones}
          />

          <Grid item xs={12}>
            <SelectField
              label={strings(stringKeys.dataCollector.form.supervisor)}
              field={form.fields.supervisorId}
              name="supervisorId"
            >
              {props.data.formData.supervisors.map(supervisor => (
                <MenuItem key={`supervisor_${supervisor.id}`} value={supervisor.id.toString()}>
                  {supervisor.name}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>
        </Grid>

        <FormActions className={formStyles.shrinked}>
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
