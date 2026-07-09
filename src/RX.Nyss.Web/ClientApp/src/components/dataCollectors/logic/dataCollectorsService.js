import {
  dataCollectorType,
  performanceStatus,
} from "./dataCollectorsConstants";
import * as http from "../../../utils/http";

export const getIconFromStatus = (status) => {
  switch (status) {
    case performanceStatus.reportingCorrectly:
      return "check";
    case performanceStatus.reportingWithErrors:
      return "close";
    case performanceStatus.notReporting:
      return "access_time";
    default:
      return null;
  }
};

export const getSaveFormModel = (projectId, values, type, locations) => {
  // Build base model without displayName
  const model = {
    projectId: projectId,
    id: values.id,
    dataCollectorType: type,
    name: values.name,
    phoneNumber: values.phoneNumber,
    additionalPhoneNumber: values.additionalPhoneNumber,
    supervisorId: parseInt(values.supervisorId),
    deployed: values.deployed,
    locations: locations.map((location) => ({
      id: location.id || null,
      latitude: parseFloat(values[`locations_${location.number}_latitude`]),
      longitude: parseFloat(values[`locations_${location.number}_longitude`]),
      regionId: parseInt(values[`locations_${location.number}_regionId`]),
      districtId: parseInt(values[`locations_${location.number}_districtId`]),
      villageId: parseInt(values[`locations_${location.number}_villageId`]),
      zoneId: values[`locations_${location.number}_zoneId`]
        ? parseInt(values[`locations_${location.number}_zoneId`])
        : null,
    })),
    linkedToHeadSupervisor: values.linkedToHeadSupervisor,
  };
  
  // Only include Human-specific fields for Human data collectors
  // Explicitly omit displayName, sex, and birthGroupDecade for CollectionPoint
  if (type === dataCollectorType.human) {
    model.displayName = values.displayName;
    model.sex = values.sex;
    model.birthGroupDecade = values.birthGroupDecade != null ? parseInt(values.birthGroupDecade) : null;
  }
  
  return model;
};

export const getFormDistricts = (regionId, callback) =>
  http
    .get(`/api/nationalSocietyStructure/district/list?regionId=${regionId}`)
    .then((response) => callback(response.value));

export const getFormVillages = (districtId, callback) =>
  http
    .get(`/api/nationalSocietyStructure/village/list?districtId=${districtId}`)
    .then((response) => callback(response.value));

export const getFormZones = (villageId, callback) =>
  http
    .get(`/api/nationalSocietyStructure/zone/list?villageId=${villageId}`)
    .then((response) => callback(response.value));
