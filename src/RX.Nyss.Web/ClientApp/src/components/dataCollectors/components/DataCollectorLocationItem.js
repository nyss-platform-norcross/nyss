import styles from "./DataCollectorLocationItem.module.scss";
import {
  Button,
  Card,
  CardContent,
  Grid,
  IconButton,
  InputLabel,
  MenuItem,
  Typography,
} from "@material-ui/core";
import { Alert } from "@material-ui/lab";
import { makeStyles } from "@material-ui/core/styles";
import { useState, useReducer, useEffect, useRef, useCallback } from "react";
import { stringKeys, strings } from "../../../strings";
import { useMount } from "../../../utils/lifecycle";
import TextInputField from "../../forms/TextInputField";
import { DataCollectorMap } from "./DataCollectorMap";
import { validators } from "../../../utils/forms";
import { ConditionalCollapse } from "../../common/conditionalCollapse/ConditionalCollapse";
import ExpandMore from "@material-ui/icons/ExpandMore";
import SelectField from "../../forms/SelectField";
import {
  getFormDistricts,
  getFormVillages,
  getFormZones,
} from "../logic/dataCollectorsService";
import { set } from "../../../utils/cache";

const useStyles = makeStyles({
  alert: {
    width: "26rem",
  },
});

export const DataCollectorLocationItem = ({
  form,
  location,
  locationNumber,
  isLastLocation,
  isOnlyLocation,
  regions,
  initialDistricts,
  initialVillages,
  initialZones,
  defaultLocation,
  isDefaultCollapsed,
  removeLocation,
  allLocations,
  rtl,
}) => {
  const classes = useStyles();

  const [ready, setReady] = useState(false);
  const [mapCenterLocation, setMapCenterLocation] = useState(null);
  const [isExpanded, setIsExpanded] = useState(false);
  const [defaultCollapsed, setDefaultCollapsed] = useState(isDefaultCollapsed);
  const [districts, setDistricts] = useState(initialDistricts || null);
  const [villages, setVillages] = useState(initialVillages || null);
  const [zones, setZones] = useState(initialZones || null);

  const locationCardRef = useRef(null);
  const getLocationCardRef = useCallback((node) => locationCardRef, []);

  const [mapLocation, dispatch] = useReducer((state, action) => {
    switch (action.type) {
      case "latitude":
        return { ...state, lat: action.value };
      case "longitude":
        return { ...state, lng: action.value };
      case "latlng":
        return { lat: action.lat, lng: action.lng };
      default:
        return state;
    }
  }, null);

  useMount(() => {
    dispatch({
      type: "latlng",
      lat: location.latitude,
      lng: location.longitude,
    });
    setMapCenterLocation(
      !!defaultLocation
        ? {
            lat: defaultLocation.latitude,
            lng: defaultLocation.longitude,
          }
        : {
            lat: location.latitude,
            lng: location.longitude,
          },
    );

    if (!!location.districtId && !location.villageId) {
      getFormDistricts(location.regionId, setDistricts);
      getFormVillages(location.districtId, setVillages);
    }

    form.addField(
      `locations_${locationNumber}_latitude`,
      location.latitude,
      [validators.required, validators.integer, validators.inRange(-90, 90)],
      locationCardRef,
    );
    form.addField(
      `locations_${locationNumber}_longitude`,
      location.longitude,
      [validators.required, validators.integer, validators.inRange(-180, 180)],
      locationCardRef,
    );
    form.addField(
      `locations_${locationNumber}_regionId`,
      location.regionId.toString(),
      [validators.required],
      locationCardRef,
    );
    form.addField(
      `locations_${locationNumber}_districtId`,
      location.districtId.toString(),
      [validators.required],
      locationCardRef,
    );
    form.addField(
      `locations_${locationNumber}_villageId`,
      location.villageId.toString(),
      [
        validators.required,
        validators.uniqueLocation(
          (x) => x[`locations_${locationNumber}_zoneId`],
          allLocations,
        ),
      ],
      locationCardRef,
    );
    form.addField(
      `locations_${locationNumber}_zoneId`,
      !!location.zoneId ? location.zoneId.toString() : "",
      [],
      locationCardRef,
    );

    form.fields[`locations_${locationNumber}_latitude`].subscribe(
      ({ newValue }) => dispatch({ type: "latitude", value: newValue }),
    );
    form.fields[`locations_${locationNumber}_longitude`].subscribe(
      ({ newValue }) => dispatch({ type: "longitude", value: newValue }),
    );
    form.fields[`locations_${locationNumber}_zoneId`].subscribe(
      ({ newValue }) => (location.zoneId = newValue),
    );

    setReady(true);

    return () => {
      form.removeField(`locations_${locationNumber}_latitude`);
      form.removeField(`locations_${locationNumber}_longitude`);
      form.removeField(`locations_${locationNumber}_regionId`);
      form.removeField(`locations_${locationNumber}_districtId`);
      form.removeField(`locations_${locationNumber}_villageId`);
      form.removeField(`locations_${locationNumber}_zoneId`);
    };
  });

  useEffect(() => {
    allLocations.forEach((l) => {
      const field = form.fields[`locations_${l.number}_villageId`];
      if (!!field) {
        field.setValidators([
          validators.required,
          validators.uniqueLocation(
            (x) => x[`locations_${l.number}_zoneId`],
            allLocations,
          ),
        ]);
      }
    });
  }, [allLocations]);

  useEffect(
    () => setIsExpanded(!isDefaultCollapsed && isLastLocation),
    [isLastLocation, isDefaultCollapsed],
  );

  const onLocationChange = (e) => {
    form.fields[`locations_${locationNumber}_latitude`].update(e.lat);
    form.fields[`locations_${locationNumber}_longitude`].update(e.lng);
  };

  const onRegionChange = (event) => {
    const regionId = event.target.value;
    form.fields[`locations_${locationNumber}_districtId`].update("", true);
    form.fields[`locations_${locationNumber}_villageId`].update("", true);
    form.fields[`locations_${locationNumber}_zoneId`].update("", true);

    setVillages(null);
    setZones(null);

    getFormDistricts(regionId, setDistricts);
  };

  const onDistrictChange = (event) => {
    const districtId = event.target.value;
    form.fields[`locations_${locationNumber}_villageId`].update("", true);
    form.fields[`locations_${locationNumber}_zoneId`].update("", true);

    setZones(null);

    getFormVillages(districtId, setVillages);
  };

  const onVillageChange = (event) => {
    const villageId = event.target.value;
    form.fields[`locations_${locationNumber}_zoneId`].update("", true);
    location.villageId = villageId;

    getFormZones(villageId, setZones);
  };

  const renderCollapsedLocationData = () => {
    const region = regions?.find(
      (r) =>
        r.id ===
        parseInt(form.fields[`locations_${locationNumber}_regionId`].value),
    );
    const district = districts?.find(
      (d) =>
        d.id ===
        parseInt(form.fields[`locations_${locationNumber}_districtId`].value),
    );
    const village = villages?.find(
      (v) =>
        v.id ===
        parseInt(form.fields[`locations_${locationNumber}_villageId`].value),
    );
    const zone = zones?.find(
      (z) =>
        z.id ===
        parseInt(form.fields[`locations_${locationNumber}_zoneId`].value),
    );

    return `${!!region ? region.name : ""} ${
      !!district ? `> ${district.name}` : ""
    } ${!!village ? `> ${village.name}` : ""} ${
      !!zone ? `> ${zone.name}` : ""
    }`;
  };

  const onToggleExpand = () => {
    if (defaultCollapsed) {
      setDefaultCollapsed(false);
    }

    setIsExpanded(!isExpanded);
  };

  const onRemoveLocation = () => {
    removeLocation(location);
  };

  const hasError = () =>
    !!form.fields[`locations_${locationNumber}_latitude`].error ||
    !!form.fields[`locations_${locationNumber}_longitude`].error ||
    !!form.fields[`locations_${locationNumber}_regionId`].error ||
    !!form.fields[`locations_${locationNumber}_districtId`].error ||
    !!form.fields[`locations_${locationNumber}_villageId`].error ||
    !!form.fields[`locations_${locationNumber}_zoneId`].error;

  if (!ready) {
    return null;
  }

  return (
    <Grid item xs={12} ref={locationCardRef}>
      <Card className={styles.requiredMapLocation} data-with-error={hasError()}>
        <CardContent className={!isExpanded ? styles.collapsibleContent : ""}>
          <Grid item xs={12} className={styles.locationHeader}>
            <Typography variant="h5">
              {strings(stringKeys.common.location)}
            </Typography>

            <Grid
              item
              className={`${styles.expandFilterButton} ${
                rtl ? styles.rtl : ""
              }`}
            >
              <IconButton data-expanded={isExpanded} onClick={onToggleExpand}>
                <ExpandMore />
              </IconButton>
            </Grid>
          </Grid>
          {!isExpanded && (
            <Grid item xs={12}>
              <Typography variant="subtitle2">
                {renderCollapsedLocationData()}
              </Typography>
            </Grid>
          )}

          <ConditionalCollapse
            collapsible
            expanded={isExpanded}
            className={styles.collapsibleContainer}
          >
            <Grid container spacing={2}>
              <Grid container item xs={12} spacing={3}>
                <Grid item xs={12} md={3}>
                  <SelectField
                    className={styles.geoStructureSelectShrinked}
                    label={strings(stringKeys.dataCollectors.form.region)}
                    field={form.fields[`locations_${locationNumber}_regionId`]}
                    name="regionId"
                    onChange={onRegionChange}
                    fieldRef={getLocationCardRef}
                  >
                    {regions.map((region) => (
                      <MenuItem
                        key={`region_${region.id}`}
                        value={region.id.toString()}
                      >
                        {region.name}
                      </MenuItem>
                    ))}
                  </SelectField>
                </Grid>

                {form.fields[`locations_${locationNumber}_regionId`] &&
                  form.fields[`locations_${locationNumber}_regionId`].value && (
                    <Grid item xs={12} md={3}>
                      {districts && districts.length > 0 ? (
                        <SelectField
                          className={styles.geoStructureSelectShrinked}
                          label={strings(stringKeys.dataCollectors.form.district)}
                          field={
                            form.fields[`locations_${locationNumber}_districtId`]
                          }
                          name="districtId"
                          onChange={onDistrictChange}
                          fieldRef={getLocationCardRef}
                        >
                          {districts.map((district) => (
                            <MenuItem
                              key={`district_${district.id}`}
                              value={district.id.toString()}
                            >
                              {district.name}
                            </MenuItem>
                          ))}
                        </SelectField>
                      ) : (
                        <Alert
                          variant="filled"
                          severity="warning"
                          className={classes.alert}
                        >
                          {strings(
                            stringKeys.dataCollectors.form.alerts
                              .noDistrictsAlert,
                          )}
                        </Alert>
                      )}
                    </Grid>
                )}

                {form.fields[`locations_${locationNumber}_districtId`] &&
                  form.fields[`locations_${locationNumber}_districtId`].value && (
                    <Grid item xs={12} md={3}>
                      {villages && villages.length > 0 ? (
                        <SelectField
                          className={styles.geoStructureSelectShrinked}
                          label={strings(stringKeys.dataCollectors.form.village)}
                          field={
                            form.fields[`locations_${locationNumber}_villageId`]
                          }
                          name="villageId"
                          onChange={onVillageChange}
                          fieldRef={getLocationCardRef}
                        >
                          {villages.map((village) => (
                            <MenuItem
                              key={`village_${village.id}`}
                              value={village.id.toString()}
                            >
                              {village.name}
                            </MenuItem>
                          ))}
                        </SelectField>
                      ) : (
                        <Alert
                          variant="filled"
                          severity="warning"
                          className={classes.alert}
                        >
                          {strings(
                            stringKeys.dataCollectors.form.alerts.noVillagesAlert,
                          )}
                        </Alert>
                      )}
                    </Grid>
                  )}

                  {zones && zones.length > 0 && (
                    <Grid item xs={12} md={3}>
                      <SelectField
                        className={styles.geoStructureSelectShrinked}
                        label={strings(stringKeys.dataCollectors.form.zone)}
                        field={form.fields[`locations_${locationNumber}_zoneId`]}
                        name="zoneId"
                        fieldRef={getLocationCardRef}
                      >
                        <MenuItem value="">&nbsp;</MenuItem>

                        {zones.map((zone) => (
                          <MenuItem
                            key={`zone_${zone.id}`}
                            value={zone.id.toString()}
                          >
                            {zone.name}
                          </MenuItem>
                        ))}
                      </SelectField>
                    </Grid>
                  )}
              </Grid>
              <Grid container item xs={12}>
                <Grid item xs={12}>
                  <InputLabel className={styles.mapLabel}>
                    {strings(stringKeys.dataCollectors.form.selectLocation)}
                  </InputLabel>
                  <DataCollectorMap
                    onChange={onLocationChange}
                    location={mapLocation}
                    initialCenterLocation={mapCenterLocation}
                    zoom={6}
                  />
                </Grid>
                <Grid item xs={12} className={styles.coordinateFieldsContainer}>
                  <Grid item xs={12} md={3} className={styles.latLngInput}>
                    <TextInputField
                      label={strings(stringKeys.dataCollectors.form.latitude)}
                      name="latitude"
                      field={form.fields[`locations_${locationNumber}_latitude`]}
                      type="number"
                      inputMode={"decimal"}
                      fieldRef={getLocationCardRef}
                    />
                  </Grid>
                  <Grid item xs={12} md={3} className={styles.latLngInput}>
                    <TextInputField
                      label={strings(stringKeys.dataCollectors.form.longitude)}
                      name="longitude"
                      field={form.fields[`locations_${locationNumber}_longitude`]}
                      type="number"
                      inputMode={"decimal"}
                      fieldRef={getLocationCardRef}
                    />
                  </Grid>
                  {!isOnlyLocation && (
                    <Button
                      color="primary"
                      className={styles.removeLocationButton}
                      onClick={onRemoveLocation}
                    >
                      {strings(stringKeys.dataCollectors.form.removeLocation)}
                    </Button>
                  )}
                </Grid>
              </Grid>
            </Grid>
          </ConditionalCollapse>
        </CardContent>
      </Card>
    </Grid>
  );
};
