import React, {useState, Fragment, useEffect, createRef} from 'react';
import { connect } from "react-redux";
import { withLayout } from '../../utils/layout';
import { validators, createForm, useCustomErrors } from '../../utils/forms';
import * as projectsActions from './logic/projectsActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../common/buttons/submitButton/SubmitButton';
import CancelButton from '../common/buttons/cancelButton/CancelButton';
import TextInputField from '../forms/TextInputField';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { Grid, Typography } from '@material-ui/core';
import { MultiSelect } from '../forms/MultiSelect';
import { ProjectsHealthRiskItem } from './ProjectHealthRiskItem';
import { getSaveFormModel } from './logic/projectsService';
import { Loading } from '../common/loading/Loading';
import { ValidationMessage } from '../forms/ValidationMessage';
import CheckboxField from '../forms/CheckboxField';


const ProjectsEditPageComponent = (props) => {
  const [healthRiskDataSource, setHealthRiskDataSource] = useState([]);
  const [selectedHealthRisks, setSelectedHealthRisks] = useState([]);
  const [healthRisksFieldTouched, setHealthRisksFieldTouched] = useState(false);


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
      allowMultipleOrganizations: props.data.allowMultipleOrganizations
    };

    let validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)]
    };

    const refs = {
      name: createRef()
    }

    setForm(createForm(fields, validation, refs));
    setSelectedHealthRisks(props.data.projectHealthRisks);
    return () => setForm(null);
  }, [props.data]);

  const handleSubmit = (e) => {
    e.preventDefault();

    const preventSubmit = selectedHealthRisks.length === 1

    if (preventSubmit) {
      setHealthRisksFieldTouched(true)
    }

    if (!form.isValid()) {

      Object.values(form.fields).filter(e => e.error)[0].scrollTo();
      return;
    }

    !preventSubmit && props.edit(props.nationalSocietyId, props.projectId, getSaveFormModel(form.getValues(), selectedHealthRisks));
  };

  const onHealthRiskChange = (value, eventData) => {
    if (eventData.action === "select-option") {
      setSelectedHealthRisks([...selectedHealthRisks, eventData.option.data]);
    } else if (eventData.action === "remove-value" || eventData.action === "pop-value") {
        setSelectedHealthRisks(selectedHealthRisks.filter(hr => hr.healthRiskId !== eventData.removedValue.value));
    } else if (eventData.action === "clear") {
      setSelectedHealthRisks(props.data.healthRisks.filter(hr => hr.healthRiskType === 'Activity' ));
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
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.project.form.name)}
              name="name"
              field={form.fields.name}
              fieldRef={form.fields.name.ref}
            />
          </Grid>

          <Grid item xs={12}>
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
              value={healthRiskDataSource.filter(hr => (selectedHealthRisks.some(shr => shr.healthRiskId === hr.value)))}
              onChange={onHealthRiskChange}
              error={(healthRisksFieldTouched && selectedHealthRisks.length < 2) ? `${strings(stringKeys.validation.noHealthRiskSelected)}` : null}
            />
          </Grid>

          {selectedHealthRisks.length > 0 &&
            <Grid item xs={12}>
              <Typography variant="h3">{strings(stringKeys.project.form.healthRisksSection)}</Typography>
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
          <CancelButton
            onClick={() => props.goToOverview(props.nationalSocietyId, props.projectId)}
          >
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.common.buttons.update)}</SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

ProjectsEditPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  healthRisks: state.projects.formHealthRisks,
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

export const ProjectsEditPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsEditPageComponent)
);
