import { stringKeys, strings } from "../../../../strings";

/* 
  extracts location ids used in dto
  if multiple locations on different levels in the same hierarchy are selected, only the highest is returned
*/
export const extractSelectedValues = (regions, includeUnknownLocation) => {
  const selectedRegions = regions
    .filter((r) => r.selected || r.districts.some((d) => d.selected))
    .map((r) => r.id);

  const selectedDistricts = regions
    .filter((r) => r.selected)
    .map((r) =>
      r.districts
        .filter((d) => d.selected || d.villages.some((v) => v.selected))
        .map((d) => d.id)
    )
    .flat();

  const selectedVillages = regions
    .filter((r) => r.selected)
    .map((r) =>
      r.districts
        .filter((d) => d.selected)
        .map((d) =>
          d.villages
            .filter((v) => v.selected || v.zones.some((z) => z.selected))
            .map((v) => v.id)
        )
    )
    .flat(2);

  const selectedZones = regions
    .filter((r) => r.selected)
    .map((r) =>
      r.districts
        .filter((d) => d.selected)
        .map((d) =>
          d.villages
            .filter((v) => v.selected)
            .map((v) => v.zones.filter((z) => z.selected).map((z) => z.id))
        )
    )
    .flat(3);

  return {
    regionIds: selectedRegions,
    districtIds: selectedDistricts,
    villageIds: selectedVillages,
    zoneIds: selectedZones,
    includeUnknownLocation: includeUnknownLocation,
  };
};

export const mapToSelectedLocations = (filterValue, regions) =>
  regions.map((region) => {
    const districts = mapSelectedDistricts(region.districts, filterValue);

    return {
      ...region,
      districts: districts,
      selected:
        !filterValue ||
        filterValue.regionIds.some((rId) => rId === region.id) ||
        districts.some((district) => district.selected),
    };
  });

const mapSelectedDistricts = (districts, filterValue) =>
  districts.map((district) => {
    const villages = mapSelectedVillages(district.villages, filterValue);

    return {
      ...district,
      villages: villages,
      selected:
        !filterValue ||
        filterValue.districtIds.some((dId) => dId === district.id) ||
        villages.some((village) => village.selected),
    };
  });

const mapSelectedVillages = (villages, filterValue) =>
  villages.map((village) => {
    const zones = mapSelectedZones(village.zones, filterValue);

    return {
      ...village,
      zones: zones,
      selected:
        !filterValue ||
        filterValue.villageIds.some((vId) => vId === village.id) ||
        zones.some((zone) => zone.selected),
    };
  });

const mapSelectedZones = (zones, filterValue) =>
  zones.map((zone) => {
    return {
      ...zone,
      selected:
        !filterValue || filterValue.zoneIds.some((zId) => zId === zone.id),
    };
  });

/*
  selects/deselects all locations
*/
export const toggleSelectedStatus = (regions, selected) =>
  regions.map((r) => ({
    ...r,
    selected: selected,
    districts: !!r.districts
      ? r.districts.map((d) => ({
          ...d,
          selected: selected,
          villages: !!d.villages
            ? d.villages.map((v) => ({
                ...v,
                selected: selected,
                zones: !!v.zones
                  ? v.zones.map((z) => ({
                      ...z,
                      selected: selected,
                    }))
                  : [],
              }))
            : [],
        }))
      : [],
  }));

/*
  if a region is selected, all locations below the region in the hierarchy are by definition also selected
*/
export const cascadeSelectRegion = (region, selected) => {
  return {
    ...region,
    selected: selected,
    districts: !!region.districts
      ? region.districts.map((d) => ({
          ...d,
          selected: selected,
          villages: !!d.villages
            ? d.villages.map((v) => ({
                ...v,
                selected: selected,
                zones: !!v.zones
                  ? v.zones.map((z) => ({
                      ...z,
                      selected: selected,
                    }))
                  : [],
              }))
            : [],
        }))
      : [],
  };
};

/*
  if a district is selected, all locations below the district in the hierarchy are by definition also selected
  additionally, if all districts in the region are selected after this, the corresponding region is also selected
*/
export const cascadeSelectDistrict = (region, districtId, districtSelected) => {
  const districts = region.districts.map((d) =>
    setDistrict(d, districtId, districtSelected)
  );
  const hasSelectedDistricts = districts.some((d) => d.selected);

  return {
    ...region,
    selected: hasSelectedDistricts,
    districts: districts,
  };
};

