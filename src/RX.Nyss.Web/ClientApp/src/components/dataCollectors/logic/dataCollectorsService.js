import { dataCollectorType, performanceStatus } from "./dataCollectorsConstants";
import * as http from '../../../utils/http';
import { apiUrl } from "../../../utils/variables";

export const getIconFromStatus = (status) => {
  switch (status) {
    case performanceStatus.reportingCorrectly: return "check";
    case performanceStatus.reportingWithErrors: return "close";
    case performanceStatus.notReporting: return "access_time";
    default: return null;
  }
}

export const getSaveFormModel = (projectId, values, type, locations) =>
  ({
    projectId: projectId,
    id: values.id,
    dataCollectorType: type,
    name: values.name,
    displayName: values.displayName,
    sex: type === dataCollectorType.human ? values.sex : null,
    supervisorId: parseInt(values.supervisorId),
    birthGroupDecade: type === dataCollectorType.human ? parseInt(values.birthGroupDecade) : null,
    additionalPhoneNumber: values.additionalPhoneNumber,
    phoneNumber: values.phoneNumber,
    deployed: values.deployed,
    locations: locations.map(location => ({
      id: location.id || null,
      latitude: parseFloat(values[`locations_${location.number}_latitude`]),
      longitude: parseFloat(values[`locations_${location.number}_longitude`]),
      regionId: parseInt(values[`locations_${location.number}_regionId`]),
      districtId: parseInt(values[`locations_${location.number}_districtId`]),
      villageId: parseInt(values[`locations_${location.number}_villageId`]),
      zoneId: values[`locations_${location.number}_zoneId`] ? parseInt(values[`locations_${location.number}_zoneId`]) : null
    })),
    linkedToHeadSupervisor: values.linkedToHeadSupervisor
  });

export const getFormDistricts = (regionId, callback) =>
  http.get(`${apiUrl}/api/nationalSocietyStructure/district/list?regionId=${regionId}`)
    .then(response => callback(response.value));

export const getFormVillages = (districtId, callback) =>
  http.get(`${apiUrl}/api/nationalSocietyStructure/village/list?districtId=${districtId}`)
    .then(response => callback(response.value));

export const getFormZones = (villageId, callback) =>
  http.get(`${apiUrl}/api/nationalSocietyStructure/zone/list?villageId=${villageId}`)
    .then(response => callback(response.value));
