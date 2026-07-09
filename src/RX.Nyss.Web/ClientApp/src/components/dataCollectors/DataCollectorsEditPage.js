import React, { useEffect, useState, Fragment, createRef } from "react";
import { connect, useSelector } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as dataCollectorsActions from "./logic/dataCollectorsActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import TextInputField from "../forms/TextInputField";
import PhoneInputField from "../forms/PhoneInputField";
import { Loading } from "../common/loading/Loading";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { sexValues, dataCollectorType } from "./logic/dataCollectorsConstants";
import SelectField from "../forms/SelectField";
import { getSaveFormModel } from "./logic/dataCollectorsService";
import { ValidationMessage } from "../forms/ValidationMessage";
import { MenuItem, makeStyles, Grid, Typography, useTheme, IconButton } from "@material-ui/core";
import { HeadSupervisor, Supervisor } from "../../authentication/roles";
import CheckboxField from "../forms/CheckboxField";
import { DataCollectorLocationItem } from "./components/DataCollectorLocationItem";
import { getBirthDecades } from "../../utils/birthYear";
import { SubMenuTitle } from "../layout/SubMenuTitle";
import { trackPageView } from "../../utils/appInsightsHelper";
import AddCircleOutlineIcon from '@material-ui/icons/AddCircleOutline';

const useStyles = makeStyles((theme) => ({
  addLocationContainer: {
    marginTop: 20,
    width: "fit-content",
    "&:hover": {
      cursor: "pointer",
    }
  }
}));

