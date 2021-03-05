import React, { useState, Fragment, useEffect, useMemo, useCallback } from 'react';
import { connect } from "react-redux";
import { withLayout } from '../../utils/layout';
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
import SelectField from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import { ValidationMessage } from '../forms/ValidationMessage';
import CheckboxField from '../forms/CheckboxField';
import * as roles from '../../authentication/roles';

const ProjectsCreatePageComponent = (props) => {
  const [healthRiskDataSource, setHealthRiskDataSource] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);
  const [healthRisksFieldTouched, setHealthRisksFieldTouched] = useState(false);

  useEffect(() => {
    props.data && setHealthRiskDataSource(props.data.healthRisks.map(hr => ({ label: hr.healthRiskName, value: hr.healthRiskId, data: hr })));
  }, [props.data])

  const canChangeOrganization = useCallback(
    () => props.callingUserRoles.some(r => r === roles.Administrator || r === roles.Coordinator),
    [props.callingUserRoles]
  );

  const form = useMemo(() => {
    const fields = {
      name: "",
      allowMultipleOrganizations: false,
      organizationId: ""
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      organizationId: [validators.requiredWhen(f => canChangeOrganization())]
    };

    return createForm(fields, validation);
  }, [canChangeOrganization]);

  useCustomErrors(form, props.error);

  useMount(() => {
    props.openCreation(props.nationalSocietyId);
  })

  const handleSubmit = (e) => {
    e.preventDefault();

    if (selectedHealthRisks.length === 0) {
      return;
    }

    if (!form.isValid()) {
      return;
    };

    props.create(props.nationalSocietyId, getSaveFormModel(form.getValues(), selectedHealthRisks));
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

  if (!props.data) {
    return null;
  }

  return (
    <Fragment>
      {props.error && <ValidationMessage message={props.error.message} />}

      <Form onSubmit={handleSubmit} fullWidth style={{ maxWidth: 800 }}>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={9}>
            <TextInputField
              label={strings(stringKeys.project.form.name)}
              name="name"
              field={form.fields.name}
              autoFocus
            />
          </Grid>

          {canChangeOrganization() && (
            <Fragment>
              <Grid item xs={12} sm={9}>
                <CheckboxField
                  label={strings(stringKeys.project.form.allowMultipleOrganizations)}
                  name="allowMultipleOrganizations"
                  field={form.fields.allowMultipleOrganizations}
                />
              </Grid>

              <Grid item xs={12}>
                <SelectField
                  label={strings(stringKeys.project.form.organization)}
                  field={form.fields.organizationId}
                  name="organizationId"
                >
                  {props.data.organizations.map(organization => (
                    <MenuItem key={`organization_${organization.id}`} value={organization.id.toString()}>
                      {organization.name}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>
            </Fragment>
          )}

          <Grid item xs={12}>
            <MultiSelect
              label={strings(stringKeys.project.form.healthRisks)}
              options={healthRiskDataSource}
              onChange={onHealthRiskChange}
              onBlur={e => setHealthRisksFieldTouched(true)}
              error={(healthRisksFieldTouched && selectedHealthRisks.length === 0) ? `${strings(stringKeys.validation.fieldRequired)}` : null}
            />
          </Grid>

          {selectedHealthRisks.length > 0 &&
            <Grid item xs={12}>
              <Typography variant="h3">{strings(stringKeys.project.form.healthRisksSection)}</Typography>
            </Grid>
          }

          {selectedHealthRisks.map(selectedHealthRisk => (
            <ProjectsHealthRiskItem
              key={`projectsHealthRiskItem_${selectedHealthRisk.healthRiskId}`}
              form={form}
              projectHealthRisk={{ id: null }}
              healthRisk={selectedHealthRisk}
            />
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
  data: state.projects.formData,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isSaving: state.projects.formSaving,
  error: state.projects.formError,
  callingUserRoles: state.appData.user.roles
});

const mapDispatchToProps = {
  openCreation: projectsActions.openCreation.invoke,
  create: projectsActions.create.invoke,
  goToList: projectsActions.goToList
};

export const ProjectsCreatePage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsCreatePageComponent)
);
