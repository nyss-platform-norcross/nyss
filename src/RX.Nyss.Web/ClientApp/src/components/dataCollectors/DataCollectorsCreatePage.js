import { useState, Fragment, useMemo, createRef } from "react";
import { connect, useSelector } from "react-redux";
import {
  Radio,
  FormControlLabel,
  MenuItem,
  makeStyles,
  Grid,
  Typography,
  IconButton,
  useTheme,
} from "@material-ui/core";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as dataCollectorsActions from "./logic/dataCollectorsActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import TextInputField from "../forms/TextInputField";
import SelectField from "../forms/SelectField";
import RadioGroupField from "../forms/RadioGroupField";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { sexValues, dataCollectorType } from "./logic/dataCollectorsConstants";
import { getSaveFormModel } from "./logic/dataCollectorsService";
import { Loading } from "../common/loading/Loading";
import { ValidationMessage } from "../forms/ValidationMessage";
import { HeadSupervisor, Supervisor } from "../../authentication/roles";
import CheckboxField from "../forms/CheckboxField";
import PhoneInputField from "../forms/PhoneInputField";
import { DataCollectorLocationItem } from "./components/DataCollectorLocationItem";
import { getBirthDecades, parseBirthDecade } from "../../utils/birthYear";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import { trackPageView } from "../../utils/appInsightsHelper";
import AddCircleOutlineIcon from '@material-ui/icons/AddCircleOutline';
import { SubMenuTitle } from "../layout/SubMenuTitle";

const useStyles = makeStyles((theme) => ({
  addLocationContainer: {
    marginTop: 20,
    width: "fit-content",
    "&:hover": {
      cursor: "pointer",
    }
  }
}));

