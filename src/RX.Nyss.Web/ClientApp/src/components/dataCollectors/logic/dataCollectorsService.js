import { performanceStatus } from "./dataCollectorsConstants";

export const getBirthDecades = () => {
  const yearMax = Math.floor(new Date().getFullYear() / 10) * 10;
  return Array.from({ length: 10 }, (_, i) => String(yearMax - (10 * i)));
}

export const getIconFromStatus = (status) => {
  switch (status) {
    case performanceStatus.reportingCorrectly: return "check";
    case performanceStatus.reportingWithErrors: return "close";
    case performanceStatus.notReporting: return "access_time";
    default: return "contact_support";
  }
}

export const getSaveFormModel = (values, dataCollectorType, locations) =>
  ({
    id: values.id,
    dataCollectorType: dataCollectorType,
    name: values.name,
    displayName: values.displayName,
    sex: values.sex,
    supervisorId: parseInt(values.supervisorId),
    birthGroupDecade: parseInt(values.birthGroupDecade),
    additionalPhoneNumber: values.additionalPhoneNumber,
    phoneNumber: values.phoneNumber,
    deployed: values.deployed,
    locations: locations.map((location, number) => ({
      id: location.id || null,
      latitude: values[`location_${number}_latitude`],
      longitude: values[`location_${number}_longitude`],
      regionId: parseInt(values[`location_${number}_regionId`]),
      districtId: parseInt(values[`location_${number}_districtId`]),
      villageId: parseInt(values[`location_${number}_villageId`]),
      zoneId: values[`location_${number}_zoneId`] ? parseInt(values[`location_${number}_zoneId`]) : null
    }))
  });
