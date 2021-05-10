import styles from './DataCollectorLocationItem.module.scss';
import { Button, Card, CardContent, Grid, IconButton, InputLabel, MenuItem, Typography } from '@material-ui/core';
import { useState, useReducer, useEffect } from 'react';
import { stringKeys, strings } from '../../strings';
import { useMount } from '../../utils/lifecycle';
import TextInputField from '../forms/TextInputField';
import { DataCollectorMap } from './DataCollectorMap';
import { validators } from '../../utils/forms';
import { ConditionalCollapse } from '../common/conditionalCollapse/ConditionalCollapse';
import ExpandMore from '@material-ui/icons/ExpandMore';
import SelectField from '../forms/SelectField';
import { getFormDistricts, getFormVillages, getFormZones } from './logic/dataCollectorsService';


export const DataCollectorLocationItem = ({ form, location, locationNumber, isLastLocation, isOnlyLocation, regions, initialDistricts, initialVillages, initialZones, defaultLocation, isDefaultCollapsed, removeLocation }) => {
  const [ready, setReady] = useState(false);
  const [mapCenterLocation, setMapCenterLocation] = useState(null);
  const [isExpanded, setIsExpanded] = useState(false);
  const [defaultCollapsed, setDefaultCollapsed] = useState(isDefaultCollapsed);
  const [districts, setDistricts] = useState(initialDistricts || []);
  const [villages, setVillages] = useState(initialVillages || []);
  const [zones, setZones] = useState(initialZones || []);

  const [selectedVillage, setSelectedVillage] = useReducer((state, action) => {
    if (action.initialValue) {
      return { value: action.initialValue, changed: false }
    }
    if (state.value !== action) {
      return { ...state, changed: true, value: action }
    } else {
      return { ...state, changed: false }
    }
  }, { value: '', changed: false });

  const [mapLocation, dispatch] = useReducer((state, action) => {
    switch (action.type) {
      case 'latitude': return { ...state, lat: action.value };
      case 'longitude': return { ...state, lng: action.value };
      case 'latlng': return { lat: action.lat, lng: action.lng };
      default: return state;
    }
  }, null);

  useMount(() => {
    dispatch({ type: 'latlng', lat: location.latitude, lng: location.longitude });
    setMapCenterLocation(!!defaultLocation ? {
      lat: defaultLocation.latitude,
      lng: defaultLocation.longitude
    } : {
      lat: location.latitude,
      lng: location.longitude
    });

    if (!!location.districtId && !location.villageId) {
      getFormVillages(location.districtId, setVillages);
    }

    form.addField(`location_${locationNumber}_latitude`, location.latitude, [validators.required, validators.integer, validators.inRange(-90, 90)]);
    form.addField(`location_${locationNumber}_longitude`, location.longitude, [validators.required, validators.integer, validators.inRange(-180, 180)]);
    form.addField(`location_${locationNumber}_regionId`, location.regionId.toString(), [validators.required]);
    form.addField(`location_${locationNumber}_districtId`, location.districtId.toString(), [validators.required]);
    form.addField(`location_${locationNumber}_villageId`, location.villageId.toString(), [validators.required]);
    form.addField(`location_${locationNumber}_zoneId`, !!location.zoneId ? location.zoneId.toString() : '');


    form.fields[`location_${locationNumber}_latitude`].subscribe(({ newValue }) => dispatch({ type: 'latitude', value: newValue }));
    form.fields[`location_${locationNumber}_longitude`].subscribe(({ newValue }) => dispatch({ type: 'longitude', value: newValue }));

    setReady(true);

    return () => {
      form.removeField(`location_${locationNumber}_latitude`);
      form.removeField(`location_${locationNumber}_longitude`);
      form.removeField(`location_${locationNumber}_regionId`);
      form.removeField(`location_${locationNumber}_districtId`);
      form.removeField(`location_${locationNumber}_villageId`);
      form.removeField(`location_${locationNumber}_zoneId`);
    }
  });

  useEffect(() => setIsExpanded(!isDefaultCollapsed && isLastLocation),
    [isLastLocation, isDefaultCollapsed]);

  useEffect(() => {
    if (form && form.fields && selectedVillage.changed) {
      form.fields[`location_${locationNumber}_latitude`].update('');
      form.fields[`location_${locationNumber}_longitude`].update('');
    }
  }, [form, selectedVillage, locationNumber]);

  const onLocationChange = (e) => {
    form.fields[`location_${locationNumber}_latitude`].update(e.lat);
    form.fields[`location_${locationNumber}_longitude`].update(e.lng);
  }

  const onRegionChange = (event) => {
    const regionId = event.target.value;
    form.fields[`location_${locationNumber}_districtId`].update('', true);
    form.fields[`location_${locationNumber}_villageId`].update('', true);
    form.fields[`location_${locationNumber}_zoneId`].update('', true);

    setDistricts([]);
    setVillages([]);
    setZones([]);

    getFormDistricts(regionId, setDistricts);
  }

  const onDistrictChange = (event) => {
    const districtId = event.target.value;
    form.fields[`location_${locationNumber}_villageId`].update('', true);
    form.fields[`location_${locationNumber}_zoneId`].update('', true);

    setVillages([]);
    setZones([]);

    getFormVillages(districtId, setVillages);
  }

  const onVillageChange = (event) => {
    const villageId = event.target.value;
    form.fields[`location_${locationNumber}_zoneId`].update('', true);

    setZones([]);

    getFormZones(villageId, setZones);

    setSelectedVillage(villageId);
  }

  const renderCollapsedLocationData = () => {
    const region = regions.find(r => r.id === parseInt(form.fields[`location_${locationNumber}_regionId`].value));
    const district = districts.find(d => d.id === parseInt(form.fields[`location_${locationNumber}_districtId`].value));
    const village = villages.find(v => v.id === parseInt(form.fields[`location_${locationNumber}_villageId`].value));
    const zone = zones.find(z => z.id === parseInt(form.fields[`location_${locationNumber}_zoneId`].value));

    return `${!!region ? region.name : ''} ${!!district ? `> ${district.name}` : ''} ${!!village ? `> ${village.name}` : ''} ${!!zone ? `> ${zone.name}` : ''}`;
  }

  const onToggleExpand = () => {
    if (defaultCollapsed) {
      setDefaultCollapsed(false);
    }

    setIsExpanded(!isExpanded);
  }

  const onRemoveLocation = () => {
    removeLocation(location);
  }

  if (!ready) {
    return null;
  }

  return (
    <Grid item xs={12}>
      <Card className={styles.requiredMapLocation} data-missing-location={form.fields[`location_${locationNumber}_latitude`].error !== null || form.fields[`location_${locationNumber}_longitude`].error !== null}>
        <CardContent className={!isExpanded ? styles.collapsibleContent : ''}>
          <Grid item xs={12} className={styles.locationHeader}>
            <Typography variant='h6'>{strings(stringKeys.dataCollector.form.location)}</Typography>

            <Grid item className={styles.expandFilterButton}>
              <IconButton data-expanded={isExpanded} onClick={onToggleExpand}>
                <ExpandMore />
              </IconButton>
            </Grid>

          </Grid>
          {!isExpanded && (
            <Grid item xs={12}>
              <Typography variant='subtitle2'>{renderCollapsedLocationData()}</Typography>
            </Grid>
          )}

          <ConditionalCollapse collapsible expanded={isExpanded} className={styles.collapsibleContainer}>
            <Grid container spacing={2}>

              <Grid item xs={12}>
                <SelectField
                  className={styles.geoStructureSelectShrinked}
                  label={strings(stringKeys.dataCollector.form.region)}
                  field={form.fields[`location_${locationNumber}_regionId`]}
                  name='regionId'
                  onChange={onRegionChange}
                >
                  {regions.map(region => (
                    <MenuItem key={`region_${region.id}`} value={region.id.toString()}>
                      {region.name}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>

              <Grid item xs={12}>
                <SelectField
                  className={styles.geoStructureSelectShrinked}
                  label={strings(stringKeys.dataCollector.form.district)}
                  field={form.fields[`location_${locationNumber}_districtId`]}
                  name='districtId'
                  onChange={onDistrictChange}
                >
                  {districts.map(district => (
                    <MenuItem key={`district_${district.id}`} value={district.id.toString()}>
                      {district.name}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>

              <Grid item xs={12}>
                <SelectField
                  className={styles.geoStructureSelectShrinked}
                  label={strings(stringKeys.dataCollector.form.village)}
                  field={form.fields[`location_${locationNumber}_villageId`]}
                  name='villageId'
                  onChange={onVillageChange}
                >
                  {villages.map(village => (
                    <MenuItem key={`village_${village.id}`} value={village.id.toString()}>
                      {village.name}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>

              <Grid item xs={12}>
                <SelectField
                  className={styles.geoStructureSelectShrinked}
                  label={strings(stringKeys.dataCollector.form.zone)}
                  field={form.fields[`location_${locationNumber}_zoneId`]}
                  name='zoneId'
                >
                  <MenuItem value=''>
                    &nbsp;
                      </MenuItem>

                  {zones.map(zone => (
                    <MenuItem key={`zone_${zone.id}`} value={zone.id.toString()}>
                      {zone.name}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>

              <Grid item xs={12}>
                <InputLabel className={styles.mapLabel}>{strings(stringKeys.dataCollector.form.selectLocation)}</InputLabel>
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
                    label={strings(stringKeys.dataCollector.form.latitude)}
                    name='latitude'
                    field={form.fields[`location_${locationNumber}_latitude`]}
                    type='number'
                    inputMode={'decimal'}
                  />
                </Grid>
                <Grid item xs={12} md={3} className={styles.latLngInput}>
                  <TextInputField
                    label={strings(stringKeys.dataCollector.form.longitude)}
                    name='longitude'
                    field={form.fields[`location_${locationNumber}_longitude`]}
                    type='number'
                    inputMode={'decimal'}
                  />
                </Grid>

                {!isOnlyLocation && (
                  <Button className={styles.removeLocationButton} onClick={onRemoveLocation}>{strings(stringKeys.dataCollector.form.removeLocation)}</Button>
                )}
              </Grid>
            </Grid>
          </ConditionalCollapse>
        </CardContent>
      </Card>
    </Grid>
  );
}