const DataCollectorsCreatePageComponent = (props) => {
  const currentUserRoles = useSelector((state) => state.appData.user.roles);
  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );
  const [birthDecades] = useState(getBirthDecades());
  const [type, setType] = useState(dataCollectorType.human);
  const [centerLocation, setCenterLocation] = useState(null);
  const [locations, setLocations] = useState([
    {
      latitude: "",
      longitude: "",
      regionId: "",
      districtId: "",
      villageId: "",
      zoneId: "",
      number: 0,
    },
  ]);

  const theme = useTheme()
  const classes = useStyles()

  useMount(() => {
    props.openCreation(props.projectId);

    // Track page view
    trackPageView("DataCollectorsCreatePage");
  });

  const form = useMemo(() => {
    if (!props.defaultLocation) {
      return null;
    }

    setCenterLocation({
      latitude: props.defaultLocation.latitude,
      longitude: props.defaultLocation.longitude,
    });

    const fields = {
      dataCollectorType: dataCollectorType.human,
      name: "",
      displayName: "",
      sex: "",
      supervisorId: props.defaultSupervisorId
        ? props.defaultSupervisorId.toString()
        : "",
      birthGroupDecade: "",
      phoneNumber: "",
      additionalPhoneNumber: "",
      deployed: true,
      linkedToHeadSupervisor: false,
    };

    const validation = {
      dataCollectorType: [validators.required],
      name: [validators.required, validators.maxLength(100)],
      displayName: [
        validators.requiredWhen(
          (x) => x.dataCollectorType === dataCollectorType.human,
        ),
        validators.maxLength(100),
      ],
      sex: [
        validators.requiredWhen(
          (x) => x.dataCollectorType === dataCollectorType.human,
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
    newForm.fields.dataCollectorType.subscribe(({ newValue }) =>
      setType(newValue),
    );
    newForm.fields.supervisorId.subscribe(
      ({ newValue }) =>
        !!newValue &&
        newForm.fields.linkedToHeadSupervisor.update(
          props.supervisors.filter((s) => s.id.toString() === newValue)[0]
            .role === HeadSupervisor,
        ),
    );
    return newForm;
  }, [props.defaultSupervisorId, props.defaultLocation, props.supervisors]);

  const addDataCollectorLocation = () => {
    const previousLocation = locations[locations.length - 1];

    setLocations([
      ...locations,
      {
        latitude: "",
        longitude: "",
        regionId: "",
        districtId: "",
        villageId: "",
        zoneId: "",
        number: previousLocation.number + 1,
      },
    ]);
  };

  const removeDataCollectorLocation = (location) => {
    setLocations(locations.filter((l) => l.number !== location.number));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!form.isValid()) {
      Object.values(form.fields)
        .filter((e) => e.error)[0]
        .scrollTo();
      return;
    }

    const values = form.getValues();

    props.create(
      getSaveFormModel(
        props.projectId,
        values,
        values.dataCollectorType,
        locations,
      ),
    );
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
      <SubMenuTitle />
      {props.error && !props.error.data && (
        <ValidationMessage message={props.error.message} />
      )}
      <Form onSubmit={handleSubmit} fullWidth>
        <Grid container>
          <Grid item xs={6} md={3}>
            <Typography variant="h5">
              {strings(stringKeys.dataCollectors.form.dataCollectorType)}
            </Typography>
            <RadioGroupField
              name="dataCollectorType"
              field={form.fields.dataCollectorType}
              horizontal
            >
              {Object.keys(dataCollectorType).map((type) => (
                <FormControlLabel
                  style={{ marginRight: 15 }}
                  key={type}
                  control={<Radio />}
                  label={strings(
                    stringKeys.dataCollectors.constants.dataCollectorType[
                    dataCollectorType[type]
                    ],
                  )}
                  value={dataCollectorType[type]}
                />
              ))}
            </RadioGroupField>
          </Grid>

          <Grid item xs={6} md={4}>
            <Typography variant="h5">
              {strings(stringKeys.dataCollectors.filters.deployedMode)}
            </Typography>
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

            {type === dataCollectorType.human && (
              <Grid item xs={12} md={3}>
                <TextInputField
                  label={strings(stringKeys.dataCollectors.form.displayName)}
                  name="displayName"
                  field={form.fields.displayName}
                  fieldRef={form.fields.displayName.ref}
                />
              </Grid>
            )}

            {type === dataCollectorType.human && (
              <Grid item xs={12} md={3}>
                <SelectField
                  label={strings(stringKeys.dataCollectors.form.sex)}
                  name="sex"
                  field={form.fields.sex}
                  fieldRef={form.fields.sex.ref}
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
          <Grid container item xs={12} spacing={4}>
            <Grid item xs={12} md={3}>
              <PhoneInputField
                label={strings(stringKeys.dataCollectors.form.phoneNumber)}
                name="phoneNumber"
                fieldRef={form.fields.phoneNumber.ref}
                field={form.fields.phoneNumber}
                defaultCountry={props.countryCode}
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
                defaultCountry={props.countryCode}
                rtl={useRtlDirection}
              />
            </Grid>

          </Grid>
          {!currentUserRoles.some((r) => r === Supervisor) && (
            <Grid container item xs={12} direction="column" style={{ marginTop: 30 }}>
              <Typography variant="h5">{strings(stringKeys.dataCollectors.form.responsibleSupervisor)}</Typography>
              <Grid item md={3}>
                <SelectField
                  label={strings(stringKeys.dataCollectors.form.supervisor)}
                  field={form.fields.supervisorId}
                  name="supervisorId"
                  fieldRef={form.fields.supervisorId.ref}
                >
                  {props.supervisors.map((supervisor) => (
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
        </Grid>
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
              regions={props.regions}
              isDefaultCollapsed={false}
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
            <Typography color="inherit" style={{ marginRight: 4 }}>
              {strings(stringKeys.common.buttons.update)}
            </Typography>
            <Typography color="inherit" style={{ textTransform: "lowercase" }}>
              {strings(stringKeys.dataCollectors.constants.dataCollectorType[type])}
            </Typography>
          </SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
};

DataCollectorsCreatePageComponent.propTypes = {};

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
  error: state.dataCollectors.formError,
  countryCode: state.dataCollectors.countryCode,
});

const mapDispatchToProps = {
  openCreation: dataCollectorsActions.openCreation.invoke,
  create: dataCollectorsActions.create.invoke,
  goToList: dataCollectorsActions.goToList,
};

export const DataCollectorsCreatePage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(DataCollectorsCreatePageComponent),
);
