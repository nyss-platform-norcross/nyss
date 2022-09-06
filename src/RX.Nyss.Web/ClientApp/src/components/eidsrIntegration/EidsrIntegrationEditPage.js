import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as eidsrIntegrationActions from './logic/eidsrIntegrationActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import TableActions from '../common/tableActions/TableActions';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { accessMap } from '../../authentication/accessMap';
import * as roles from '../../authentication/roles';
import FormActions from "../forms/formActions/FormActions";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import Form from "../forms/form/Form";
import {Loading} from "../common/loading/Loading";

const EidsrIntegrationEditPageComponent = (props) => {
  useMount(() => {
    props.getEidsrIntegration(props.nationalSocietyId);
  });

  if (props.isFetching) {
    return <Loading />;
  }

  return (
    <Fragment>
      <p>edition: integration Id from the redux store:</p>
      {props.eidsrIntegration.id}

      <Form>
        <FormActions>
          <CancelButton onClick={() => props.goToEidsrIntegration(props.nationalSocietyId)}>{strings(stringKeys.form.cancel)}</CancelButton>
          <SubmitButton isFetching={props.isSaving}>{strings(stringKeys.common.buttons.update)}</SubmitButton>
        </FormActions>
      </Form>

    </Fragment>
  );
}

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,

  eidsrIntegration: state.eidsrIntegration.data,
  isFetching: state.eidsrIntegration.isFetching,

  callingUserRoles: state.appData.user.roles,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived,
  nationalSocietyHasCoordinator: state.appData.siteMap.parameters.nationalSocietyHasCoordinator
});

const mapDispatchToProps = {
  getEidsrIntegration: eidsrIntegrationActions.get.invoke,
  goToEidsrIntegration: eidsrIntegrationActions.goToEidsrIntegration,
};

export const EidsrIntegrationEditPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(EidsrIntegrationEditPageComponent)
);