const DataCollectorsEditPageComponent = (props) => {
  const currentUserRoles = useSelector((state) => state.appData.user.roles);
  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );
  const [birthDecades] = useState(getBirthDecades());
  const [form, setForm] = useState(null);
  const [locations, setLocations] = useState(null);
  const [centerLocation, setCenterLocation] = useState(null);
  const [allLocationsCollapsed, setAllLocationsCollapsed] = useState(true);
  const classes = useStyles();
  const theme = useTheme();

  useMount(() => {
    props.openEdition(props.dataCollectorId);
    // Track page view
    trackPageView("DataCollectorsEditPage");
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
      displayName:
        props.data.dataCollectorType === dataCollectorType.human
          ? props.data.displayName
          : null,
      sex:
        props.data.dataCollectorType === dataCollectorType.human
          ? props.data.sex
          : null,
      supervisorId: props.data.supervisorId.toString(),
      phoneNumber: props.data.phoneNumber,
      additionalPhoneNumber: props.data.additionalPhoneNumber,
      deployed: props.data.deployed,
      linkedToHeadSupervisor:
        props.data.formData.supervisors.filter(
          (s) => s.id === props.data.supervisorId,
        )[0].role === HeadSupervisor,
    };

    const validation = {
      name: [validators.required, validators.maxLength(100)],
      displayName: [
        validators.requiredWhen(
          (x) => props.data.dataCollectorType === dataCollectorType.human,
        ),
        validators.maxLength(100),
      ],
      sex: [
        validators.requiredWhen(
          (x) => props.data.dataCollectorType === dataCollectorType.human,
        ),
      ],
      supervisorId: [validators.required],
      phoneNumber: [validators.required, validators.phoneNumber, validators.maxLength(20)],
      additionalPhoneNumber: [validators.phoneNumber, validators.maxLength(20)],
    };

    const refs = {
      dataCollectorType: createRef(),
      name: createRef(),
      displayName: createRef(),
      sex: createRef(),
      supervisorId: createRef(),
      phoneNumber: createRef(),
      additionalPhoneNumber: createRef(),
      deployed: createRef(),
      linkedToHeadSupervisor: createRef(),
    };

    const newForm = createForm(fields, validation, refs);
    newForm.fields.supervisorId.subscribe(({ newValue }) =>
      newForm.fields.linkedToHeadSupervisor.update(
        props.data.formData.supervisors.filter(
          (s) => s.id.toString() === newValue,
        )[0].role === HeadSupervisor,
      ),
    );

    setForm(newForm);
  }, [props.data, props.match]);

  const addDataCollectorLocation = () => {
    const previousLocation = locations[locations.length - 1];
    setAllLocationsCollapsed(false);
    setLocations([
      ...locations,
      {
        latitude: "",
        longitude: "",
        regionId: "",
        districtId: "",
        villageId: "",
        zoneId: "",
        initialFormData: {
          districts: previousLocation.initialFormData.districts,
          villages: [],
          zones: [],
        },
        number: previousLocation.number + 1,
      },
    ]);
  };

  const removeDataCollectorLocation = (location) => {
    setLocations(locations.filter((l) => l.number !== location.number));
  };

  useEffect(() => {
    if (locations && locations.length > 0) {
      setCenterLocation({
        latitude: locations[0].latitude,
        longitude: locations[0].longitude,
      });
    }
  }, [locations]);

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.isValid()) {
      Object.values(form.fields)
        .filter((e) => e.error)[0]
        .scrollTo();
      return;
    }

    const values = form.getValues();

    props.edit(
      getSaveFormModel(
        props.projectId,
        values,
        props.data.dataCollectorType,
        locations,
      ),
    );
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
      <SubMenuTitle />
      {props.error && !props.error.data && (
        <ValidationMessage message={props.error.message} />
      )}
      <Form onSubmit={handleSubmit} fullWidth>
        <Grid container>
          <Grid item xs={12}>
            <Typography variant="h5">
              {strings(stringKeys.dataCollectors.filters.deployedMode)}
            </Typography>
          </Grid>
          <Grid item xs={12}>
            <CheckboxField
              name="deployed"
              label={strings(stringKeys.dataCollectors.form.deployed)}
              field={form.fields.deployed}
              color="primary"
            />
          </Grid>
        </Grid>
        <Typography variant="h5" style={{ marginTop: 50 }}>
          {strings(stringKeys.dataCollectors.form.personalia)}
        </Typography>

        <Grid container spacing={4}>
          <Grid container item xs={12} spacing={4}>
            <Grid item xs={12} md={3}>
              <TextInputField
                label={strings(stringKeys.common.name)}
                name="name"
                field={form.fields.name}
                fieldRef={form.fields.name.ref}
              />
            </Grid>
            {props.data.dataCollectorType === dataCollectorType.human && (
              <Grid item xs={12} md={3}>
                <TextInputField
                  label={strings(stringKeys.dataCollectors.form.displayName)}
                  name="displayName"
                  field={form.fields.displayName}
                  fieldRef={form.fields.displayName.ref}
                />
              </Grid>
            )}

            {props.data.dataCollectorType === dataCollectorType.human && (
              <Grid item xs={12} md={3}>
                <SelectField
                  label={strings(stringKeys.dataCollectors.form.sex)}
                  field={form.fields.sex}
                  fieldRef={form.fields.sex.ref}
                  name="sex"
                >
                  {sexValues.map((type) => (
                    <MenuItem key={`sex${type}`} value={type}>
                      {strings(
                        stringKeys.dataCollectors.constants.sex[
                        type.toLowerCase()
                        ],
                      )}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>
            )}
          </Grid>
        </Grid>
        <Grid container item xs={12} spacing={4} style={{ marginTop: 16 }}>
          <Grid item xs={12} md={3}>
            <PhoneInputField
              label={strings(stringKeys.dataCollectors.form.phoneNumber)}
              name="phoneNumber"
              fieldRef={form.fields.phoneNumber.ref}
              field={form.fields.phoneNumber}
              defaultCountry={props.data.nationalSocietyCountryCode}
              rtl={useRtlDirection}
            />
          </Grid>
          <Grid item xs={12} md={3}>
            <PhoneInputField
              label={strings(
                stringKeys.dataCollectors.form.additionalPhoneNumber,
              )}
              name="additionalPhoneNumber"
              fieldRef={form.fields.additionalPhoneNumber.ref}
              field={form.fields.additionalPhoneNumber}
              defaultCountry={props.data.nationalSocietyCountryCode}
              rtl={useRtlDirection}
            />
          </Grid>
        </Grid>
        {!currentUserRoles.some((r) => r === Supervisor) && (
          <Grid container item xs={12} direction="column" style={{ marginTop: 62 }}>
            <Typography variant="h5">{strings(stringKeys.dataCollectors.form.responsibleSupervisor)}</Typography>
            <Grid item md={3}>
              <SelectField
                label={strings(stringKeys.dataCollectors.form.supervisor)}
                field={form.fields.supervisorId}
                name="supervisorId"
                fieldRef={form.fields.supervisorId.ref}
              >
                {props.data.formData.supervisors.map((supervisor) => (
                  <MenuItem
                    key={`supervisor_${supervisor.id}`}
                    value={supervisor.id.toString()}
                  >
                    {supervisor.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          </Grid>
        )}
        <Typography variant="h5" style={{ marginTop: 50 }}>
          {strings(stringKeys.dataCollectors.form.locationsHeader)}
        </Typography>
        <Grid container spacing={2} style={{ marginTop: 5 }}>
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
              rtl={useRtlDirection}
            />
          ))}
        </Grid>
        <Grid className={classes.addLocationContainer} onClick={addDataCollectorLocation} container alignItems="center">
          <IconButton
            color="primary"
            variant="outlined"
          >
            <AddCircleOutlineIcon />
          </IconButton>
          <Typography style={{ color: theme.palette.primary.main }}>{strings(stringKeys.dataCollectors.form.addLocation)}</Typography>
        </Grid>
        <FormActions>
          <CancelButton
            variant="outlined"
            onClick={() => props.goToList(props.projectId)}
          >
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton isFetching={props.isSaving}>
            {strings(stringKeys.common.buttons.update)}
          </SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
};

DataCollectorsEditPageComponent.propTypes = {};

const mapStateToProps = (state, ownProps) => ({
  dataCollectorId: ownProps.match.params.dataCollectorId,
  projectId: ownProps.match.params.projectId,
  isFetching: state.dataCollectors.formFetching,
  isSaving: state.dataCollectors.formSaving,
  data: state.dataCollectors.formData,
  error: state.dataCollectors.formError,
});

const mapDispatchToProps = {
  openEdition: dataCollectorsActions.openEdition.invoke,
  edit: dataCollectorsActions.edit.invoke,
  goToList: dataCollectorsActions.goToList,
};

export const DataCollectorsEditPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsEditPageComponent),
);
