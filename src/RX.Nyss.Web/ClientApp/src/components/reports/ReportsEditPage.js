import styles from "./ReportsEditPage.module.scss";

import { useEffect, useState, Fragment, useReducer } from "react";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm } from "../../utils/forms";
import * as reportsActions from "./logic/reportsActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import TextInputField from "../forms/TextInputField";
import DateInputField from "../forms/DateInputField";
import SelectField from "../forms/SelectField";
import { MenuItem, Grid } from "@material-ui/core";
import { Loading } from "../common/loading/Loading";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { ValidationMessage } from "../forms/ValidationMessage";
import dayjs from "dayjs";
import {
  reportAges,
  reportCountToSexAge,
  reportSexes,
  reportStatus,
  ReportType,
} from "./logic/reportsConstants";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import { SubMenuTitle } from "../layout/SubMenuTitle";
import { trackPageView } from "../../utils/appInsightsHelper";

const ReportsEditPageComponent = (props) => {
  const [form, setForm] = useState(null);
  const [selectedDataCollector, setSelectedDataCollector] = useState(null);
  const [availableReportStatus, setAvailableReportStatus] = useState(null);
  const [reportSex, setReportSex] = useState(null);
  const [reportAge, setReportAge] = useState(null);

  const [selectedLocation, setLocation] = useReducer(
    (state, locationId) => {
      if (props.data !== null && state.id !== locationId) {
        return (
          props.dataCollectors
            .find((dc) => dc.id === selectedDataCollector.id)
            .locations.find((lc) => lc.id.toString() === locationId) || state
        );
      }

      return state;
    },
    { id: 0 },
  );

  useMount(() => {
    props.openEdition(props.projectId, props.reportId);

    // Track page view
    trackPageView("ReportsEditPage");
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

    if (!!props.data.dataCollectorId && !!props.data.locationId) {
      setLocation(props.data.locationId.toString());
    }

    if (props.data.reportStatus === reportStatus.new) {
      setAvailableReportStatus([
        reportStatus.new,
        reportStatus.accepted,
        reportStatus.rejected,
      ]);
    }
    if (
      props.data.reportStatus !== reportStatus.new &&
      props.data.reportStatus !== reportStatus.closed
    ) {
      setAvailableReportStatus([
        reportStatus.pending,
        reportStatus.accepted,
        reportStatus.rejected,
      ]);
    }
    if (props.data.reportStatus === reportStatus.closed) {
      setAvailableReportStatus([reportStatus.closed]);
    }

    const fields = {
      id: props.data.id,
      date: dayjs(props.data.date),
      dataCollectorId: !!props.data.dataCollectorId
        ? props.data.dataCollectorId.toString()
        : "",
      reportStatus: props.data.reportStatus,
      healthRiskId: props.data.healthRiskId.toString(),
      locationId: !!props.data.locationId
        ? props.data.locationId.toString()
        : "",
      reportSex: findSexAgeHelper(props.data).sex,
      reportAge: findSexAgeHelper(props.data).age,
      countMalesBelowFive: !!props.data.countMalesBelowFive
        ? props.data.countMalesBelowFive.toString()
        : "",
      countMalesAtLeastFive: !!props.data.countMalesAtLeastFive
        ? props.data.countMalesAtLeastFive.toString()
        : "",
      countFemalesBelowFive: !!props.data.countFemalesBelowFive
        ? props.data.countFemalesBelowFive.toString()
        : "",
      countFemalesAtLeastFive: !!props.data.countFemalesAtLeastFive
        ? props.data.countFemalesAtLeastFive.toString()
        : "",
      countUnspecifiedSexAndAge: !!props.data.countUnspecifiedSexAndAge
        ? props.data.countUnspecifiedSexAndAge.toString()
        : "",
      referredCount: !!props.data.referredCount
        ? props.data.referredCount.toString()
        : "",
      
    };

    const validation = {
      date: [validators.required],
      dataCollectorId: [validators.required],
      reportStatus: [validators.required],
      locationId: [validators.required],
      reportSex: [
        validators.requiredWhen(
          () => props.data.reportType === ReportType.single,
        ),
        validators.sexAge((x) => x.reportAge),
      ],
      reportAge: [
        validators.requiredWhen(
          () => props.data.reportType === ReportType.single,
        ),
        validators.sexAge((x) => x.reportSex),
      ],
      healthRiskId: [validators.required],
      countMalesBelowFive: [validators.integer, validators.nonNegativeNumber],
      countMalesAtLeastFive: [validators.integer, validators.nonNegativeNumber],
      countFemalesBelowFive: [validators.integer, validators.nonNegativeNumber],
      countFemalesAtLeastFive: [
        validators.integer,
        validators.nonNegativeNumber,
      ],
      referredCount: [validators.integer, validators.nonNegativeNumber],
    };

    const newForm = createForm(fields, validation);

    setSelectedDataCollector(
      props.dataCollectors.find((dc) => dc.id === props.data.dataCollectorId),
    );
    setReportSex(fields.reportSex);
    setReportAge(fields.reportAge);

    newForm.fields.locationId.subscribe(({ newValue }) =>
      setLocation(newValue),
    );
    newForm.fields.reportSex.subscribe(({ newValue, form }) => {
      setReportSex(newValue);
      newForm.revalidateField(form.reportAge, form);
    });
    newForm.fields.reportAge.subscribe(({ newValue, form }) => {
      setReportAge(newValue);
      newForm.revalidateField(form.reportSex, form);
    });

    setForm(newForm);
  }, [props.data, props.dataCollectors]);

  const handleDataCollectorChange = (event) => {
    form.fields.locationId.update("", true);

    const newDataCollector = props.dataCollectors.find(
      (dc) => dc.id.toString() === event.target.value,
    );
    setSelectedDataCollector(newDataCollector);
    if (newDataCollector?.locations.length === 1) {
      form.fields.locationId.update(
        newDataCollector.locations[0].id.toString(),
      );
      setLocation(newDataCollector.locations[0].id.toString());
    }
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    }

    const values = form.getValues();

    if (props.data.reportType === ReportType.single) {
      Object.keys(reportCountToSexAge).forEach((comb) => (values[comb] = "0"));
      values[findSexAgeCombinationHelper()] = "1";
    }

    props.edit(props.projectId, props.reportId, {
      date: values.date.format("YYYY-MM-DDTHH:mm:ss"),
      dataCollectorId: parseInt(values.dataCollectorId),
      dataCollectorLocationId: parseInt(values.locationId),
      reportStatus: values.reportStatus,
      healthRiskId: parseInt(values.healthRiskId),
      countMalesBelowFive: parseInt(values.countMalesBelowFive),
      countMalesAtLeastFive: parseInt(values.countMalesAtLeastFive),
      countFemalesBelowFive: parseInt(values.countFemalesBelowFive),
      countFemalesAtLeastFive: parseInt(values.countFemalesAtLeastFive),
      countUnspecifiedSexAndAge: parseInt(values.countUnspecifiedSexAndAge),
      referredCount:
        values.referredCount === "" ? 0 : parseInt(values.referredCount),
    });
  };

  const findSexAgeCombinationHelper = () =>
    Object.keys(reportCountToSexAge).find(
      (key) =>
        reportCountToSexAge[key].sex === reportSex &&
        reportCountToSexAge[key].age === reportAge,
    );

  const findSexAgeHelper = (data) => {
    const key = Object.keys(reportCountToSexAge).find((key) => data[key] > 0);
    return key ? reportCountToSexAge[key] : { sex: "", age: "" };
  };

  const dataCollectorAndLocationSelected = () =>
    selectedDataCollector && selectedLocation.id !== 0;

  if (props.isFetching || !form || !props.data) {
    return <Loading />;
  }

  return (
    <Fragment>
      <SubMenuTitle />
      {props.error && <ValidationMessage message={props.error} />}

      <Form onSubmit={handleSubmit}>
        <Grid
          container
          spacing={2}
          direction="column"
          className={styles.correctNegativeMargin}
        >
          <div className={styles.formSectionTitle}>
            {strings(stringKeys.reports.form.senderSectionTitle)}
          </div>
          <Grid item>
            <SelectField
              label={strings(stringKeys.reports.form.dataCollector)}
              name="dataCollectorId"
              field={form.fields.dataCollectorId}
              disabled={props.data.reportStatus !== reportStatus.new}
              disabledlabel={strings(
                stringKeys.reports.form.reportPartOfAlertLabel,
              )}
              onChange={handleDataCollectorChange}
            >
              {props.dataCollectors.map((dataCollector) => (
                <MenuItem
                  key={`dataCollector_${dataCollector.id}`}
                  value={dataCollector.id.toString()}
                >
                  {dataCollector.name}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>

          <Grid item>
            <SelectField
              label={strings(stringKeys.common.location)}
              name="locationId"
              field={form.fields.locationId}
              disabled={
                props.data.reportStatus !== reportStatus.new ||
                !selectedDataCollector
              }
              disabledlabel={
                !selectedDataCollector
                  ? strings(stringKeys.reports.form.selectDcFirst)
                  : null
              }
            >
              {selectedDataCollector &&
                selectedDataCollector.locations.map((location) => (
                  <MenuItem
                    key={`dataCollectorLocations_${location.id}`}
                    value={location.id.toString()}
                  >
                    {location.village +
                      (location.zone ? " > " + location.zone : "")}
                  </MenuItem>
                ))}
            </SelectField>
          </Grid>

          {props.data.reportType !== ReportType.dataCollectionPoint &&
            props.data.reportType !== ReportType.aggregate &&
            !props.data.isActivityReport && (
              <Fragment>
                <div className={styles.formSectionTitle}>
                  {strings(stringKeys.reports.form.statusSectionTitle)}
                </div>
                <Grid item>
                  <SelectField
                    label={strings(stringKeys.reports.form.reportStatus)}
                    name="reportStatus"
                    field={form.fields.reportStatus}
                    disabled={
                      props.data.reportStatus === reportStatus.closed ||
                      !dataCollectorAndLocationSelected()
                    }
                    disabledlabel={
                      !dataCollectorAndLocationSelected()
                        ? strings(
                            stringKeys.reports.form.selectDcAndLocationFirst,
                          )
                        : null
                    }
                  >
                    {availableReportStatus.map((status) => (
                      <MenuItem
                        key={`status_${status}`}
                        value={status.toString()}
                      >
                        {strings(stringKeys.reports.status[status])}
                      </MenuItem>
                    ))}
                  </SelectField>
                </Grid>
              </Fragment>
            )}

          {props.data.reportType !== ReportType.event && (
            <div className={styles.formSectionTitle}>
              {strings(stringKeys.reports.form.contentSectionTitle)}
            </div>
          )}

          {props.data.reportType === ReportType.single && (
            <Fragment>
              <Grid item>
                <SelectField
                  label={strings(stringKeys.reports.form.reportSex)}
                  name="reportSex"
                  field={form.fields.reportSex}
                >
                  {Object.keys(reportSexes).map((sex) => (
                    <MenuItem
                      key={`sex_${reportSexes[sex]}`}
                      value={reportSexes[sex].toString()}
                    >
                      {strings(
                        stringKeys.reports.sexAgeConstants[reportSexes[sex]],
                      )}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>

              <Grid item>
                <SelectField
                  label={strings(stringKeys.reports.form.reportAge)}
                  name="reportAge"
                  field={form.fields.reportAge}
                >
                  {Object.keys(reportAges).map((age) => (
                    <MenuItem
                      key={`age_${reportAges[age]}`}
                      value={reportAges[age].toString()}
                    >
                      {strings(
                        stringKeys.reports.sexAgeConstants[reportAges[age]],
                      )}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>
            </Fragment>
          )}

          {(props.data.reportType === ReportType.dataCollectionPoint ||
            props.data.reportType === ReportType.aggregate) && (
            <Fragment>
              <Grid item>
                <DateInputField
                  className={styles.fullWidth}
                  label={strings(stringKeys.reports.form.date)}
                  name="date"
                  field={form.fields.date}
                />
              </Grid>

              <Grid item>
                <SelectField
                  label={strings(stringKeys.reports.form.healthRisk)}
                  name="healthRiskId"
                  field={form.fields.healthRiskId}
                >
                  {props.healthRisks.map((healthRisk) => (
                    <MenuItem
                      key={`healthRisk_${healthRisk.id}`}
                      value={healthRisk.id.toString()}
                    >
                      {healthRisk.name}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>

              <Grid item>
                <TextInputField
                  label={strings(stringKeys.reports.form.malesBelowFive)}
                  name="countMalesBelowFive"
                  field={form.fields.countMalesBelowFive}
                />
              </Grid>

              <Grid item>
                <TextInputField
                  label={strings(stringKeys.reports.form.malesAtLeastFive)}
                  name="countMalesAtLeastFive"
                  field={form.fields.countMalesAtLeastFive}
                />
              </Grid>

              <Grid item>
                <TextInputField
                  label={strings(stringKeys.reports.form.femalesBelowFive)}
                  name="countFemalesBelowFive"
                  field={form.fields.countFemalesBelowFive}
                />
              </Grid>

              <Grid item>
                <TextInputField
                  label={strings(stringKeys.reports.form.femalesAtLeastFive)}
                  name="countFemalesAtLeastFive"
                  field={form.fields.countFemalesAtLeastFive}
                />
              </Grid>
            </Fragment>
          )}

          {props.data.reportType === ReportType.dataCollectionPoint && (
            <Fragment>
              <Grid item>
                <TextInputField
                  label={strings(stringKeys.reports.form.referredCount)}
                  name="referredCount"
                  field={form.fields.referredCount}
                />
              </Grid>
            </Fragment>
          )}

          <FormActions className={styles.tableActionsContainer}>
            <CancelButton onClick={() => props.goToList(props.projectId)}>
              {strings(stringKeys.form.cancel)}
            </CancelButton>
            <SubmitButton
              className={styles.editButton}
              isFetching={props.isSaving}
            >
              {strings(stringKeys.common.buttons.update)}
            </SubmitButton>
          </FormActions>
        </Grid>
      </Form>
    </Fragment>
  );
};

ReportsEditPageComponent.propTypes = {};

const mapStateToProps = (state, ownProps) => ({
  reportId: ownProps.match.params.reportId,
  projectId: ownProps.match.params.projectId,
  isFetching: state.reports.formFetching,
  isSaving: state.reports.formSaving,
  data: state.reports.formData,
  error: state.reports.formError,
  healthRisks: state.reports.editReport.formHealthRisks,
  dataCollectors: state.reports.editReport.formDataCollectors,
});

const mapDispatchToProps = {
  openEdition: reportsActions.openEdition.invoke,
  edit: reportsActions.edit.invoke,
  goToList: reportsActions.goToList,
};

export const ReportsEditPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ReportsEditPageComponent),
);
