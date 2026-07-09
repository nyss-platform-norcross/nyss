import React, { useEffect } from "react";
import ExpandMoreIcon from "@material-ui/icons/ExpandMore";
import Typography from "@material-ui/core/Typography";
import {
  DistrictAccordion,
  DistrictAccordionDetails,
  DistrictAccordionSummary,
} from "./DistrictAccordion";
import { Button, Grid } from "@material-ui/core";
import WarningIcon from "@material-ui/icons/Warning";
import CheckCircleIcon from "@material-ui/icons/CheckCircle";
import { VirtualizedAutocomplete } from "../../forms/VirtualizedAutocomplete";
import { stringKeys, strings } from "../../../strings";

export const EidsrIntegrationEditPageDistrictsComponent = ({
  districtsWithOrganizationUnits,
  integrationEditingDisabled,
  organisationUnits,
  organisationUnitsIsFetching,
  onChange,
}) => {
  const [districtAccordionExpanded, setDistrictAccordionExpanded] =
    React.useState("");
  const [districtDropdownsValues, setDistrictDropdownsValues] = React.useState(
    {},
  );
  const [
    districtsWithOrganizationUnitsState,
    setDistrictsWithOrganizationUnitsState,
  ] = React.useState(districtsWithOrganizationUnits);

  const districtAccordionChange = (panel) => (event, newExpanded) => {
    setDistrictAccordionExpanded(newExpanded ? panel : false);
  };

  const updateDistrictDropdownsValues = (index, value) => {
    let newTab = districtDropdownsValues;
    newTab[index] = value;
    setDistrictDropdownsValues(newTab);
  };

  useEffect(() => {
    onChange(districtsWithOrganizationUnitsState);
  }, [JSON.stringify(districtsWithOrganizationUnitsState)]);

  const applyOrganisationUnit = (index) => {
    let newState = districtsWithOrganizationUnitsState;

    const newValue = districtDropdownsValues[index];
    const idx = newState.findIndex((x) => x.districtId === index);

    newState[idx] = {
      ...newState[idx],
      organisationUnitId: newValue?.id,
      organisationUnitName: newValue?.display,
    };

    setDistrictsWithOrganizationUnitsState(newState);
    setDistrictAccordionExpanded("");
  };

  const organisationUnitsOptions = organisationUnits.map((option) => {
    return {
      id: option.id,
      display: option.displayName,
    };
  });

  return (
    <>
      {districtsWithOrganizationUnitsState?.map((item, index) => (
        <DistrictAccordion
          key={item.districtId}
          square
          style={{ opacity: integrationEditingDisabled ? 0.6 : 1 }}
          expanded={districtAccordionExpanded === item.districtId}
          onChange={districtAccordionChange(item.districtId)}
        >
          <DistrictAccordionSummary
            expandIcon={integrationEditingDisabled ? "" : <ExpandMoreIcon />}
            aria-controls={`panel${item.districtId}-content`}
            id={`panel${item.districtId}-header`}
          >
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Typography variant="caption">
                  {" "}
                  {strings(stringKeys.eidsrIntegration.form.districts)}{" "}
                </Typography>
                <Typography>{item.districtName}</Typography>
              </Grid>
              <Grid item xs={6}>
                <Typography variant="caption">
                  {" "}
                  {strings(
                    stringKeys.eidsrIntegration.form.organisationUnits,
                  )}{" "}
                </Typography>
                {item.organisationUnitId ? (
                  <Typography>
                    {item.organisationUnitName}{" "}
                    <CheckCircleIcon
                      style={{ color: "green", fontSize: "inherit" }}
                    />
                  </Typography>
                ) : (
                  <Typography>
                    {" "}
                    <WarningIcon fontSize="small" color={"error"} />
                  </Typography>
                )}
              </Grid>
            </Grid>
          </DistrictAccordionSummary>
          {!integrationEditingDisabled && (
            <DistrictAccordionDetails>
              <Grid container spacing={2} alignItems="center">
                <Grid item xs={10}>
                  <VirtualizedAutocomplete
                    id={item.districtId.toString()}
                    l
                    label={strings(
                      stringKeys.eidsrIntegration.form.choseOrganisationUnit,
                    )}
                    options={organisationUnitsOptions}
                    handleChangeValue={(value) =>
                      updateDistrictDropdownsValues(item.districtId, value)
                    }
                    optionsLoading={organisationUnitsIsFetching}
                    loadingText={strings(
                      stringKeys.eidsrIntegration.form.loading,
                    )}
                    noOptionsText={strings(
                      stringKeys.eidsrIntegration.form.noOptions,
                    )}
                  />
                </Grid>
                <Grid item xs={2}>
                  <Button
                    color="primary"
                    variant="contained"
                    size={"small"}
                    onClick={() => {
                      applyOrganisationUnit(item.districtId);
                    }}
                  >
                    Save
                  </Button>
                </Grid>
              </Grid>
            </DistrictAccordionDetails>
          )}
        </DistrictAccordion>
      ))}
      {districtsWithOrganizationUnitsState.length === 0 && (
        <p> {strings(stringKeys.eidsrIntegration.form.noDistricts)} </p>
      )}
    </>
  );
};
