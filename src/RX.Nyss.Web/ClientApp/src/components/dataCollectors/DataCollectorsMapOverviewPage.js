import React, { Fragment, useRef, useEffect } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as dataCollectorsActions from './logic/dataCollectorsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { useMount } from '../../utils/lifecycle';
import { DataCollectorsPerformanceMap } from './DataCollectorsPerformanceMap';

const DataCollectorsMapOverviewPageComponent = (props) => {
  useMount(() => {
    props.openDataCollectorsMapOverview(props.projectId, '2019-01-01', '2020-01-01');
  });

  if (!props.centerLocation) {
    return null;
  }

  return (
    <Fragment>
      <DataCollectorsPerformanceMap
        projectId={props.projectId}
        centerLocation={props.centerLocation}
        dataCollectorLocations={props.dataCollectorLocations}
        getMapDetails={props.getMapDetails}
        details={props.details}
        detailsFetching={props.detailsFetching}
      />
    </Fragment>
  );
}

DataCollectorsMapOverviewPageComponent.propTypes = {
  getDataCollectorsMapOverview: PropTypes.func,
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  dataCollectorLocations: state.dataCollectors.mapOverviewDataCollectorLocations,
  centerLocation: state.dataCollectors.mapOverviewCenterLocation,
  details: state.dataCollectors.mapOverviewDetails,
  detailsFetching: state.dataCollectors.mapOverviewDetailsFetching,

});

const mapDispatchToProps = {
  openDataCollectorsMapOverview: dataCollectorsActions.openMapOverview.invoke,
  getDataCollectorsMapOverview: dataCollectorsActions.getMapOverview.invoke,
  getMapDetails: dataCollectorsActions.getMapDetails.invoke
};

export const DataCollectorsMapOverviewPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsMapOverviewPageComponent)
);
