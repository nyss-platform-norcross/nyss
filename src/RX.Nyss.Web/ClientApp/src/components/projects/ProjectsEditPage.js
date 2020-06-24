import React, { useState, Fragment, useEffect } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as projectsActions from './logic/projectsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import { MultiSelect } from '../forms/MultiSelect';
import { ProjectsHealthRiskItem } from './ProjectHealthRiskItem';
import { getSaveFormModel } from './logic/projectsService';
import { Loading } from '../common/loading/Loading';
import SelectField from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import { ValidationMessage } from '../forms/ValidationMessage';
import CheckboxField from '../forms/CheckboxField';

const ProjectsEditPageComponent = (props) => {
  const [healthRiskDataSource, setHealthRiskDataSource] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);

  useMount(() => {
    props.openEdition(props.nationalSocietyId, props.projectId);
  })

  useEffect(() => {
    setHealthRiskDataSource(props.healthRisks.map(hr => ({ label: hr.healthRiskName, value: hr.healthRiskId, data: hr })));
  }, [props.healthRisks])

  const [form, setForm] = useState(null);

  useEffect(() => {
    if (!props.data) {
      return;
    }

    let fields = {
      name: props.data.name,
      allowMultipleOrganizations: props.data.allowMultipleOrganizations,
      timeZoneId: props.data.timeZoneId
    };

    let validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      timeZoneId: [validators.required, validators.minLength(1), validators.maxLength(50)]
    };

    setForm(createForm(fields, validation));
    setSelectedHealthRisks(props.data.projectHealthRisks);
    return () => setForm(null);
  }, [props.data]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (selectedHealthRisks.length === 0) {
      return;
    }

    if (!form.isValid()) {
      return;
    };

    props.edit(props.nationalSocietyId, props.projectId, getSaveFormModel(form.getValues(), selectedHealthRisks));
  };

  const onHealthRiskChange = (value, eventData) => {
    if (eventData.action === "select-option") {
      setSelectedHealthRisks([...selectedHealthRisks, eventData.option.data]);
    } else if (eventData.action === "remove-value" || eventData.action === "pop-value") {
      setSelectedHealthRisks(selectedHealthRisks.filter(hr => hr.healthRiskId !== eventData.removedValue.value));
    } else if (eventData.action === "clear") {
      setSelectedHealthRisks([]);
    }
  }

  useCustomErrors(form, props.error);

  if (props.isFetching || !form) {
    return <Loading />;
  }

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={3}>
          <Grid item xs={12} sm={9}>
            <TextInputField
              label={strings(stringKeys.project.form.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          <Grid item xs={12}>
            <SelectField
              label={strings(stringKeys.project.form.timeZone)}
              field={form.fields.timeZoneId}
              name="timeZoneId"
            >
              {props.timeZones.map(timeZone => (
                <MenuItem key={timeZone.id} value={timeZone.id}>
                  {timeZone.displayName}
                </MenuItem>
              ))}
            </SelectField>
          </Grid>

          <Grid item xs={12} sm={9}>
            <CheckboxField
              label={strings(stringKeys.project.form.allowMultipleOrganizations)}
              name="allowMultipleOrganizations"
              field={form.fields.allowMultipleOrganizations}
            />
          </Grid>

          <Grid item xs={12}>
            <MultiSelect
              label={strings(stringKeys.project.form.healthRisks)}
              options={healthRiskDataSource}
              defaultValue={healthRiskDataSource.filter(hr => (selectedHealthRisks.some(shr => shr.healthRiskId === hr.value)))}
              onChange={onHealthRiskChange}
              error={selectedHealthRisks.length === 0 ? `${strings(stringKeys.validation.fieldRequired)}` : null}
            />
          </Grid>

          {selectedHealthRisks.length > 0 &&
            <Grid item xs={12}>
              <Typography variant="h3">{strings(stringKeys.project.form.healthRisksSetion)}</Typography>
            </Grid>
          }

          {selectedHealthRisks.sort((a, b) => a.healthRiskCode - b.healthRiskCode).map(selectedHealthRisk => (
            <Grid item xs={12} key={`projectsHealthRiskItem_${selectedHealthRisk.healthRiskId}`}>
              <ProjectsHealthRiskItem
                form={form}
                projectHealthRisk={{ id: selectedHealthRisk.id }}
                healthRisk={selectedHealthRisk}
              />
            </Grid>
          ))}
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToOverview(props.nationalSocietyId, props.projectId)}>{strings(stringKeys.form.cancel)}</Button>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.project.form.update)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

ProjectsEditPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  healthRisks: state.projects.formHealthRisks,
  timeZones: state.projects.formTimeZones,
  projectId: ownProps.match.params.projectId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.projects.formFetching,
  isSaving: state.projects.formSaving,
  data: state.projects.formData,
  error: state.projects.formError
});

const mapDispatchToProps = {
  openEdition: projectsActions.openEdition.invoke,
  edit: projectsActions.edit.invoke,
  goToOverview: projectsActions.goToOverview
};

export const ProjectsEditPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsEditPageComponent)
);
