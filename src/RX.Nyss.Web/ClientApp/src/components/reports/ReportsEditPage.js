import styles from "./ReportsEditPage.module.scss";

import React, { useEffect, useState, Fragment } from 'react';
import { connect } from "react-redux";
import { withLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as reportsActions from './logic/reportsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import TextInputField from '../forms/TextInputField';
import DateInputField from "../forms/DateInputField";
import SelectField from '../forms/SelectField';
import { MenuItem, Button, Grid } from "@material-ui/core";
import { Loading } from '../common/loading/Loading';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { ValidationMessage } from '../forms/ValidationMessage';
import dayjs from "dayjs";
import {reportAges, reportCountToSexAge, reportSexes, reportStatus} from "./logic/reportsConstants";

const ReportsEditPageComponent = (props) => {
    const [form, setForm] = useState(null);
    const [selectedDataCollector, setDataCollector] = useState(null);
    const [selectedLocation, setLocation] = useState(null);
    const [availableReportStatus, setAvailableReportStatus] = useState(null);
    const [reportSex, setReportSex] = useState(null);
    const [reportAge, setReportAge] = useState(null);

    useMount(() => {
        props.openEdition(props.projectId, props.reportId);
    });

    useEffect(() => {
        if (!props.data) {
            return;
        }

        let reportLocation;
        if (props.data.dataCollectorId !== 0) {
          reportLocation = props.dataCollectors.find(dc => dc.id.toString() === props.data.dataCollectorId.toString())
            .locations.find(lc => (lc.villageId === props.data.reportVillageId && lc.zoneId === props.data.reportZoneId));
          setLocation(reportLocation);
        }

        if (props.data.reportStatus === reportStatus.new) {
          setAvailableReportStatus([reportStatus.new, reportStatus.accepted, reportStatus.rejected]);
        }
        if (props.data.reportStatus !== reportStatus.new && props.data.reportStatus !== reportStatus.closed) {
          setAvailableReportStatus([reportStatus.pending, reportStatus.accepted, reportStatus.rejected]);
        }
        if (props.data.reportStatus === reportStatus.closed) {
          setAvailableReportStatus([reportStatus.closed]);
        }

        const fields = {
            id: props.data.id,
            date: dayjs(props.data.date),
            dataCollectorId: props.data.dataCollectorId.toString(),
            reportStatus: props.data.reportStatus,
            healthRiskId: props.data.healthRiskId.toString(),
            location: reportLocation?.village + (reportLocation?.zone ? (" > " + reportLocation?.zone) : ""),
            reportSex: findSexAgeHelper(props.data)[0],
            reportAge: findSexAgeHelper(props.data)[1],
            countMalesBelowFive: props.data.countMalesBelowFive.toString(),
            countMalesAtLeastFive: props.data.countMalesAtLeastFive.toString(),
            countFemalesBelowFive: props.data.countFemalesBelowFive.toString(),
            countFemalesAtLeastFive: props.data.countFemalesAtLeastFive.toString(),
            countUnspecifiedSexAndAge: props.data.countUnspecifiedSexAndAge.toString(),
            referredCount: props.data.referredCount.toString(),
            deathCount: props.data.deathCount.toString(),
            fromOtherVillagesCount: props.data.fromOtherVillagesCount.toString()
        };

        const validation = {
            date: [validators.required],
            dataCollectorId: [validators.required],
            reportStatus: [validators.required],
            location: [validators.required],
            reportSex: [validators.required],
            reportAge: [validators.required],
            healthRiskId: [validators.required],
            countMalesBelowFive: [validators.required, validators.integer, validators.nonNegativeNumber],
            countMalesAtLeastFive: [validators.required, validators.integer, validators.nonNegativeNumber],
            countFemalesBelowFive: [validators.required, validators.integer, validators.nonNegativeNumber],
            countFemalesAtLeastFive: [validators.required, validators.integer, validators.nonNegativeNumber],
            referredCount: [validators.integer, validators.nonNegativeNumber],
            deathCount: [validators.integer, validators.nonNegativeNumber],
            fromOtherVillagesCount: [validators.integer, validators.nonNegativeNumber]
        };

        const newForm = createForm(fields, validation);


        setDataCollector(props.dataCollectors.find(dc => dc.id.toString() === newForm.fields.dataCollectorId.value));
        setReportSex(fields.reportSex);
        setReportAge(fields.reportAge);

        newForm.fields.dataCollectorId.subscribe(({ newValue }) => {
            const newDataCollector = props.dataCollectors.find(dc => dc.id.toString() === newValue.toString())
            setDataCollector(newDataCollector);
            if (newDataCollector?.locations.length === 1) {
              setLocation(newDataCollector.locations[0]);
            }
          }
        );
        newForm.fields.location.subscribe(({ newValue }) => setLocation(newValue));
        newForm.fields.reportSex.subscribe(({ newValue }) => setReportSex(newValue));
        newForm.fields.reportAge.subscribe(({ newValue }) => setReportAge(newValue));

          setForm(newForm);
    }, [props.data, props.match]);

    const handleSubmit = (e) => {
        e.preventDefault();

        if (!form.isValid()) {
            return;
        };

        const values = form.getValues();

        if (props.data.reportType === 'Single') {
          Object.keys(reportCountToSexAge).map(comb => values[comb] = "0");
          values[findSexAgeCombinationHelper()] = "1";
        }

        props.edit(props.projectId, props.reportId, {
          date: values.date.format('YYYY-MM-DDTHH:mm:ss'),
          dataCollectorId: parseInt(values.dataCollectorId),
          dataCollectorLocation: selectedLocation,
          reportStatus: values.reportStatus,
          healthRiskId: parseInt(values.healthRiskId),
          countMalesBelowFive: parseInt(values.countMalesBelowFive),
          countMalesAtLeastFive: parseInt(values.countMalesAtLeastFive),
          countFemalesBelowFive: parseInt(values.countFemalesBelowFive),
          countFemalesAtLeastFive: parseInt(values.countFemalesAtLeastFive),
          countUnspecifiedSexAndAge: parseInt(values.countUnspecifiedSexAndAge),
          referredCount: values.referredCount === "" ? null : parseInt(values.referredCount),
          deathCount: values.deathCount === "" ? null : parseInt(values.deathCount),
          fromOtherVillagesCount: values.fromOtherVillagesCount === "" ? null : parseInt(values.fromOtherVillagesCount)
        });
    };

    const findSexAgeCombinationHelper = () => {
      let selectedCombination;
      Object.keys(reportCountToSexAge).map(key => {
          if (reportCountToSexAge[key][0] === reportSex && reportCountToSexAge[key][1] === reportAge) selectedCombination = key;
        }
      )
      return selectedCombination;
    }
    const findSexAgeHelper = (data) => {
      let sexAge = [];
      Object.keys(reportCountToSexAge).map(key => {
          if(data[key] > 0) sexAge = reportCountToSexAge[key]
        }
      )
      return sexAge;
    }

    if (props.isFetching || !form || !props.data) {
        return <Loading />;
    }

    return (
        <Fragment>
            {props.error && <ValidationMessage message={props.error} />}

            <Form onSubmit={handleSubmit}>
                <Fragment>
                  <div className={styles.formSectionTitle}>{strings(stringKeys.reports.form.senderSectionTitle)}</div>
                  <Grid container spacing={2}>
                    <Grid item xs={12}>
                      <SelectField
                        label={strings(stringKeys.reports.form.dataCollector)}
                        name="dataCollectorId"
                        field={form.fields.dataCollectorId}
                        disabled={props.data.reportStatus !== "New"}
                        disabledLabel={props.data.reportStatus !== "New" ?
                          strings(stringKeys.reports.form.reportPartOfAlertLabel)
                          : ""}
                      >
                        {props.dataCollectors.map(dataCollector => (
                          <MenuItem key={`dataCollector_${dataCollector.id}`} value={dataCollector.id.toString()}>
                            {dataCollector.name}
                          </MenuItem>
                        ))}
                      </SelectField>
                    </Grid>
                  </Grid>

                  <Grid container spacing={2}>
                    <Grid item xs={12}>
                      <SelectField
                        label={strings(stringKeys.reports.form.dataCollectorLocations)}
                        name="location"
                        field={form.fields.location}
                        disabled={props.data.reportStatus !== "New" || !selectedDataCollector}
                        disabledLabel={(!selectedDataCollector) ? strings(stringKeys.reports.form.selectDcFirst) : ""}
                      >
                        { selectedDataCollector && selectedDataCollector.locations.map(location => (
                            <MenuItem key={`dataCollectorLocations_${location.villageId}_${location.zoneId}`} value={(location.village + (location.zone ? (" > " + location.zone) : ""))}>
                              {location.village + (location.zone ? (" > " + location.zone) : "")}
                            </MenuItem>
                          ))}
                      </SelectField>
                    </Grid>
                  </Grid>
                </Fragment>
                { (props.data.reportType !== "DataCollectionPoint" && props.data.reportType !== "Aggregate") && (
                  <Fragment>
                    <div className={styles.formSectionTitle}>{strings(stringKeys.reports.form.statusSectionTitle)}</div>
                    <Grid container spacing={2}>
                      <Grid item xs={12}>
                        <SelectField
                          label={strings(stringKeys.reports.form.reportStatus)}
                          name="reportStatus"
                          field={form.fields.reportStatus}
                          disabled={props.data.reportStatus === "Closed" || !selectedDataCollector || !selectedLocation}
                        >
                          {availableReportStatus.map(status => (
                            <MenuItem key={`status_${status}`} value={status.toString()}>
                              {strings(stringKeys.reports.status[status])}
                            </MenuItem>
                          ))}
                        </SelectField>
                      </Grid>
                    </Grid>
                  </Fragment>
                )}
                <div className={styles.formSectionTitle}>{strings(stringKeys.reports.form.contentSectionTitle)}</div>
                {
                  (props.data.reportType === "Single") && (
                    <Fragment>
                      <Grid container spacing={2}>
                        <Grid item xs={12}>
                          <SelectField
                            label={strings(stringKeys.reports.form.sex)}
                            name="reportSex"
                            field={form.fields.reportSex}
                          >
                            { Object.keys(reportSexes).map(sex => (
                              <MenuItem key={`sex_${reportSexes[sex]}`} value={reportSexes[sex].toString()}>
                                {reportSexes[sex]}
                              </MenuItem>
                            ))}
                          </SelectField>
                        </Grid>
                      </Grid>
                      <Grid container spacing={2}>
                        <Grid item xs={12}>
                          <SelectField
                            label={strings(stringKeys.reports.form.age)}
                            name="reportAge"
                            field={form.fields.reportAge}
                          >
                            { Object.keys(reportAges).map(age => (
                              <MenuItem key={`age_${reportAges[age]}`} value={reportAges[age].toString()}>
                                {reportAges[age]}
                              </MenuItem>
                            ))}
                          </SelectField>
                        </Grid>
                      </Grid>
                    </Fragment>
                )}
                { (props.data.reportType === "DataCollectionPoint" || props.data.reportType === "Aggregate") && (
                  <Fragment>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <DateInputField
                                className={styles.fullWidth}
                                label={strings(stringKeys.reports.form.date)}
                                name="date"
                                field={form.fields.date}
                            />
                        </Grid>
                    </Grid>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <SelectField
                                label={strings(stringKeys.reports.form.healthRisk)}
                                name="healthRiskId"
                                field={form.fields.healthRiskId}
                            >
                                {props.healthRisks.map(healthRisk => (
                                    <MenuItem key={`healthRisk_${healthRisk.id}`} value={healthRisk.id.toString()}>
                                        {healthRisk.name}
                                    </MenuItem>
                                ))}
                            </SelectField>
                        </Grid>
                    </Grid>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <TextInputField
                                label={strings(stringKeys.reports.form.malesBelowFive)}
                                name="countMalesBelowFive"
                                field={form.fields.countMalesBelowFive}
                            />
                        </Grid>
                    </Grid>

                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <TextInputField
                                label={strings(stringKeys.reports.form.malesAtLeastFive)}
                                name="countMalesAtLeastFive"
                                field={form.fields.countMalesAtLeastFive}
                            />
                        </Grid>
                    </Grid>

                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <TextInputField
                                label={strings(stringKeys.reports.form.femalesBelowFive)}
                                name="countFemalesBelowFive"
                                field={form.fields.countFemalesBelowFive}
                            />
                        </Grid>
                    </Grid>

                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <TextInputField
                                label={strings(stringKeys.reports.form.femalesAtLeastFive)}
                                name="countFemalesAtLeastFive"
                                field={form.fields.countFemalesAtLeastFive}
                            />
                        </Grid>
                    </Grid>
                  </Fragment>
                )}
                { props.data.reportType === "DataCollectionPoint" && (
                  <Fragment>
                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <TextInputField
                                label={strings(stringKeys.reports.form.referredCount)}
                                name="referredCount"
                                field={form.fields.referredCount}
                                disabled={props.data.reportType !== "DataCollectionPoint"}
                            />
                        </Grid>
                    </Grid>

                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <TextInputField
                                label={strings(stringKeys.reports.form.deathCount)}
                                name="deathCount"
                                field={form.fields.deathCount}
                                disabled={props.data.reportType !== "DataCollectionPoint"}
                            />
                        </Grid>
                    </Grid>

                    <Grid container spacing={2}>
                        <Grid item xs={12}>
                            <TextInputField
                                label={strings(stringKeys.reports.form.fromOtherVillagesCount)}
                                name="fromOtherVillagesCount"
                                field={form.fields.fromOtherVillagesCount}
                                disabled={props.data.reportType !== "DataCollectionPoint"}
                            />
                        </Grid>
                    </Grid>
                  </Fragment>
                )}
                <FormActions>
                    <Button onClick={() => props.goToList(props.projectId)}>{strings(stringKeys.form.cancel)}</Button>
                    <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.reports.form.update)}</SubmitButton>
                </FormActions>
            </Form>
        </Fragment>
    );
}

ReportsEditPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
    reportId: ownProps.match.params.reportId,
    projectId: ownProps.match.params.projectId,
    isFetching: state.reports.formFetching,
    isSaving: state.reports.formSaving,
    data: state.reports.formData,
    error: state.reports.formError,
    healthRisks: state.reports.editReport.formHealthRisks,
    dataCollectors: state.reports.editReport.formDataCollectors
});

const mapDispatchToProps = {
    openEdition: reportsActions.openEdition.invoke,
    edit: reportsActions.edit.invoke,
    goToList: reportsActions.goToList
};

export const ReportsEditPage = withLayout(
    Layout,
    connect(mapStateToProps, mapDispatchToProps)(ReportsEditPageComponent)
);
