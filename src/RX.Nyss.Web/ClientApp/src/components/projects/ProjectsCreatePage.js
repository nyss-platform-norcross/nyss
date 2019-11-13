import React, { useState, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as projectsActions from './logic/projectsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import SelectInput from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import SnackbarContent from '@material-ui/core/SnackbarContent';
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import Select from 'react-select';
import { MultiSelect } from '../forms/MultiSelect';

const ProjectsCreatePageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      name: "",
      timeZone: "",
      healthRisks: []
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      timeZone: [validators.required, validators.minLength(1), validators.maxLength(50)]
    };

    return createForm(fields, validation);
  });

  useMount(() => {
    props.openCreation(props.nationalSocietyId);
  })

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.create(props.nationalSocietyId, {
      name: values.name,
      timeZone: values.timeZone,
      healthRisks: values.healthRisks
    });
  };

  return (
    <Fragment>
      <Typography variant="h2">{strings(stringKeys.project.form.creationTitle)}</Typography>

      {props.error &&
        <SnackbarContent
          message={props.error}
        />
      }

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.project.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.project.form.timeZone)}
              name="timeZone"
              field={form.fields.timeZone}
            />
          </Grid>

          <Grid item xs={12}>
            <MultiSelect options={[ {label: "Alan", value: "1"}, {label: "Ala", value: "2"} ]} label={"Health risks"} />
          </Grid>

          {props.healthRisks.map(projectHealthRisk => (
            <Fragment key={`projectHealthRisk${projectHealthRisk.id}`}>
              <Grid item xs={12}>
                <Typography variant="h3">{strings(stringKeys.healthRisk.form.translationsSetion).replace("{language}", projectHealthRisk.name)}</Typography>

                <Grid container spacing={3}>
                  <Grid item xs={12}>
                    <TextInputField
                      label={strings(stringKeys.healthRisk.form.contentLanguageName)}
                      name={`contentLanguage_${projectHealthRisk.id}_name`}
                      field={form.fields[`contentLanguage_${projectHealthRisk.id}_name`]}
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <TextInputField
                      label={strings(stringKeys.healthRisk.form.contentLanguageCaseDefinition)}
                      name={`contentLanguage_${projectHealthRisk.id}_caseDefinition`}
                      field={form.fields[`contentLanguage_${projectHealthRisk.id}_caseDefinition`]}
                      multiline
                    />
                  </Grid>
                  <Grid item xs={6}>
                    <TextInputField
                      label={strings(stringKeys.healthRisk.form.contentLanguageFeedbackMessage)}
                      name={`contentLanguage_${projectHealthRisk.id}_feedbackMessage`}
                      field={form.fields[`contentLanguage_${projectHealthRisk.id}_feedbackMessage`]}
                      multiline
                    />
                  </Grid>
                </Grid>

                <Grid item xs={12}>
                  <Typography variant="h3">{strings(stringKeys.healthRisk.form.alertsSetion)}</Typography>
                  <Typography variant="subtitle1">{strings(stringKeys.healthRisk.form.alertRuleDescription)}</Typography>
                </Grid>

                <Grid item xs={4}>
                  <TextInputField
                    label={strings(stringKeys.healthRisk.form.alertRuleCountThreshold)}
                    name="alertRuleCountThreshold"
                    field={form.fields.alertRuleCountThreshold}
                  />
                </Grid>

                <Grid item xs={4}>
                  <TextInputField
                    label={strings(stringKeys.healthRisk.form.alertRuleDaysThreshold)}
                    name="alertRuleDaysThreshold"
                    field={form.fields.alertRuleDaysThreshold}
                  />
                </Grid>

                <Grid item xs={4}>
                  <TextInputField
                    label={strings(stringKeys.healthRisk.form.alertRuleKilometersThreshold)}
                    name="alertRuleKilometersThreshold"
                    field={form.fields.alertRuleKilometersThreshold}
                  />
                </Grid>
              </Grid>
            </Fragment>
          ))}
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList(props.nationalSocietyId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.project.form.create)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

ProjectsCreatePageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  healthRisks: [],
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isSaving: state.projects.formSaving,
  error: state.projects.formError
});

const mapDispatchToProps = {
  openCreation: projectsActions.openCreation.invoke,
  create: projectsActions.create.invoke,
  goToList: projectsActions.goToList
};

export const ProjectsCreatePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsCreatePageComponent)
);
