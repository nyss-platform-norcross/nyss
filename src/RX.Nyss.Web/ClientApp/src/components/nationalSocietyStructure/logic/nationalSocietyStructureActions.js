import {
  OPEN_NATIONAL_SOCIETY_STRUCTURE,
  CREATE_REGION,
  EDIT_REGION,
  REMOVE_REGION,
  CREATE_DISTRICT,
  EDIT_DISTRICT,
  REMOVE_DISTRICT,
  CREATE_VILLAGE,
  EDIT_VILLAGE,
  REMOVE_VILLAGE,
  CREATE_ZONE,
  EDIT_ZONE,
  REMOVE_ZONE,
} from "./nationalSocietyStructureConstants";

export const openStructure = {
  invoke: (nationalSocietyId) => ({
    type: OPEN_NATIONAL_SOCIETY_STRUCTURE.INVOKE,
    nationalSocietyId,
  }),
  request: () => ({ type: OPEN_NATIONAL_SOCIETY_STRUCTURE.REQUEST }),
  success: (regions, districts, villages, zones) => ({
    type: OPEN_NATIONAL_SOCIETY_STRUCTURE.SUCCESS,
    regions,
    districts,
    villages,
    zones,
  }),
  failure: (message) => ({
    type: OPEN_NATIONAL_SOCIETY_STRUCTURE.FAILURE,
    message,
  }),
};

export const createRegion = {
  invoke: (nationalSocietyId, name) => ({
    type: CREATE_REGION.INVOKE,
    nationalSocietyId,
    name,
  }),
  request: () => ({ type: CREATE_REGION.REQUEST }),
  success: (data) => ({ type: CREATE_REGION.SUCCESS, region: data }),
  failure: (message) => ({ type: CREATE_REGION.FAILURE, message }),
};

export const editRegion = {
  invoke: (id, name) => ({ type: EDIT_REGION.INVOKE, id, name }),
  request: (id) => ({ type: EDIT_REGION.REQUEST, id }),
  success: (id, name) => ({ type: EDIT_REGION.SUCCESS, id, name }),
  failure: (message) => ({ type: EDIT_REGION.FAILURE, message }),
};

export const removeRegion = {
  invoke: (id) => ({ type: REMOVE_REGION.INVOKE, id }),
  request: (id) => ({ type: REMOVE_REGION.REQUEST, id }),
  success: (id) => ({ type: REMOVE_REGION.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_REGION.FAILURE, id, message }),
};

export const createDistrict = {
  invoke: (regionId, name) => ({
    type: CREATE_DISTRICT.INVOKE,
    regionId,
    name,
  }),
  request: () => ({ type: CREATE_DISTRICT.REQUEST }),
  success: (regionId, data) => ({
    type: CREATE_DISTRICT.SUCCESS,
    regionId,
    district: data,
  }),
  failure: (message) => ({ type: CREATE_DISTRICT.FAILURE, message }),
};

export const editDistrict = {
  invoke: (id, name) => ({ type: EDIT_DISTRICT.INVOKE, id, name }),
  request: (id) => ({ type: EDIT_DISTRICT.REQUEST, id }),
  success: (id, name) => ({ type: EDIT_DISTRICT.SUCCESS, id, name }),
  failure: (message) => ({ type: EDIT_DISTRICT.FAILURE, message }),
};

export const removeDistrict = {
  invoke: (id) => ({ type: REMOVE_DISTRICT.INVOKE, id }),
  request: (id) => ({ type: REMOVE_DISTRICT.REQUEST, id }),
  success: (id) => ({ type: REMOVE_DISTRICT.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_DISTRICT.FAILURE, id, message }),
};

export const createVillage = {
  invoke: (districtId, name) => ({
    type: CREATE_VILLAGE.INVOKE,
    districtId,
    name,
  }),
  request: () => ({ type: CREATE_VILLAGE.REQUEST }),
  success: (data) => ({ type: CREATE_VILLAGE.SUCCESS, village: data }),
  failure: (message) => ({ type: CREATE_VILLAGE.FAILURE, message }),
};

export const editVillage = {
  invoke: (id, name) => ({ type: EDIT_VILLAGE.INVOKE, id, name }),
  request: (id) => ({ type: EDIT_VILLAGE.REQUEST, id }),
  success: (id, name) => ({ type: EDIT_VILLAGE.SUCCESS, id, name }),
  failure: (message) => ({ type: EDIT_VILLAGE.FAILURE, message }),
};

export const removeVillage = {
  invoke: (id) => ({ type: REMOVE_VILLAGE.INVOKE, id }),
  request: (id) => ({ type: REMOVE_VILLAGE.REQUEST, id }),
  success: (id) => ({ type: REMOVE_VILLAGE.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_VILLAGE.FAILURE, id, message }),
};

export const createZone = {
  invoke: (villageId, name) => ({ type: CREATE_ZONE.INVOKE, villageId, name }),
  request: () => ({ type: CREATE_ZONE.REQUEST }),
  success: (data) => ({ type: CREATE_ZONE.SUCCESS, zone: data }),
  failure: (message) => ({ type: CREATE_ZONE.FAILURE, message }),
};

export const editZone = {
  invoke: (id, name) => ({ type: EDIT_ZONE.INVOKE, id, name }),
  request: (id) => ({ type: EDIT_ZONE.REQUEST, id }),
  success: (id, name) => ({ type: EDIT_ZONE.SUCCESS, id, name }),
  failure: (message) => ({ type: EDIT_ZONE.FAILURE, message }),
};

export const removeZone = {
  invoke: (id) => ({ type: REMOVE_ZONE.INVOKE, id }),
  request: (id) => ({ type: REMOVE_ZONE.REQUEST, id }),
  success: (id) => ({ type: REMOVE_ZONE.SUCCESS, id }),
  failure: (id, message) => ({ type: REMOVE_ZONE.FAILURE, id, message }),
};
