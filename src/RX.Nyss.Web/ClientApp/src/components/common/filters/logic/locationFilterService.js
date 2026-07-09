import { stringKeys, strings, stringsFormat } from "../../../../strings";

const ALL_EXCEPT_THRESHOLD = 5;

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
        .map((d) => d.id),
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
            .map((v) => v.id),
        ),
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
            .map((v) => v.zones.filter((z) => z.selected).map((z) => z.id)),
        ),
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
    setDistrict(d, districtId, districtSelected),
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
  villageSelected,
) => {
  const villages = region.districts
    .filter((d) => d.id === districtId)
    .map((d) =>
      d.villages.map((v) => setVillage(v, villageId, villageSelected)),
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
  zoneSelected,
) => {
  const zones = region.districts
    .filter((d) => d.id === districtId)
    .map((d) =>
      d.villages
        .filter((v) => v.id === villageId)
        .map((v) => v.zones.map((z) => setZone(z, zoneId, zoneSelected))),
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
      })),
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

const compactNameDisplay = (locationNames, withUnknownLocation) => {
  if (withUnknownLocation) {
    return locationNames.length >= 1
      ? `${locationNames[0]} (+${locationNames.length})`
      : locationNames[0];
  }

  return locationNames.length > 1
    ? `${locationNames[0]} (+${locationNames.length - 1})`
    : locationNames[0];
};

const countTotalLeaves = (regions) => {
  let count = 0;
  regions.forEach((region) => {
    if (!region.districts?.length) {
      count += 1;
      return;
    }
    region.districts.forEach((district) => {
      if (!district.villages?.length) {
        count += 1;
        return;
      }
      district.villages.forEach((village) => {
        if (village.zones?.length > 0) {
          count += village.zones.length;
        } else {
          count += 1;
        }
      });
    });
  });
  return count;
};

const countSelectedLeaves = (mappedRegions) => {
  let count = 0;
  mappedRegions.forEach((region) => {
    if (!region.districts?.length) {
      if (region.selected) count += 1;
      return;
    }
    region.districts.forEach((district) => {
      if (!district.villages?.length) {
        if (district.selected) count += 1;
        return;
      }
      district.villages.forEach((village) => {
        if (village.zones?.length > 0) {
          count += village.zones.filter((zone) => zone.selected).length;
        } else if (village.selected) {
          count += 1;
        }
      });
    });
  });
  return count;
};

const isRegionFullySelected = (region) => {
  if (!region.districts?.length) {
    return region.selected;
  }

  return region.districts.every((district) => {
    if (!district.selected) return false;
    if (!district.villages?.length) return true;

    return district.villages.every((village) => {
      if (!village.selected) return false;
      if (!village.zones?.length) return true;
      return village.zones.every((zone) => zone.selected);
    });
  });
};

const hasAnySelectionInRegion = (region) => {
  if (!region.districts?.length) {
    return region.selected;
  }

  return region.districts.some(
    (district) =>
      district.selected ||
      district.villages.some(
        (village) =>
          village.selected || village.zones.some((zone) => zone.selected),
      ),
  );
};

const hasPartialSelection = (filterValue, regions) => {
  const mapped = mapToSelectedLocations(filterValue, regions);
  return mapped.some(
    (region) =>
      hasAnySelectionInRegion(region) && !isRegionFullySelected(region),
  );
};

const isNothingSelected = (filterValue, showUnknownLocation) =>
  !filterValue.regionIds.length &&
  !filterValue.districtIds.length &&
  !filterValue.villageIds.length &&
  !filterValue.zoneIds.length &&
  (showUnknownLocation ? !filterValue.includeUnknownLocation : true);

const getFullySelectedRegionNames = (filterValue, regions) =>
  mapToSelectedLocations(filterValue, regions)
    .filter((region) => isRegionFullySelected(region))
    .map((region) => region.name);

const renderCountLabel = (filterValue, regions) => {
  const mapped = mapToSelectedLocations(filterValue, regions);
  const total = countTotalLeaves(regions);
  const selected = countSelectedLeaves(mapped);
  const excluded = total - selected;

  if (excluded > 0 && excluded <= ALL_EXCEPT_THRESHOLD) {
    return stringsFormat(stringKeys.filters.area.allExcept, { count: excluded });
  }

  return stringsFormat(stringKeys.filters.area.selectedCount, { count: selected });
};

export const isAllLocationsSelected = (
  filterValue,
  regions,
  showUnknownLocation,
) => {
  if (!filterValue || !regions?.length) return true;

  const mapped = mapToSelectedLocations(filterValue, regions);
  const unknownOk = !showUnknownLocation || filterValue.includeUnknownLocation;

  return (
    countSelectedLeaves(mapped) === countTotalLeaves(regions) && unknownOk
  );
};

/*
  renders a concatenated label to display when the filter is closed
*/
export const renderFilterLabel = (
  filterValue,
  regions,
  showUnknownLocation,
) => {
  if (!filterValue || !regions?.length) {
    return strings(stringKeys.filters.area.all);
  }

  if (isAllLocationsSelected(filterValue, regions, showUnknownLocation)) {
    return strings(stringKeys.filters.area.all);
  }

  if (isNothingSelected(filterValue, showUnknownLocation)) {
    return strings(stringKeys.filters.area.all);
  }

  const withUnknownLocation =
    showUnknownLocation && filterValue.includeUnknownLocation;

  if (!hasPartialSelection(filterValue, regions)) {
    const fullySelectedRegionNames = getFullySelectedRegionNames(
      filterValue,
      regions,
    );
    if (fullySelectedRegionNames.length > 0) {
      return compactNameDisplay(fullySelectedRegionNames, withUnknownLocation);
    }
  }

  if (filterValue.districtIds.length > 0) {
    const districtNames = filterValue.districtIds
      .map((id) =>
        regions.map((r) =>
          r.districts.filter((d) => d.id === id).map((d) => d.name),
        ),
      )
      .flat(2);

    return compactNameDisplay(districtNames, withUnknownLocation);
  }

  if (filterValue.villageIds.length > 0) {
    const villageNames = filterValue.villageIds
      .map((id) =>
        regions.map((r) =>
          r.districts.map((d) =>
            d.villages.filter((v) => v.id === id).map((v) => v.name),
          ),
        ),
      )
      .flat(3);

    return compactNameDisplay(villageNames, withUnknownLocation);
  }

  if (filterValue.zoneIds.length > 0) {
    const zoneNames = filterValue.zoneIds
      .map((id) =>
        regions.map((r) =>
          r.districts.map((d) =>
            d.villages.map((v) =>
              v.zones.filter((z) => z.id === id).map((z) => z.name),
            ),
          ),
        ),
      )
      .flat(4);

    return compactNameDisplay(zoneNames, withUnknownLocation);
  }

  if (withUnknownLocation && isNothingSelected(filterValue, false)) {
    return strings(stringKeys.filters.area.unknown);
  }

  return renderCountLabel(filterValue, regions);
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
