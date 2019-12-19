import React, { useEffect, Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as nationalSocietiesActions from './logic/nationalSocietiesActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { Loading } from '../common/loading/Loading';

const NationalSocietiesDashboardPageComponent = ({ openDashbaord, isFetching, match, name }) => {
  useEffect(() => {
    openDashbaord(match.path, match.params);
  }, [openDashbaord, match])

  if (isFetching) {
    return <Loading />;
  }

  return (
    <Fragment>
    </Fragment>
  );
}

NationalSocietiesDashboardPageComponent.propTypes = {
  openDashbaord: PropTypes.func,
  name: PropTypes.string
};

const mapStateToProps = state => ({
  name: state.nationalSocieties.dashboard.name,
  isFetching: state.nationalSocieties.dashboard.isFetching
});

const mapDispatchToProps = {
  openDashbaord: nationalSocietiesActions.openDashbaord.invoke
};

export const NationalSocietiesDashboardPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietiesDashboardPageComponent)
);
