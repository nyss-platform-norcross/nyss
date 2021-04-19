import formStyles from "../forms/form/Form.module.scss";
import styles from "./DataCollectorsCreateOrEditPage.module.scss";
import React, { useState, Fragment, useMemo } from 'react';
import { connect, useSelector } from "react-redux";
import {
  Radio,
  FormControlLabel,
  MenuItem,
  Button,
  Grid,
  Typography,
} from "@material-ui/core";
import { withLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as dataCollectorsActions from './logic/dataCollectorsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import SelectField from '../forms/SelectField';
import RadioGroupField from '../forms/RadioGroupField';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { sexValues, dataCollectorType } from './logic/dataCollectorsConstants';
import { getBirthDecades, getSaveFormModel } from './logic/dataCollectorsService';
import { Loading } from '../common/loading/Loading';
import { ValidationMessage } from "../forms/ValidationMessage";
import { Supervisor } from "../../authentication/roles";
import CheckboxField from "../forms/CheckboxField";
import { DataCollectorLocationItem } from "./DataCollectorLocationItem";

const DataCollectorsCreatePageComponent = (props) => {
  const currentUserRoles = useSelector(state => state.appData.user.roles);
  const [birthDecades] = useState(getBirthDecades());
  const [type, setType] = useState(dataCollectorType.human);
  const [centerLocation, setCenterLocation] = useState(null);
  const [locations, setLocations] = useState([{
    latitude: '',
    longitude: '',
    regionId: '',
    districtId: '',
    villageId: '',
    zoneId: '',
    number: 0
  }]);
  const [initialDistricts, setInitialDistricts] = useState([]);

  useMount(() => {
    props.openCreation(props.projectId);
  })

  const form = useMemo(() => {
    if (!props.defaultLocation) {
      return null;
    }

    setCenterLocation({ latitude: props.defaultLocation.latitude, longitude: props.defaultLocation.longitude });

    const fields = {
      dataCollectorType: dataCollectorType.human,
      name: "",
      displayName: "",
      sex: "",
      supervisorId: props.defaultSupervisorId ? props.defaultSupervisorId.toString() : "",
      birthGroupDecade: "",
      phoneNumber: "",
      additionalPhoneNumber: "",
      deployed: true
    };

    const validation = {
      dataCollectorType: [validators.required],
      name: [validators.required, validators.maxLength(100)],
      displayName: [validators.requiredWhen(x => x.dataCollectorType === dataCollectorType.human), validators.maxLength(100)],
      sex: [validators.requiredWhen(x => x.dataCollectorType === dataCollectorType.human)],
      supervisorId: [validators.required],
      birthGroupDecade: [validators.requiredWhen(x => x.dataCollectorType === dataCollectorType.human)],
      phoneNumber: [validators.required, validators.phoneNumber, validators.maxLength(20)],
      additionalPhoneNumber: [validators.maxLength(20), validators.phoneNumber]
    };

    const newForm = createForm(fields, validation);
    newForm.fields.dataCollectorType.subscribe(({ newValue }) => setType(newValue));
    return newForm;
  }, [props.defaultSupervisorId, props.defaultLocation]);

  const addDataCollectorLocation = () => {
    const previousRegionId = locations.length > 0 ? form.fields[`location_${locations.length - 1}_regionId`].value : '';
    const previousDistrictId = locations.length > 0 ? form.fields[`location_${locations.length - 1}_districtId`].value : '';
    setLocations([...locations, {
      latitude: '',
      longitude: '',
      regionId: previousRegionId,
      districtId: previousDistrictId,
      villageId: '',
      zoneId: '',
      number: locations.length
    }]);
  }

  const removeDataCollectorLocation = (location) => {
    setLocations(locations.filter(l => l.number !== location.number));
  }

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();

    props.create(props.projectId, getSaveFormModel(values, values.dataCollectorType, locations));
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
        <Grid container spacing={2} className={formStyles.shrinked}>
          <Grid item xs={12}>
            <RadioGroupField
              name="dataCollectorType"
              label={strings(stringKeys.dataCollector.form.dataCollectorType)}
              boldLabel
              field={form.fields.dataCollectorType}
              horizontal >
              {Object.keys(dataCollectorType).map(type => (
                <FormControlLabel key={type} control={<Radio />} label={strings(stringKeys.dataCollector.constants.dataCollectorType[dataCollectorType[type]])} value={dataCollectorType[type]} />
              ))}
            </RadioGroupField>
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
              inputMode={"tel"}
            />
          </Grid>

          {type === dataCollectorType.human && (<Grid item xs={12}>
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
              {props.supervisors.map(supervisor => (
                <MenuItem key={`supervisor_${supervisor.id}`} value={supervisor.id.toString()}>
                  {supervisor.name}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>)}
        </Grid>

        <Grid container spacing={2} className={styles.locationsContainer}>
          <Grid item xs={12}>
            <Typography variant="h6">{strings(stringKeys.dataCollector.form.locationsHeader)}</Typography>
          </Grid>

          {locations.map(location => (
            <DataCollectorLocationItem
              key={`location_${location.number}`}
              form={form}
              location={location}
              locationNumber={location.number}
              totalLocations={locations.length}
              defaultLocation={centerLocation}
              regions={props.regions}
              initialDistricts={initialDistricts}
              initialVillages={[]}
              initialZones={[]}
              isDefaultCollapsed={false}
              setInitialDistricts={setInitialDistricts}
              removeLocation={removeDataCollectorLocation}
            />
          ))}

          <Button color='primary' onClick={addDataCollectorLocation}>{strings(stringKeys.dataCollector.form.addLocation)}</Button>
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

export const DataCollectorsCreatePage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsCreatePageComponent)
);
