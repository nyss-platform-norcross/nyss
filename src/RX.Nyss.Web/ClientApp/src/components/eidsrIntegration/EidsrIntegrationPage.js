import React, {Fragment} from 'react';
import {connect} from "react-redux";
import * as eidsrIntegrationActions from './logic/eidsrIntegrationActions';
import {withLayout} from '../../utils/layout';
import Layout from '../layout/Layout';
import {useMount} from '../../utils/lifecycle';
import {stringKeys, strings} from '../../strings';
import {accessMap} from '../../authentication/accessMap';
import FormActions from "../forms/formActions/FormActions";
import {TableActionsButton} from "../common/buttons/tableActionsButton/TableActionsButton";
import Form from "../forms/form/Form";
import {Grid, Typography} from "@material-ui/core";
import styles from "../common/filters/LocationFilter.module.scss";
import {Loading} from "../common/loading/Loading";
import { EidsrIntegrationNotEnabled } from "./EidsrIntegrationNotEnabled";

const EidsrIntegrationPageComponent = (props) => {
  useMount(() => {
    props.getEidsrIntegration(props.nationalSocietyId);
  });

  if (props.isFetching || !props.data) {
    return <Loading />;
  }

  if(!props.isEnabled){
    return <EidsrIntegrationNotEnabled/>;
  }

  return (
    <Fragment>

      <Form>
        <Grid container spacing={2}>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.userName)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              { props.data.username ?? strings(stringKeys.eidsrIntegration.form.dataNotSet) }
            </Typography>
          </Grid>

          {/* TODO: make it look like a password */}
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.password)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              { props.data.password ?? strings(stringKeys.eidsrIntegration.form.dataNotSet) }
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.apiBaseUrl)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              { props.data.apiBaseUrl ?? strings(stringKeys.eidsrIntegration.form.dataNotSet) }
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.trackerProgramId)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              { props.data.trackerProgramId ?? strings(stringKeys.eidsrIntegration.form.dataNotSet) }
            </Typography>
          </Grid>

          {/* TODO: style this to match mockups */}
          <Grid item xs={12}>
            <hr className={styles.divider} />
            <Typography variant="h5">
              {strings(stringKeys.eidsrIntegration.form.dataElements)}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.locationDataElementId)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              { props.data.locationDataElementId ?? strings(stringKeys.eidsrIntegration.form.dataNotSet) }
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.dateOfOnsetDataElementId)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              { props.data.dateOfOnsetDataElementId ?? strings(stringKeys.eidsrIntegration.form.dataNotSet) }
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.phoneNumberDataElementId)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              { props.data.phoneNumberDataElementId ?? strings(stringKeys.eidsrIntegration.form.dataNotSet) }
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.suspectedDiseaseDataElementId)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              { props.data.suspectedDiseaseDataElementId ?? strings(stringKeys.eidsrIntegration.form.dataNotSet) }
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.eventTypeDataElementId)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              { props.data.eventTypeDataElementId ?? strings(stringKeys.eidsrIntegration.form.dataNotSet) }
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.eidsrIntegration.form.genderDataElementId)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              { props.data.genderDataElementId ?? strings(stringKeys.eidsrIntegration.form.dataNotSet) }
            </Typography>
          </Grid>

        </Grid>

        <FormActions>
          <TableActionsButton
            onClick={() => props.goToEidsrIntegrationEdition(props.nationalSocietyId)}
            roles={accessMap.eidsrIntegration.edit}
            variant={"contained"}
          >
            {strings(stringKeys.common.buttons.edit)}
          </TableActionsButton>
        </FormActions>

      </Form>

    </Fragment>
  );
}

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,

  data: state.eidsrIntegration.data,
  isFetching: state.eidsrIntegration.isFetching,

  isEnabled: state.appData.siteMap.parameters.nationalSocietyEnableEidsrIntegration,
  callingUserRoles: state.appData.user.roles,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived,
  nationalSocietyHasCoordinator: state.appData.siteMap.parameters.nationalSocietyHasCoordinator
});

const mapDispatchToProps = {
  getEidsrIntegration: eidsrIntegrationActions.get.invoke,
  goToEidsrIntegrationEdition: eidsrIntegrationActions.goToEidsrIntegrationEdition,
};

export const EidsrIntegrationPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(EidsrIntegrationPageComponent)
);
