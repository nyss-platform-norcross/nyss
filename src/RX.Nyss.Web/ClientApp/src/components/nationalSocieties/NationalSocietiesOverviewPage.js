import React, { Fragment } from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import * as nationalSocietiesActions from "./logic/nationalSocietiesActions";
import Layout from "../layout/Layout";
import { Typography, Grid } from "@material-ui/core";
import { Loading } from "../common/loading/Loading";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import { TableActionsButton } from "../common/buttons/tableActionsButton/TableActionsButton";
import { accessMap } from "../../authentication/accessMap";
import * as roles from "../../authentication/roles";
import { trackPageView } from "../../utils/appInsightsHelper";

const NationalSocietiesOverviewPageComponent = (props) => {
  useMount(() => {
    props.openOverview(props.match);

    // Track page view
    trackPageView("NationalSocietiesOverviewPage");
  });

  if (props.isFetching || !props.data) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Form>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <Typography variant="h5">
              {strings(stringKeys.nationalSociety.form.name)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.name}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h5">
              {strings(stringKeys.nationalSociety.form.country)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.countryName}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h5">
              {strings(stringKeys.nationalSociety.form.contentLanguage)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.contentLanguageName}
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h5">
              {strings(stringKeys.nationalSociety.form.epiWeekStandard.title)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {strings(
                stringKeys.nationalSociety.form.epiWeekStandard[
                  props.data.epiWeekStartDay
                ].label,
              )}
              <br />
              <Typography variant="body2">
                (
                {strings(
                  stringKeys.nationalSociety.form.epiWeekStandard[
                    props.data.epiWeekStartDay
                  ].description,
                )}
                )
              </Typography>
            </Typography>
          </Grid>

          <Grid item xs={12}>
            <Typography variant="h5">
              {strings(stringKeys.nationalSociety.form.enableEidsrIntegration)}
            </Typography>
            <Typography variant="body1" gutterBottom>
              {props.data.enableEidsrIntegration
                ? strings(stringKeys.common.boolean.true)
                : strings(stringKeys.common.boolean.false)}
            </Typography>
          </Grid>
        </Grid>

        <FormActions>
          <TableActionsButton
            onClick={() => props.openEdition(props.data.id)}
            roles={accessMap.nationalSocieties.edit}
            condition={
              !props.nationalSocietyIsArchived &&
              (!props.data.hasCoordinator ||
                props.callingUserRoles.some(
                  (r) => r === roles.Coordinator || r === roles.Administrator,
                ))
            }
            variant={"contained"}
          >
            {strings(stringKeys.common.buttons.edit)}
          </TableActionsButton>
        </FormActions>
      </Form>
    </Fragment>
  );
};

NationalSocietiesOverviewPageComponent.propTypes = {
  getNationalSocieties: PropTypes.func,
  list: PropTypes.array,
};

const mapStateToProps = (state) => ({
  isFetching: state.nationalSocieties.overviewFetching,
  data: state.nationalSocieties.overviewData,
  nationalSocietyIsArchived:
    state.appData.siteMap.parameters.nationalSocietyIsArchived,
  nationalSocietyHasCoordinator:
    state.appData.siteMap.parameters.nationalSocietyHasCoordinator,
  callingUserRoles: state.appData.user.roles,
});

const mapDispatchToProps = {
  openOverview: nationalSocietiesActions.openOverview.invoke,
  openEdition: nationalSocietiesActions.goToEdition,
};

export const NationalSocietiesOverviewPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(NationalSocietiesOverviewPageComponent),
);
