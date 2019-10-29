import React, { useEffect, Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import * as nationalSocietiesActions from './logic/nationalSocietiesActions';
import Layout from '../layout/Layout';
import Typography from '@material-ui/core/Typography';
import { Loading } from '../common/loading/Loading';
import TextField from '@material-ui/core/TextField';
import Button from '@material-ui/core/Button';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import ReadOnlyField from '../forms/ReadOnlyField';

const NationalSocietiesOverviewPageComponent = (props) => {
  useEffect(() => {
    props.openOverview(props.match);
  }, []);

  if (props.isFetching || !props.data) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Typography variant="h2">Overview</Typography>


      <Form>
        <ReadOnlyField
          label={"National Society name"}
          value={props.data.name}
        />

        <ReadOnlyField
          label={"Country"}
          value={props.data.countryName}
        />

        <ReadOnlyField
          label={"Content language"}
          value={props.data.contentLanguageName}
        />

        <FormActions>
          <Button variant="outlined" color="primary" onClick={() => props.openEdition(props.data.id)}>Edit National Society</Button>
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
  data: state.nationalSocieties.overviewData
});

const mapDispatchToProps = {
  openOverview: nationalSocietiesActions.openOverview.invoke,
  openEdition: nationalSocietiesActions.goToEdition
};

export const NationalSocietiesOverviewPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietiesOverviewPageComponent)
);
