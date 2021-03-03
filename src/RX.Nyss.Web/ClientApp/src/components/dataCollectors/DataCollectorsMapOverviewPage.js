import React, { Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as dataCollectorsActions from './logic/dataCollectorsActions';
import { withLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { useMount } from '../../utils/lifecycle';
import { DataCollectorsPerformanceMap } from './DataCollectorsPerformanceMap';
import { DataCollectorsPerformanceMapFilters } from './DataCollectorsPerformanceMapFilters';
import { DataCollectorsPerformanceMapLegend } from './DataCollectorsPerformanceMapLegend';

const DataCollectorsMapOverviewPageComponent = (props) => {
  useMount(() => {
    props.openDataCollectorsMapOverview(props.projectId);
  });

  const handleFiltersChange = (value) => {
    props.getDataCollectorsMapOverview(props.projectId, value)
  };

  if (!props.filters) {
    return null;
  }

  return (
    <Fragment>
      <DataCollectorsPerformanceMapFilters
        onChange={handleFiltersChange}
        filters={props.filters}
      />
      <DataCollectorsPerformanceMap
        projectId={props.projectId}
        centerLocation={props.centerLocation}
        dataCollectorLocations={props.dataCollectorLocations}
        getMapDetails={props.getMapDetails}
        details={props.details}
        detailsFetching={props.detailsFetching}
      />
      <DataCollectorsPerformanceMapLegend />
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
  filters: state.dataCollectors.mapOverviewFilters,
});

const mapDispatchToProps = {
  openDataCollectorsMapOverview: dataCollectorsActions.openMapOverview.invoke,
  getDataCollectorsMapOverview: dataCollectorsActions.getMapOverview.invoke,
  getMapDetails: dataCollectorsActions.getMapDetails.invoke
};

export const DataCollectorsMapOverviewPage = withLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsMapOverviewPageComponent)
);
