import React, { useState, Fragment } from 'react';
import SelectInput from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import * as http from '../../utils/http';

export const GeoStructureSelect = ({ regions, initialDistricts, initialVillages, initialZones, regionIdField, districtIdField, villageIdField, zoneIdField }) => {
  const [districts, setDistricts] = useState(initialDistricts || []);
  const [villages, setVillages] = useState(initialVillages || []);
  const [zones, setZones] = useState(initialZones || []);

  const onRegionChange = (event) => {
    const regionId = event.target.value;
    districtIdField.update("", true);
    villageIdField.update("", true);
    zoneIdField.update("", true);

    setDistricts([]);
    setVillages([]);
    setZones([]);

    http.get(`/api/region/${regionId}/district/list`)
      .then(response => setDistricts(response.value))
  }

  const onDistrictChange = (event) => {
    const districtId = event.target.value;
    villageIdField.update("", true);
    zoneIdField.update("", true);

    setVillages([]);
    setZones([]);

    http.get(`/api/district/${districtId}/village/list`)
      .then(response => setVillages(response.value))
  }

  const onVillageChange = (event) => {
    const villageId = event.target.value;
    zoneIdField.update("", true);

    setZones([]);

    http.get(`/api/village/${villageId}/zone/list`)
      .then(response => setZones(response.value))
  }

  return (
    <Fragment>
      <Grid item xs={12}>
        <SelectInput
          label={strings(stringKeys.dataCollector.form.region)}
          field={regionIdField}
          name="regionId"
          onChange={onRegionChange}
        >
          {regions.map(region => (
            <MenuItem key={`region_${region.id}`} value={region.id.toString()}>
              {region.name}
            </MenuItem>
          ))}
        </SelectInput>
      </Grid>

      <Grid item xs={12}>
        <SelectInput
          label={strings(stringKeys.dataCollector.form.district)}
          field={districtIdField}
          name="districtId"
          onChange={onDistrictChange}
        >
          {districts.map(district => (
            <MenuItem key={`district_${district.id}`} value={district.id.toString()}>
              {district.name}
            </MenuItem>
          ))}
        </SelectInput>
      </Grid>

      <Grid item xs={12}>
        <SelectInput
          label={strings(stringKeys.dataCollector.form.village)}
          field={villageIdField}
          name="villageId"
          onChange={onVillageChange}
        >
          {villages.map(village => (
            <MenuItem key={`village_${village.id}`} value={village.id.toString()}>
              {village.name}
            </MenuItem>
          ))}
        </SelectInput>
      </Grid>

      <Grid item xs={12}>
        <SelectInput
          label={strings(stringKeys.dataCollector.form.zone)}
          field={zoneIdField}
          name="zoneId"
        >
          {zones.map(zone => (
            <MenuItem key={`zone_${zone.id}`} value={zone.id.toString()}>
              {zone.name}
            </MenuItem>
          ))}
        </SelectInput>
      </Grid>
    </Fragment>
  );
}
