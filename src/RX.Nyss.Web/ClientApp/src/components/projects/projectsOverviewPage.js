import React, { useState, Fragment, useEffect } from 'react';
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
import Button from "@material-ui/core/Button";
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import Grid from '@material-ui/core/Grid';
import AddIcon from '@material-ui/icons/Add';
import { MultiSelect } from '../forms/MultiSelect';
import { ProjectsHealthRiskItem } from './ProjectHealthRiskItem';
import { ProjectEmailNotificationItem } from './ProjectEmailNotificationItem';
import { ProjectSmsNotificationItem } from './ProjectSmsNotificationItem';
import { getSaveFormModel } from './logic/projectsService';
import { Loading } from '../common/loading/Loading';
import SelectField from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import { ValidationMessage } from '../forms/ValidationMessage';

const ProjectsOverviewPageComponent = (props) => {
  useMount(() => {
    props.openOverview(props.nationalSocietyId, props.projectId);
  });

  if (props.isFetching) {
    return <Loading />;
  }

  return (
    <Fragment>
        
    </Fragment>
  );
}

ProjectsOverviewPageComponent.propTypes = {
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
  openOverview: projectsActions.openOverview.invoke,
  goToList: projectsActions.goToList
};

export const ProjectsOverviewPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(ProjectsOverviewPageComponent)
);
