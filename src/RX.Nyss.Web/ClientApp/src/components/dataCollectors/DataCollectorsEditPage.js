import formStyles from "../forms/form/Form.module.scss";
import styles from "./DataCollectorsCreateOrEditPage.module.scss";
import React, { useEffect, useState, Fragment } from 'react';
import { connect, useSelector } from "react-redux";
import { withLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as dataCollectorsActions from './logic/dataCollectorsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { sexValues, dataCollectorType } from './logic/dataCollectorsConstants';
import SelectField from '../forms/SelectField';
import { getSaveFormModel } from './logic/dataCollectorsService';
import { ValidationMessage } from "../forms/ValidationMessage";
import { MenuItem, Button, Grid, Typography } from "@material-ui/core";
import { Supervisor } from "../../authentication/roles";
import CheckboxField from "../forms/CheckboxField";
import { DataCollectorLocationItem } from "./DataCollectorLocationItem";
import { getBirthDecades, parseBirthDecade } from "../../utils/birthYear";

const DataCollectorsEditPageComponent = (props) => {
  const currentUserRoles = useSelector(state => state.appData.user.roles);
  const [birthDecades] = useState(getBirthDecades());
  const [form, setForm] = useState(null);
  const [locations, setLocations] = useState(null);
  const [centerLocation, setCenterLocation] = useState(null);
  const [allLocationsCollapsed, setAllLocationsCollapsed] = useState(true);

  useMount(() => {
    props.openEdition(props.dataCollectorId);
  });

  useEffect(() => {
    if (!props.data) {
      setForm(null);
      return;
    }

    setLocations(props.data.locations.map((l, i) => ({ ...l, number: i })));

    const fields = {
      id: props.data.id,
      name: props.data.name,
      displayName: props.data.dataCollectorType === dataCollectorType.human ? props.data.displayName : null,
      sex: props.data.dataCollectorType === dataCollectorType.human ? props.data.sex : null,
      supervisorId: props.data.supervisorId.toString(),
      birthGroupDecade: props.data.dataCollectorType === dataCollectorType.human ? props.data.birthGroupDecade.toString() : null,
      phoneNumber: props.data.phoneNumber,
      additionalPhoneNumber: props.data.additionalPhoneNumber,
      deployed: props.data.deployed
    };

    const validation = {
      name: [validators.required, validators.maxLength(100)],
      displayName: [validators.requiredWhen(x => props.data.dataCollectorType === dataCollectorType.human), validators.maxLength(100)],
      sex: [validators.requiredWhen(x => props.data.dataCollectorType === dataCollectorType.human)],
      supervisorId: [validators.required],
      birthGroupDecade: [validators.requiredWhen(x => props.data.dataCollectorType === dataCollectorType.human)],
      phoneNumber: [validators.phoneNumber, validators.maxLength(20)],
      additionalPhoneNumber: [validators.maxLength(20), validators.phoneNumber]
    };

    const newForm = createForm(fields, validation);
    setForm(newForm);
  }, [props.data, props.match]);

  const addDataCollectorLocation = () => {
    const previousLocation = locations[locations.length - 1];
    setAllLocationsCollapsed(false);
    setLocations([...locations, {
      latitude: '',
      longitude: '',
      regionId: previousLocation.regionId,
      districtId: previousLocation.districtId,
      villageId: '',
      zoneId: '',
      initialFormData: {
        districts: previousLocation.initialFormData.districts,
        villages: [],
        zones: []
      },
      number: previousLocation.number + 1
    }]);
  }

  const removeDataCollectorLocation = (location) => {
    setLocations(locations.filter(l => l.number !== location.number));
  }

  useEffect(() => {
    if (locations && locations.length > 0) {
      setCenterLocation({
        latitude: locations[0].latitude,
        longitude: locations[0].longitude
      });
    }
  }, [locations]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();

    props.edit(props.projectId, getSaveFormModel(values, props.data.dataCollectorType, locations));
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
        <Grid item xs={12}>
          <Typography variant="h6">{strings(stringKeys.dataCollector.filters.deployedMode)}</Typography>
        </Grid>

        <Grid item xs={12}>
          <CheckboxField
            name="deployed"
            label={strings(stringKeys.dataCollector.form.deployed)}
            field={form.fields.deployed}
            color="primary"
          />
        </Grid>


        <Grid item xs={12}>
          <Typography variant="h6">{strings(stringKeys.dataCollector.form.personalia)}</Typography>
        </Grid>

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
                  {parseBirthDecade(decade)}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>)}

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.phoneNumber)}
              name="phoneNumber"
              field={form.fields.phoneNumber}
              inputMode={"tel"}
            />
          </Grid>

          {props.data.dataCollectorType === dataCollectorType.human && (<Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.dataCollector.form.additionalPhoneNumber)}
              name="additionalPhoneNumber"
              field={form.fields.additionalPhoneNumber}
              inputMode={"tel"}
            />
          </Grid>)}

          {!currentUserRoles.some(r => r === Supervisor) && (<Grid item xs={12}>
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
          </Grid>)}
        </Grid>

        <Grid container spacing={2} className={styles.locationsContainer}>
          {locations.map((location, i) => (
            <DataCollectorLocationItem
              key={`location_${location.number}`}
              form={form}
              location={location}
              locationNumber={location.number}
              isLastLocation={i === locations.length - 1}
              isOnlyLocation={locations.length === 1}
              defaultLocation={centerLocation}
              regions={props.data.formData.regions}
              initialDistricts={location.initialFormData.districts}
              initialVillages={location.initialFormData.villages}
              initialZones={location.initialFormData.zones}
              isDefaultCollapsed={allLocationsCollapsed}
              removeLocation={removeDataCollectorLocation}
              allLocations={locations}
            />
          ))}

          <Button color='primary' onClick={addDataCollectorLocation}>{strings(stringKeys.dataCollector.form.addLocation)}</Button>
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

export const DataCollectorsEditPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsEditPageComponent)
);
