import formStyles from "../forms/form/Form.module.scss";
import styles from './DataCollectorsCreatePage.module.scss';

import React, { useState, Fragment, useEffect, useReducer } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as dataCollectorsActions from './logic/dataCollectorsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import SelectField from '../forms/SelectField';
import RadioGroupField from '../forms/RadioGroupField';
import MenuItem from "@material-ui/core/MenuItem";
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { sexValues, dataCollectorType } from './logic/dataCollectorsConstants';
import { GeoStructureSelect } from './GeoStructureSelect';
import { getBirthDecades } from './logic/dataCollectorsService';
import { DataCollectorMap } from './DataCollectorMap';
import { Loading } from '../common/loading/Loading';
import { ValidationMessage } from "../forms/ValidationMessage";
import { Radio, FormControlLabel } from "@material-ui/core";
import { TableActionsButton } from "../common/tableActions/TableActionsButton";
import { retrieveGpsLocation } from "../../utils/map";

const DataCollectorsCreatePageComponent = (props) => {
  const [birthDecades] = useState(getBirthDecades());
  const [form, setForm] = useState(null);
  const [type, setType] = useState(dataCollectorType.human);
  const [isFetchingLocation, setIsFetchingLocation] = useState(false);

  const [location, dispatch] = useReducer((state, action) => {
    switch (action.type) {
      case "latitude": return { ...state, lat: action.value };
      case "longitude": return { ...state, lng: action.value };
      case "latlng": return { lat: action.lat, lng: action.lng };
      default: return state;
    }
  }, null);

  useMount(() => {
    props.openCreation(props.projectId);
  })

  useEffect(() => {
    if (!props.defaultLocation) {
      setForm(null);
      return;
    }

    dispatch({ type: "latlng", lat: props.defaultLocation.latitude, lng: props.defaultLocation.longitude });

    const fields = {
      dataCollectorType: dataCollectorType.human,
      name: "",
      displayName: "",
      sex: "",
      supervisorId: props.defaultSupervisorId ? props.defaultSupervisorId.toString() : "",
      birthGroupDecade: "",
      phoneNumber: "",
      additionalPhoneNumber: "",
      latitude: props.defaultLocation.latitude,
      longitude: props.defaultLocation.longitude,
      villageId: "",
      districtId: "",
      regionId: "",
      zoneId: ""
    };

    const validation = {
      dataCollectorType: [validators.required],
      name: [validators.required, validators.maxLength(100)],
      displayName: [validators.requiredWhen(x => x.dataCollectorType === dataCollectorType.human), validators.maxLength(100)],
      sex: [validators.requiredWhen(x => x.dataCollectorType === dataCollectorType.human)],
      supervisorId: [validators.required],
      birthGroupDecade: [validators.requiredWhen(x => x.dataCollectorType === dataCollectorType.human)],
      phoneNumber: [validators.required, validators.phoneNumber, validators.maxLength(20)],
      additionalPhoneNumber: [validators.maxLength(20), validators.phoneNumber],
      villageId: [validators.required],
      districtId: [validators.required],
      regionId: [validators.required],
      longitude: [validators.required, validators.integer, validators.inRange(-180, 180)],
      latitude: [validators.required, validators.integer, validators.inRange(-90, 90)]
    };

    const newForm = createForm(fields, validation);
    setForm(newForm);
    newForm.fields.dataCollectorType.subscribe(({newValue}) => setType(newValue));
    newForm.fields.latitude.subscribe(({newValue}) => dispatch({ type: "latitude", value: newValue}));
    newForm.fields.longitude.subscribe(({newValue}) => dispatch({ type: "longitude", value: newValue}));
  }, [props.regions, props.defaultSupervisorId, props.defaultLocation]);

  const onLocationChange = (e) => {
    form.fields.latitude.update(e.lat);
    form.fields.longitude.update(e.lng);
  }

  const onRetrieveLocation = () => {
    setIsFetchingLocation(true);
    retrieveGpsLocation(location => {
      if (location === null){
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

    props.create(props.projectId, {
      dataCollectorType: values.dataCollectorType,
      name: values.name,
      displayName: values.displayName,
      sex: values.sex === "" ? null : values.sex,
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

  if (!form) {
    return null;
  }

  return (
    <Fragment>
      {props.error && !props.error.data && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit} fullWidth>
        <Grid container spacing={3} className={formStyles.shrinked}>
          <Grid item xs={12}>
            <RadioGroupField
              name="dataCollectorType"
              label={strings(stringKeys.dataCollector.form.dataCollectorType)}
              field={form.fields.dataCollectorType}
              horizontal >
              {Object.keys(dataCollectorType).map(type => (
                <FormControlLabel key={type} control={<Radio />} label={strings(stringKeys.dataCollector.constants.dataCollectorType[dataCollectorType[type]])} value={dataCollectorType[type]} />
              ))}
            </RadioGroupField>
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          {type === dataCollectorType.human && (<Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.displayName)}
              name="displayName"
              field={form.fields.displayName}
            />
          </Grid>)}

          {type === dataCollectorType.human && (
            <Grid item xs={12}>
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
          </Grid>
          )}

          {type === dataCollectorType.human && (<Grid item xs={12}>
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

          {type === dataCollectorType.human && (<Grid item xs={12}>
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
              location={location}
              zoom={6}
            />
          </Grid>

        </Grid>
        <Grid container spacing={3} className={formStyles.shrinked}>

          <Grid item className={styles.locationButton}>
            <TableActionsButton
              onClick={onRetrieveLocation}
              isFetching={isFetchingLocation}
            >
              {strings(stringKeys.dataCollector.form.retrieveLocation)}
            </TableActionsButton>
          </Grid>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.latitude)}
              name="latitude"
              field={form.fields.latitude}
              type="number"
            />
          </Grid>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.longitude)}
              name="longitude"
              field={form.fields.longitude}
              type="number"
            />
          </Grid>

          <GeoStructureSelect
            regions={props.regions}
            regionIdField={form.fields.regionId}
            districtIdField={form.fields.districtId}
            villageIdField={form.fields.villageId}
            zoneIdField={form.fields.zoneId}
          />

          <Grid item xs={12}>
            <SelectField
              label={strings(stringKeys.dataCollector.form.supervisor)}
              field={form.fields.supervisorId}
              name="supervisorId"
            >
              {props.supervisors.map(supervisor => (
                <MenuItem key={`supervisor_${supervisor.id}`} value={supervisor.id.toString()}>
                  {supervisor.name}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>
        </Grid>

        <FormActions className={formStyles.shrinked}>
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
  countryName: state.appData.siteMap.parameters.nationalSocietyCountry,
  isFetching: state.dataCollectors.formFetching,
  isSaving: state.dataCollectors.formSaving,
  regions: state.dataCollectors.formRegions,
  defaultLocation: state.dataCollectors.formDefaultLocation,
  supervisors: state.dataCollectors.formSupervisors,
  defaultSupervisorId: state.dataCollectors.formDefaultSupervisorId,
  isGettingCountryLocation: state.dataCollectors.gettingLocation,
  country: state.dataCollectors.countryData,
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
