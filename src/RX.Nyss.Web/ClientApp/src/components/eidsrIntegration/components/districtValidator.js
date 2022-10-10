import {stringKeys, strings, stringsFormat} from "../../../strings";

export const districtValidator = {
  allOrganisationUnits: [() => "All districts need to have organisation unit", (value) => {
    if(value && value.length > 0){
      for (let i = 0; i < value.length; i++) {
        let districtItem = value[i];

        if (districtItem.organisationUnitId == null || districtItem.organisationUnitId.trim() === ''){
          return false;
        }

        if (districtItem.organisationUnitName == null || districtItem.organisationUnitName.trim() === ''){
          return false;
        }
      }
    }

    return true;
  }],
}