/*
  if a village is selected, all locations below the village in the hierarchy are by definition also selected
  additionally, if all villages and districts in the region are selected after this, the region and district are also selected
*/
export const cascadeSelectVillage = (
  region,
  districtId,
  villageId,
  villageSelected
) => {
  const villages = region.districts
    .filter((d) => d.id === districtId)
    .map((d) =>
      d.villages.map((v) => setVillage(v, villageId, villageSelected))
    )
    .flat();
  const anySelectedVillages = villages.some((v) => v.selected);
  const districts = region.districts.map((d) => ({
    ...d,
    selected: d.id === districtId ? anySelectedVillages : d.selected,
    villages: d.id === districtId ? villages : d.villages,
  }));
  const anySelectedDistricts = districts.some((d) => d.selected);

  return {
    ...region,
    selected: anySelectedDistricts,
    districts: districts,
  };
};

/*
  if a zone is selected and that results in all the zones in the village being selected, the village is also selected
  this trickles upwards to the region level
*/
export const cascadeSelectZone = (
  region,
  districtId,
  villageId,
  zoneId,
  zoneSelected
) => {
  const zones = region.districts
    .filter((d) => d.id === districtId)
    .map((d) =>
      d.villages
        .filter((v) => v.id === villageId)
        .map((v) => v.zones.map((z) => setZone(z, zoneId, zoneSelected)))
    )
    .flat(2);

  const anySelectedZones = zones.some((z) => z.selected);
  const villages = region.districts
    .filter((d) => d.id === districtId)
    .map((d) =>
      d.villages.map((v) => ({
        ...v,
        selected: v.id === villageId ? anySelectedZones : v.selected,
        zones: v.id === villageId ? zones : v.zones,
      }))
    )
    .flat();
  const anySelectedVillages = villages.some((v) => v.selected);
  const districts = region.districts.map((d) => ({
    ...d,
    selected: d.id === districtId ? anySelectedVillages : d.selected,
    villages: d.id === districtId ? villages : d.villages,
  }));
  const anySelectedDistricts = districts.some((d) => d.selected);

  return {
    ...region,
    selected: anySelectedDistricts,
    districts: [...districts],
  };
};

/*
  renders a concatenated label to display when the filter is closed
*/
export const renderFilterLabel = (
  filterValue,
  regions,
  showUnknownLocation
) => {
  if (
    !filterValue ||
    (showUnknownLocation &&
      filterValue.regionIds.length === regions.length &&
      filterValue.includeUnknownLocation) ||
    (!showUnknownLocation && filterValue.regionIds.length === regions.length)
  ) {
    return strings(stringKeys.filters.area.all);
  }

  if (filterValue.regionIds.length > 0) {
    const regionNames = filterValue.regionIds.map(
      (id) => regions.find((r) => r.id === id).name
    );

    return regionNames.length > 1
      ? `${regionNames[0]} (+${regionNames.length - 1})`
      : regionNames[0];
  }

  if (filterValue.districtIds.length > 0) {
    const districtNames = filterValue.districtIds
      .map((id) =>
        regions.map((r) =>
          r.districts.filter((d) => d.id === id).map((d) => d.name)
        )
      )
      .flat(2);

    return districtNames.length > 1
      ? `${districtNames[0]} (+${districtNames.length - 1})`
      : districtNames[0];
  }

  if (filterValue.villageIds.length > 0) {
    const villageNames = filterValue.villageIds
      .map((id) =>
        regions.map((r) =>
          r.districts.map((d) =>
            d.villages.filter((v) => v.id === id).map((v) => v.name)
          )
        )
      )
      .flat(3);

    return villageNames.length > 1
      ? `${villageNames[0]} (+${villageNames.length - 1})`
      : villageNames[0];
  }

  if (filterValue.zoneIds.length > 0) {
    const zoneNames = filterValue.zoneIds
      .map((id) =>
        regions.map((r) =>
          r.districts.map((d) =>
            d.villages.map((v) =>
              v.zones.filter((z) => z.id === id).map((z) => z.name)
            )
          )
        )
      )
      .flat(4);

    return zoneNames.length > 1
      ? `${zoneNames[0]} (+ ${zoneNames.length - 1})`
      : zoneNames[0];
  }

  if (filterValue.includeUnknownLocation) {
    return strings(stringKeys.filters.area.unknown);
  }

  return "";
};

const setDistrict = (district, districtToUpdateId, selected) => {
  if (district.id !== districtToUpdateId) {
    return district;
  }

  return {
    ...district,
    selected: selected,
    villages: !!district.villages
      ? district.villages.map((v) => ({
          ...v,
          selected: selected,
          zones: !!v.zones
            ? v.zones.map((z) => ({
                ...z,
                selected: selected,
              }))
            : [],
        }))
      : [],
  };
};

const setVillage = (village, villageToUpdateId, selected) => {
  if (village.id !== villageToUpdateId) {
    return village;
  }

  return {
    ...village,
    selected: selected,
    zones: !!village.zones
      ? village.zones.map((z) => ({
          ...z,
          selected: selected,
        }))
      : [],
  };
};

const setZone = (zone, zoneToUpdateId, selected) => {
  if (zone.id !== zoneToUpdateId) {
    return zone;
  }

  return {
    ...zone,
    selected: selected,
  };
};
