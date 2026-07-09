import React, { useState, Fragment, useEffect, createRef } from "react";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as projectsActions from "./logic/projectsActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { Grid, Typography } from "@material-ui/core";
import { MultiSelect } from "../forms/MultiSelect";
import { ProjectsHealthRiskItem } from "./ProjectHealthRiskItem";
import { getSaveFormModel } from "./logic/projectsService";
import { Loading } from "../common/loading/Loading";
import { ValidationMessage } from "../forms/ValidationMessage";
import { SubMenuTitle } from "../layout/SubMenuTitle";
import { trackPageView } from "../../utils/appInsightsHelper";

const ProjectHealthRisksEditPageComponent = (props) => {
  const [healthRiskDataSource, setHealthRiskDataSource] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);
  const [healthRisksFieldTouched, setHealthRisksFieldTouched] = useState(false);

  useMount(() => {
    props.openHealthRisksEdition(props.nationalSocietyId, props.projectId);

    // Track page view
    trackPageView("ProjectHealthRisksEditPage");
  });

  useEffect(() => {
    setHealthRiskDataSource(
      props.healthRisks.map((hr) => ({
        label: hr.healthRiskName,
        value: hr.healthRiskId,
        data: hr,
      })),
    );
  }, [props.healthRisks]);

  const [form, setForm] = useState(null);

  useEffect(() => {
    if (!props.data) {
      return;
    }

    let validation = {
      name: [
        validators.required,
        validators.minLength(1),
        validators.maxLength(100),
      ],
    };

    const refs = {
      name: createRef(),
    };

    setForm(createForm(null, validation, refs));
    setSelectedHealthRisks(props.data.projectHealthRisks);
    return () => setForm(null);
  }, [props.data]);

  const handleSubmit = (e) => {
    e.preventDefault();

    const preventSubmit = selectedHealthRisks.length === 1;

    if (preventSubmit) {
      setHealthRisksFieldTouched(true);
    }

    if (!form.isValid()) {
      Object.values(form.fields)
        .filter((e) => e.error)[0]
        .scrollTo();
      return;
    }

    !preventSubmit &&
      props.editHealthRisk(
        props.nationalSocietyId,
        props.projectId,
        getSaveFormModel(form.getValues(), selectedHealthRisks),
      );
  };

  const onHealthRiskChange = (value, eventData) => {
    if (eventData.action === "select-option") {
      setSelectedHealthRisks([...selectedHealthRisks, eventData.option.data]);
    } else if (
      (eventData.action === "remove-value" ||
        eventData.action === "pop-value") &&
      eventData.removedValue.data.healthRiskType !== "Activity"
    ) {
      setSelectedHealthRisks(
        selectedHealthRisks.filter(
          (hr) => hr.healthRiskId !== eventData.removedValue.value,
        ),
      );
    } else if (eventData.action === "clear") {
      setSelectedHealthRisks(
        selectedHealthRisks.filter((hr) => hr.healthRiskType === "Activity"),
      );
    }
  };

  const getSelectedHealthRiskValue = () =>
    healthRiskDataSource
      .filter((hr) =>
        selectedHealthRisks.some((shr) => shr.healthRiskId === hr.value),
      )
      .sort((a, b) => (a.data.healthRiskType === "Activity" ? -1 : 1));

  useCustomErrors(form, props.error);

  if (props.isFetching || !form) {
    return <Loading />;
  }

  return (
    <Fragment>
      <SubMenuTitle />
      {props.error && <ValidationMessage message={props.error.message} />}
      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <MultiSelect
              options={healthRiskDataSource}
              value={getSelectedHealthRiskValue()}
              onChange={onHealthRiskChange}
              isClearable={false}
              error={
                healthRisksFieldTouched && selectedHealthRisks.length < 2
                  ? `${strings(stringKeys.validation.noHealthRiskSelected)}`
                  : null
              }
            />
          </Grid>

          {selectedHealthRisks.length > 0 && (
            <Grid item xs={12}>
              <Typography variant="h3">
                {strings(stringKeys.project.form.healthRisksSection)}
              </Typography>
            </Grid>
          )}

          {selectedHealthRisks
            .sort((a, b) => a.healthRiskCode - b.healthRiskCode)
            .map((selectedHealthRisk) => (
              <Grid
                item
                xs={12}
                key={`projectsHealthRiskItem_${selectedHealthRisk.healthRiskId}`}
              >
                <ProjectsHealthRiskItem
                  form={form}
                  projectHealthRisk={{ id: selectedHealthRisk.id }}
                  healthRisk={selectedHealthRisk}
                />
              </Grid>
            ))}
        </Grid>

        <FormActions>
          <CancelButton
            onClick={() =>
              props.goToHealthRisks(props.nationalSocietyId, props.projectId)
            }
          >
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton isFetching={props.isSaving}>
            {strings(stringKeys.common.buttons.update)}
          </SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
};

ProjectHealthRisksEditPageComponent.propTypes = {};

const mapStateToProps = (state, ownProps) => ({
  healthRisks: state.projects.formHealthRisks,
  projectId: ownProps.match.params.projectId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.projects.formFetching,
  isSaving: state.projects.formSaving,
  data: state.projects.formData,
  error: state.projects.formError,
});

const mapDispatchToProps = {
  openHealthRisksEdition: projectsActions.openHealthRisksEdition.invoke,
  editHealthRisk: projectsActions.editHealthRisks.invoke,
  goToHealthRisks: projectsActions.goToHealthRisks,
};

export const ProjectHealthRisksEditPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(ProjectHealthRisksEditPageComponent),
);
