import formStyles from "../forms/form/Form.module.scss";
import styles from './DataCollectorsCreateOrEditPage.module.scss';

import React, { useEffect, useState, useReducer, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
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
import { sexValues, dataCollectorType } from './logic/dataCollectorsConstants';
import { GeoStructureSelect } from './GeoStructureSelect';
import SelectField from '../forms/SelectField';
import { getBirthDecades } from './logic/dataCollectorsService';
import { DataCollectorMap } from './DataCollectorMap';
import { ValidationMessage } from "../forms/ValidationMessage";
import { TableActionsButton } from "../common/tableActions/TableActionsButton";
import { retrieveGpsLocation } from "../../utils/map";
import { Card, CardContent, InputLabel } from "@material-ui/core";

const DataCollectorsEditPageComponent = (props) => {
  const [birthDecades] = useState(getBirthDecades());
  const [form, setForm] = useState(null);
  const [isFetchingLocation, setIsFetchingLocation] = useState(false);
  const [selectedVillage, setSelectedVillage] = useReducer((state, action) => {
    if (action.initialValue) {
      return { value: action.initialValue, changed: false }
    }
    if (state.value !== action) {
      return { ...state, changed: true, value: action }
    } else {
      return { ...state, changed: false }
    }
  }, { value: "", changed: false });

  const [location, dispatch] = useReducer((state, action) => {
    switch (action.type) {
      case "latitude": return { ...state, lat: action.value };
      case "longitude": return { ...state, lng: action.value };
      case "latlng": return { lat: action.lat, lng: action.lng };
      default: return state;
    }
  }, null);

  useMount(() => {
    props.openEdition(props.dataCollectorId);
  });

  useEffect(() => {
    if (!props.data) {
      setForm(null);
      return;
    }

    dispatch({ type: "latlng", lat: props.data.latitude, lng: props.data.longitude });

    const fields = {
      id: props.data.id,
      name: props.data.name,
      displayName: props.data.dataCollectorType === dataCollectorType.human ? props.data.displayName : null,
      sex: props.data.dataCollectorType === dataCollectorType.human ? props.data.sex : null,
      supervisorId: props.data.supervisorId.toString(),
      birthGroupDecade: props.data.dataCollectorType === dataCollectorType.human ? props.data.birthGroupDecade.toString() : null,
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
      displayName: [validators.requiredWhen(x => props.data.dataCollectorType === dataCollectorType.human), validators.maxLength(100)],
      sex: [validators.requiredWhen(x => props.data.dataCollectorType === dataCollectorType.human)],
      supervisorId: [validators.required],
      birthGroupDecade: [validators.requiredWhen(x => props.data.dataCollectorType === dataCollectorType.human)],
      phoneNumber: [validators.required, validators.phoneNumber, validators.maxLength(20)],
      additionalPhoneNumber: [validators.maxLength(20), validators.phoneNumber],
      villageId: [validators.required],
      districtId: [validators.required],
      regionId: [validators.required],
      longitude: [validators.required, validators.integer, validators.inRange(-180, 180)],
      latitude: [validators.required, validators.integer, validators.inRange(-90, 90)]
    };

    const newForm = createForm(fields, validation);
    newForm.fields.latitude.subscribe(({ newValue }) => dispatch({ type: "latitude", value: newValue }));
    newForm.fields.longitude.subscribe(({ newValue }) => dispatch({ type: "longitude", value: newValue }));

    newForm.fields.villageId.subscribe(({ newValue }) => setSelectedVillage(newValue));
    setSelectedVillage({ initialValue: props.data.villageId.toString() });
    setForm(newForm);
  }, [props.data, props.match]);

  useEffect(() => {
    if (form && form.fields && selectedVillage.changed) {
      form.fields.latitude.update("");
      form.fields.longitude.update("");
    }
  }, [form, selectedVillage])

  const onLocationChange = (e) => {
    form.fields.latitude.update(e.lat);
    form.fields.longitude.update(e.lng);
  }

  const onRetrieveLocation = () => {
    setIsFetchingLocation(true);
    retrieveGpsLocation(location => {
      if (location === null) {
        setIsFetchingLocation(false);
        return;
      }
      const lat = location.coords.latitude;
      const lng = location.coords.longitude;
      form.fields.latitude.update(lat);
      form.fields.longitude.update(lng);
      dispatch({ type: "latlng", lat: lat, lng: lng });
      setIsFetchingLocation(false);
    });
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

  useCustomErrors(form, props.error);

  if (props.isFetching) {
    return <Loading />;
  }

  if (!form || !props.data) {
    return null;
  }

  return (
    <Fragment>
      {props.error && !props.error.data && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit} fullWidth>
        <Grid container spacing={2} className={formStyles.shrinked}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          {props.data.dataCollectorType === dataCollectorType.human && (<Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.displayName)}
              name="displayName"
              field={form.fields.displayName}
            />
          </Grid>)}

          {props.data.dataCollectorType === dataCollectorType.human && (<Grid item xs={12}>
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

          {props.data.dataCollectorType === dataCollectorType.human && (<Grid item xs={12}>
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

          {props.data.dataCollectorType === dataCollectorType.human && (<Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.additionalPhoneNumber)}
              name="additionalPhoneNumber"
              field={form.fields.additionalPhoneNumber}
            />
          </Grid>)}

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

        </Grid>

        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Card className={styles.requiredMapLocation} data-missing-location={form.fields.latitude.error !== null || form.fields.longitude.error !== null}>
              <CardContent>
                <Grid container spacing={2}>
                  <Grid item xs={12}>
                    <InputLabel style={{ marginBottom: "10px" }}>{strings(stringKeys.dataCollector.form.selectLocation)}</InputLabel>
                    <DataCollectorMap
                      onChange={onLocationChange}
                      location={location}
                      zoom={6}
                    />
                  </Grid>
                  <Grid item className={styles.locationButton}>
                    <TableActionsButton
                      onClick={onRetrieveLocation}
                      isFetching={isFetchingLocation}
                    >
                      {strings(stringKeys.dataCollector.form.retrieveLocation)}
                    </TableActionsButton>
                  </Grid>
                  <Grid item xs={12} md={3} style={{maxWidth: "190px"}}>
                    <TextInputField
                      label={strings(stringKeys.dataCollector.form.latitude)}
                      name="latitude"
                      field={form.fields.latitude}
                      type="number"
                    />
                  </Grid>
                  <Grid item xs={12} md={3} style={{maxWidth: "190px"}}>
                    <TextInputField
                      label={strings(stringKeys.dataCollector.form.longitude)}
                      name="longitude"
                      field={form.fields.longitude}
                      type="number"
                    />
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>
        </Grid>

        <Grid container spacing={2} className={formStyles.shrinked}>
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
