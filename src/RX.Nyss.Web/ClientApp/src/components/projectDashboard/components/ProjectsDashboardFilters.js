import styles from "./ProjectsDashboardFilters.module.scss";
import React, { useState } from "react";
import { DatePicker } from "../../forms/DatePicker";
import { AreaFilter } from "../../common/filters/AreaFilter";
import { strings, stringKeys } from "../../../strings";
import { ConditionalCollapse } from "../../common/conditionalCollapse/ConditionalCollapse";
import { convertToLocalDate, convertToUtc } from "../../../utils/date";
import ExpandMore from "@material-ui/icons/ExpandMore";
import DateRange from "@material-ui/icons/DateRange";
import {
  LinearProgress,
  Chip,
  IconButton,
  Grid,
  TextField,
  MenuItem,
  Card,
  useMediaQuery,
  CardContent,
  CardHeader,
  FormControl,
  FormControlLabel,
  FormLabel,
  RadioGroup,
  Radio,
} from "@material-ui/core";
import { ReportStatusFilter } from "../../common/filters/ReportStatusFilter";
import { Fragment } from "react";
import { DataConsumer } from "../../../authentication/roles";

export const ProjectsDashboardFilters = ({
  filters,
  nationalSocietyId,
  healthRisks,
  organizations,
  onChange,
  isFetching,
  isGeneratingPdf,
  isFilterExpanded,
  setIsFilterExpanded,
  userRoles,
}) => {
  const [value, setValue] = useState(filters);
  const [selectedArea, setSelectedArea] = useState(filters && filters.area);
  const isSmallScreen = useMediaQuery((theme) => theme.breakpoints.down("lg"));

  const updateValue = (change) => {
    const newValue = {
      ...value,
      ...change,
    };

    setValue(newValue);
    return newValue;
  };

  const handleAreaChange = (item) => {
    setSelectedArea(item);
    onChange(
      updateValue({
        area: item ? { type: item.type, id: item.id, name: item.name } : null,
      })
    );
  };

  const handleHealthRiskChange = (event) =>
    onChange(
      updateValue({
        healthRiskId: event.target.value === 0 ? null : event.target.value,
      })
    );

  const handleOrganizationChange = (event) =>
    onChange(
      updateValue({
        organizationId: event.target.value === 0 ? null : event.target.value,
      })
    );

  const handleDateFromChange = (date) =>
    onChange(updateValue({ startDate: convertToUtc(date) }));

  const handleDateToChange = (date) =>
    onChange(updateValue({ endDate: convertToUtc(date) }));

  const handleGroupingTypeChange = (event) =>
    onChange(updateValue({ groupingType: event.target.value }));

  const handleDataCollectorTypeChange = (event) =>
    onChange(updateValue({ dataCollectorType: event.target.value }));

  const handleReportStatusChange = (event) =>
    onChange(
      updateValue({
        reportStatus: {
          ...value.reportStatus,
          [event.target.name]: event.target.checked,
        },
      })
    );

  const handleTrainingStatusChange = (event) =>
    onChange(updateValue({ trainingStatus: event.target.value }));

  const collectionsTypes = {
    all: strings(stringKeys.project.dashboard.filters.allReportsType),
    dataCollector: strings(
      stringKeys.project.dashboard.filters.dataCollectorReportsType
    ),
    dataCollectionPoint: strings(
      stringKeys.project.dashboard.filters.dataCollectionPointReportsType
    ),
  };

  if (!value) {
    return null;
  }

  return (
    <Card className={styles.filters}>
      {isFetching && <LinearProgress color="primary" />}
      {isSmallScreen && (
        <CardContent className={styles.collapsedFilterBar}>
          <Grid container spacing={2} alignItems="center">
            <Grid item>
              <CardHeader
                title={strings(
                  stringKeys.nationalSociety.dashboard.filters.title
                )}
              />
            </Grid>
            {!isFilterExpanded && (
              <Fragment>
                <Grid item>
                  <Chip
                    icon={<DateRange />}
                    label={`${convertToLocalDate(value.startDate).format(
                      "YYYY-MM-DD"
                    )} - ${convertToLocalDate(value.endDate).format(
                      "YYYY-MM-DD"
                    )}`}
                    onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                  />
                </Grid>
                <Grid item>
                  <Chip
                    onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                    label={
                      value.groupingType === "Day"
                        ? strings(
                            stringKeys.project.dashboard.filters.timeGroupingDay
                          )
                        : strings(
                            stringKeys.project.dashboard.filters
                              .timeGroupingWeek
                          )
                    }
                  />
                </Grid>
              </Fragment>
            )}
            {!isFilterExpanded && selectedArea && (
              <Grid item>
                <Chip
                  label={selectedArea.name}
                  onDelete={() => handleAreaChange(null)}
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )}
            {!isFilterExpanded && value.healthRiskId && (
              <Grid item>
                <Chip
                  label={
                    healthRisks.filter((hr) => hr.id === value.healthRiskId)[0]
                      .name
                  }
                  onDelete={() => onChange(updateValue({ healthRiskId: null }))}
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )}
            {!isFilterExpanded && value.dataCollectorType !== "all" && (
              <Grid item>
                <Chip
                  label={collectionsTypes[value.dataCollectorType]}
                  onDelete={() =>
                    onChange(updateValue({ dataCollectorType: "all" }))
                  }
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )}
            {!isFilterExpanded && value.organizationId && (
              <Grid item>
                <Chip
                  label={
                    organizations.filter(
                      (o) => o.id === value.organizationId
                    )[0].name
                  }
                  onDelete={() =>
                    onChange(updateValue({ organizationId: null }))
                  }
                  onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                />
              </Grid>
            )}
            {!isFilterExpanded &&
              !userRoles.some((r) => r === DataConsumer) &&
              value.reportStatus.kept && (
                <Grid item>
                  <Chip
                    label={strings(stringKeys.filters.report.kept)}
                    onDelete={() =>
                      onChange(
                        updateValue({
                          reportStatus: {
                            ...value.reportStatus,
                            kept: false,
                          },
                        })
                      )
                    }
                    onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                  />
                </Grid>
              )}
            {!isFilterExpanded &&
              !userRoles.some((r) => r === DataConsumer) &&
              value.reportStatus.dismissed && (
                <Grid item>
                  <Chip
                    label={strings(stringKeys.filters.report.dismissed)}
                    onDelete={() =>
                      onChange(
                        updateValue({
                          reportStatus: {
                            ...value.reportStatus,
                            dismissed: false,
                          },
                        })
                      )
                    }
                    onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                  />
                </Grid>
              )}
            {!isFilterExpanded &&
              !userRoles.some((r) => r === DataConsumer) &&
              value.reportStatus.notCrossChecked && (
                <Grid item>
                  <Chip
                    label={strings(stringKeys.filters.report.notCrossChecked)}
                    onDelete={() =>
                      onChange(
                        updateValue({
                          reportStatus: {
                            ...value.reportStatus,
                            notCrossChecked: false,
                          },
                        })
                      )
                    }
                    onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                  />
                </Grid>
              )}
            {!isFilterExpanded &&
              !userRoles.some((r) => r === DataConsumer) &&
              value.trainingStatus !== "Trained" && (
                <Grid item>
                  <Chip
                    label={strings(stringKeys.dataCollector.constants.trainingStatus.InTraining)}
                    onDelete={() =>
                      onChange(
                        updateValue({
                          ...value,
                          trainingStatus: "Trained"
                        })
                      )
                    }
                    onClick={() => setIsFilterExpanded(!isFilterExpanded)}
                  />
                </Grid>
              )}              
            <Grid item className={styles.expandFilterButton}>
              <IconButton
                data-expanded={isFilterExpanded}
                onClick={() => setIsFilterExpanded(!isFilterExpanded)}
              >
                <ExpandMore />
              </IconButton>
            </Grid>
          </Grid>
        </CardContent>
      )}

      <ConditionalCollapse
        collapsible={isSmallScreen && !isGeneratingPdf}
        expanded={isFilterExpanded}
      >
        {!isSmallScreen && (
          <Grid container spacing={2}>
            <CardHeader
              title={strings(
                stringKeys.nationalSociety.dashboard.filters.title
              )}
              className={styles.filterTitle}
            />
          </Grid>
        )}
        <CardContent data-printable={true}>
          <Grid container spacing={2}>
            <Grid item>
              <DatePicker
                className={styles.filterDate}
                onChange={handleDateFromChange}
                label={strings(stringKeys.project.dashboard.filters.startDate)}
                value={convertToLocalDate(value.startDate)}
              />
            </Grid>

            <Grid item>
              <DatePicker
                className={styles.filterDate}
                onChange={handleDateToChange}
                label={strings(stringKeys.project.dashboard.filters.endDate)}
                value={convertToLocalDate(value.endDate)}
              />
            </Grid>

            <Grid item>
              <FormControl>
                <FormLabel component="legend">
                  {strings(stringKeys.project.dashboard.filters.timeGrouping)}
                </FormLabel>
                <RadioGroup
                  value={value.groupingType}
                  onChange={handleGroupingTypeChange}
                  className={styles.radioGroup}
                >
                  <FormControlLabel
                    className={styles.radio}
                    label={strings(
                      stringKeys.project.dashboard.filters.timeGroupingDay
                    )}
                    value={"Day"}
                    control={<Radio color="primary" />}
                  />
                  <FormControlLabel
                    className={styles.radio}
                    label={strings(
                      stringKeys.project.dashboard.filters.timeGroupingWeek
                    )}
                    value={"Week"}
                    control={<Radio color="primary" />}
                  />
                </RadioGroup>
              </FormControl>
            </Grid>

            <Grid item>
              <AreaFilter
                nationalSocietyId={nationalSocietyId}
                selectedItem={selectedArea}
                onChange={handleAreaChange}
              />
            </Grid>

            <Grid item>
              <TextField
                select
                label={strings(stringKeys.project.dashboard.filters.healthRisk)}
                onChange={handleHealthRiskChange}
                value={value.healthRiskId || 0}
                className={styles.filterItem}
                InputLabelProps={{ shrink: true }}
              >
                <MenuItem value={0}>
                  {strings(stringKeys.project.dashboard.filters.healthRiskAll)}
                </MenuItem>

                {healthRisks.map((healthRisk) => (
                  <MenuItem
                    key={`filter_healthRisk_${healthRisk.id}`}
                    value={healthRisk.id}
                  >
                    {healthRisk.name}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>

            <Grid item>
              <TextField
                select
                label={strings(
                  stringKeys.project.dashboard.filters.reportsType
                )}
                onChange={handleDataCollectorTypeChange}
                value={value.dataCollectorType || "all"}
                className={styles.filterItem}
                InputLabelProps={{ shrink: true }}
              >
                <MenuItem value="all">{collectionsTypes["all"]}</MenuItem>
                <MenuItem value="dataCollector">
                  {collectionsTypes["dataCollector"]}
                </MenuItem>
                <MenuItem value="dataCollectionPoint">
                  {collectionsTypes["dataCollectionPoint"]}
                </MenuItem>
              </TextField>
            </Grid>          

            <Grid item>
              <TextField
                select
                label={strings(
                  stringKeys.project.dashboard.filters.organization
                )}
                onChange={handleOrganizationChange}
                value={value.organizationId || 0}
                className={styles.filterItem}
                InputLabelProps={{ shrink: true }}
              >
                <MenuItem value={0}>
                  {strings(
                    stringKeys.project.dashboard.filters.organizationsAll
                  )}
                </MenuItem>

                {organizations.map((organization) => (
                  <MenuItem
                    key={`filter_organization_${organization.id}`}
                    value={organization.id}
                  >
                    {organization.name}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>

            <Grid item>
              <FormControl>
                <FormLabel component="legend">
                  {strings(stringKeys.project.dashboard.filters.dataCollectorStatus)}
                </FormLabel>
                <RadioGroup
                  value={value.trainingStatus}
                  onChange={handleTrainingStatusChange}
                  className={styles.radioGroup}
                >
                  <FormControlLabel
                    className={styles.radio}
                    label={strings(
                      stringKeys.dataCollector.constants.trainingStatus.Trained
                    )}
                    value={"Trained"}
                    control={<Radio color="primary" />}
                  />
                  <FormControlLabel
                    className={styles.radio}
                    label={strings(
                      stringKeys.dataCollector.constants.trainingStatus.InTraining
                    )}
                    value={"InTraining"}
                    control={<Radio color="primary" />}
                  />
                </RadioGroup>
              </FormControl>
            </Grid>            

            {!userRoles.some((r) => r === DataConsumer) && (
              <Grid item>
                <ReportStatusFilter
                  filter={value.reportStatus}
                  correctReports
                  showDismissedFilter
                  onChange={handleReportStatusChange}
                />
              </Grid>
            )}
          </Grid>
        </CardContent>
      </ConditionalCollapse>
    </Card>
  );
};
