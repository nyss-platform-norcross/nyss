import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { withLayout } from '../../utils/layout';
import * as nationalSocietiesActions from './logic/nationalSocietiesActions';
import Layout from '../layout/Layout';
import { Typography, Grid } from '@material-ui/core';
import { Loading } from '../common/loading/Loading';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import { useMount } from '../../utils/lifecycle';
import { strings, stringKeys } from '../../strings';
import { TableActionsButton } from '../common/tableActions/TableActionsButton';
import { accessMap } from '../../authentication/accessMap';
import * as roles from '../../authentication/roles';

const NationalSocietiesOverviewPageComponent = (props) => {
  useMount(() => {
    props.openOverview(props.match);
  });

  if (props.isFetching || !props.data) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Form>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.nationalSociety.form.name)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.name}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.nationalSociety.form.country)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.countryName}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.nationalSociety.form.contentLanguage)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.contentLanguageName}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h6">
              {strings(stringKeys.nationalSociety.form.epiWeekStandard.title)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {strings(`nationalSociety.form.epiWeekStandard.${props.data.epiWeekStartDay}`)}
            </Typography>
          </Grid>          
        </Grid>

        <FormActions>
          <TableActionsButton
            variant="outlined"
            color="primary"
            onClick={() => props.openEdition(props.data.id)}
            roles={accessMap.nationalSocieties.edit}
            condition={!props.nationalSocietyIsArchived && (!props.data.hasCoordinator || props.callingUserRoles.some(r => r === roles.Coordinator || r === roles.Administrator))}
          >
            {strings(stringKeys.nationalSociety.edit)}
          </TableActionsButton>
        </FormActions>
      </Form>

    </Fragment >
  );
}

NationalSocietiesOverviewPageComponent.propTypes = {
  getNationalSocieties: PropTypes.func,
  list: PropTypes.array
};

const mapStateToProps = state => ({
  isFetching: state.nationalSocieties.overviewFetching,
  data: state.nationalSocieties.overviewData,
  nationalSocietyIsArchived: state.appData.siteMap.parameters.nationalSocietyIsArchived,
  nationalSocietyHasCoordinator: state.appData.siteMap.parameters.nationalSocietyHasCoordinator,
  callingUserRoles: state.appData.user.roles
});

const mapDispatchToProps = {
  openOverview: nationalSocietiesActions.openOverview.invoke,
  openEdition: nationalSocietiesActions.goToEdition
};

export const NationalSocietiesOverviewPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietiesOverviewPageComponent)
);
