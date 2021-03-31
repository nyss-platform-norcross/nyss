import styles from './DataCollectorLocationItem.module.scss';
import { Card, CardContent, Grid, IconButton, InputLabel, MenuItem, Typography } from '@material-ui/core';
import { useState, useReducer, useEffect } from 'react';
import { stringKeys, strings } from '../../strings';
import { useMount } from '../../utils/lifecycle';
import { TableActionsButton } from '../common/tableActions/TableActionsButton';
import TextInputField from '../forms/TextInputField';
import { DataCollectorMap } from './DataCollectorMap';
import { retrieveGpsLocation } from '../../utils/map';
import { validators } from '../../utils/forms';
import { ConditionalCollapse } from '../common/conditionalCollapse/ConditionalCollapse';
import ExpandMore from '@material-ui/icons/ExpandMore';
import * as http from '../../utils/http';
import SelectField from '../forms/SelectField';


export const DataCollectorLocationItem = ({ form, location, locationNumber, totalLocations, regions, initialDistricts, initialVillages, initialZones, defaultLocation }) => {
  const [ready, setReady] = useState(false);
  const [isFetchingLocation, setIsFetchingLocation] = useState(false);
  const [mapCenterLocation, setMapCenterLocation] = useState(null);
  const [isExpanded, setIsExpanded] = useState(false);
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

  useEffect(() => setIsExpanded(locationNumber === totalLocations - 1), [locationNumber, totalLocations]);

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

  const onRetrieveLocation = () => {
    setIsFetchingLocation(true);
    retrieveGpsLocation(location => {
      if (location === null) {
        setIsFetchingLocation(false);
        return;
      }
      const lat = location.coords.latitude;
      const lng = location.coords.longitude;
      form.fields[`location_${locationNumber}_latitude`].update(lat);
      form.fields[`location_${locationNumber}_longitude`].update(lng);
      dispatch({ type: 'latlng', lat: lat, lng: lng });
      setIsFetchingLocation(false);
    });
  }

  const onRegionChange = (event) => {
    const regionId = event.target.value;
    form.fields[`location_${locationNumber}_districtId`].update('', true);
    form.fields[`location_${locationNumber}_villageId`].update('', true);
    form.fields[`location_${locationNumber}_zoneId`].update('', true);

    setDistricts([]);
    setVillages([]);
    setZones([]);

    http.get(`/api/nationalSocietyStructure/district/list?regionId=${regionId}`)
      .then(response => setDistricts(response.value));
  }

  const onDistrictChange = (event) => {
    const districtId = event.target.value;
    form.fields[`location_${locationNumber}_villageId`].update('', true);
    form.fields[`location_${locationNumber}_zoneId`].update('', true);

    setVillages([]);
    setZones([]);

    http.get(`/api/nationalSocietyStructure/village/list?districtId=${districtId}`)
      .then(response => setVillages(response.value));
  }

  const onVillageChange = (event) => {
    const villageId = event.target.value;
    form.fields[`location_${locationNumber}_zoneId`].update('', true);

    setZones([]);

    http.get(`/api/nationalSocietyStructure/zone/list?villageId=${villageId}`)
      .then(response => setZones(response.value));

    setSelectedVillage(villageId);
  }

  const renderCollapsedLocationData = () => {
    const region = regions.find(r => r.id === parseInt(form.fields[`location_${locationNumber}_regionId`].value));
    const district = districts.find(d => d.id === parseInt(form.fields[`location_${locationNumber}_districtId`].value));
    const village = villages.find(v => v.id === parseInt(form.fields[`location_${locationNumber}_villageId`].value));
    const zone = zones.find(z => z.id === parseInt(form.fields[`location_${locationNumber}_zoneId`].value));

    return `${!!region ? region.name : ''} ${!!district ? `> ${district.name}` : ''} ${!!village ? `> ${village.name}` : ''} ${!!zone ? `> ${zone.name}` : ''}`;
  }

  if (!ready) {
    return null;
  }

  return (
    <Grid container spacing={2}>
      <Grid item xs={12}>
        <Card className={styles.requiredMapLocation} data-missing-location={form.fields[`location_${locationNumber}_latitude`].error !== null || form.fields[`location_${locationNumber}_longitude`].error !== null}>
          <CardContent>
            <Grid item xs={12} className={styles.locationHeader}>
              <Typography variant='h6'>{`${strings(stringKeys.dataCollector.form.location)} ${locationNumber + 1}`}</Typography>

              <Grid item className={styles.expandFilterButton}>
                <IconButton data-expanded={isExpanded} onClick={() => setIsExpanded(!isExpanded)}>
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

                <Grid item xs={12} className={styles.geoStructureShrinked}>
                  <Grid item xs={12}>
                    <SelectField
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
                </Grid>

                <Grid item xs={12}>
                  <InputLabel className={styles.mapLabel}>{strings(stringKeys.dataCollector.form.selectLocation)}</InputLabel>
                  <DataCollectorMap
                    onChange={onLocationChange}
                    location={mapLocation}
                    centerLocation={mapCenterLocation}
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
                  <Grid item xs={12} md={3} className={styles.locationButton}>
                    <TableActionsButton
                      onClick={onRetrieveLocation}
                      isFetching={isFetchingLocation}
                    >
                      {strings(stringKeys.dataCollector.form.retrieveLocation)}
                    </TableActionsButton>
                  </Grid>
                </Grid>
              </Grid>
            </ConditionalCollapse>
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );
}