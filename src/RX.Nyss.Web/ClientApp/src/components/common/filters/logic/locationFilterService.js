export const extractSelectedValues = (regions) => {
  const selectedRegions = regions
    .filter(r => r.selected)
    .map(r => r.id);

  const selectedDistricts = regions
    .filter(r => !r.selected)
    .map(r => r.districts.filter(d => d.selected)
      .map(d => d.id))
    .flat();

  const selectedVillages = regions
    .filter(r => !r.selected)
    .map(r => r.districts.filter(d => !d.selected)
      .map(d => d.villages.filter(v => v.selected)
        .map(v => v.id)))
    .flat(2);

  const selectedZones = regions
    .filter(r => !r.selected)
    .map(r => r.districts.filter(d => !d.selected)
      .map(d => d.villages.filter(v => !v.selected)
        .map(v => v.zones.filter(z => z.selected)
          .map(z => z.id))))
    .flat(3);

  return {
    regionIds: selectedRegions,
    districtIds: selectedDistricts,
    villageIds: selectedVillages,
    zoneIds: selectedZones
  };
}

export const mapToSelectedLocations = (regions) =>
  regions.map(r => ({
    ...r,
    selected: true,
    districts: !!r.districts ? r.districts.map(d => ({
      ...d,
      selected: true,
      villages: !!d.villages ? d.villages.map(v => ({
        ...v,
        selected: true,
        zones: !!v.zones ? v.zones.map(z => ({
          ...z,
          selected: true
        })) : []
      })) : []
    })) : []
  }));

export const toggleSelectedStatus = (regions, selected) =>
  regions.map(r => ({
    ...r,
    selected: selected,
    districts: !!r.districts ? r.districts.map(d => ({
      ...d,
      selected: selected,
      villages: !!d.villages ? d.villages.map(v => ({
        ...v,
        selected: selected,
        zones: !!v.zones ? v.zones.map(z => ({
          ...z,
          selected: selected
        })) : []
      })) : []
    })) : []
  }));

export const cascadeSelectRegion = (region, selected) => {
  return {
    ...region,
    selected: selected,
    districts: !!region.districts ? region.districts.map(d => ({
      ...d,
      selected: selected,
      villages: !!d.villages ? d.villages.map(v => ({
        ...v,
        selected: selected,
        zones: !!v.zones ? v.zones.map(z => ({
          ...z,
          selected: selected
        })) : []
      })) : []
    })) : []
  };
}

export const cascadeSelectDistrict = (region, districtId, districtSelected) => {
  const districts = region.districts.map(d => setDistrict(d, districtId, districtSelected));
  const hasUnselectedDistricts = districts.some(d => !d.selected);

  return {
    ...region,
    selected: !hasUnselectedDistricts,
    districts: districts
  };
}
export const cascadeSelectVillage = (region, districtId, villageId, villageSelected) => {
  const villages = region.districts.filter(d => d.id === districtId).map(d => d.villages.map(v => setVillage(v, villageId, villageSelected))).flat();
  const anyUnslectedVillages = villages.some(v => !v.selected);
  const districts = region.districts.map(d => ({ ...d, selected: d.id === districtId ? !anyUnslectedVillages : d.selected, villages: d.id === districtId ? villages : d.villages }));
  const anyUnselectedDistricts = districts.some(d => !d.selected);

  return {
    ...region,
    selected: !anyUnselectedDistricts,
    districts: districts
  };
}


export const cascadeSelectZone = (region, districtId, villageId, zoneId, zoneSelected) => {
  const zones = region.districts.filter(d => d.id === districtId)
  .map(d => d.villages.filter(v => v.id === villageId).map(v => v.zones.map(z => setZone(z, zoneId, zoneSelected)))).flat(2);
  
  const anyUnselectedZones = zones.some(z => !z.selected);
  const villages = region.districts.filter(d => d.id === districtId)
  .map(d => d.villages.map(v => ({ ...v, selected: v.id === villageId ? !anyUnselectedZones : v.selected, zones: v.id === villageId ? zones : v.zones })))
  .flat();
  const anyUnselectedVillages = villages.some(v => !v.selected);
  const districts = region.districts.map(d => ({ ...d, selected: d.id === districtId ? !anyUnselectedVillages : d.selected, villages: d.id === districtId ? villages : d.villages }));
  const anyUnselectedDistricts = districts.some(d => !d.selected);
  
  return {
    ...region,
    selected: !anyUnselectedDistricts,
    districts: [
      ...districts
    ]
  }
}

const setDistrict = (district, districtToUpdateId, selected) => {
  if (district.id !== districtToUpdateId) {
    return district;
  }

  return {
    ...district,
    selected: selected,
    villages: !!district.villages ? district.villages.map(v => ({
      ...v,
      selected: selected,
      zones: !!v.zones ? v.zones.map(z => ({
        ...z,
        selected: selected
      })) : []
    })) : []
  }
}

const setVillage = (village, villageToUpdateId, selected) => {
  if (village.id !== villageToUpdateId) {
    return village;
  }

  return {
    ...village,
    selected: selected,
    zones: !!village.zones ? village.zones.map(z => ({
      ...z,
      selected: selected
    })) : []
  };
}


const setZone = (zone, zoneToUpdateId, selected) => {
  if (zone.id !== zoneToUpdateId) {
    return zone;
  }

  return {
    ...zone,
    selected: selected
  }
}
