import React, { Fragment, useEffect } from "react";
import PropTypes from "prop-types";
import { connect, useSelector } from "react-redux";
import * as dataCollectorsActions from "./logic/dataCollectorsActions";
import { withLayout } from "../../utils/layout";
import Layout from "../layout/Layout";
import { useMount } from "../../utils/lifecycle";
import { DataCollectorsPerformanceMap } from "./components/DataCollectorsPerformanceMap";
import { DataCollectorsPerformanceMapFilters } from "./components/DataCollectorsPerformanceMapFilters";
import { DataCollectorsPerformanceMapLegend } from "./components/DataCollectorsPerformanceMapLegend";
import * as tracking from "../../utils/tracking";
import TableHeader from "../common/tableHeader/TableHeader";
import { trackPageView } from "../../utils/appInsightsHelper";

const DataCollectorsMapOverviewPageComponent = (props) => {
  useMount(() => {
    props.openDataCollectorsMapOverview(props.projectId);

    // Track page view
    trackPageView("DataCollectorsMapOverviewPage");
  });

  const useRtlDirection = useSelector(
    (state) => state.appData.direction === "rtl",
  );

  const handleFiltersChange = (value) =>
    props.getDataCollectorsMapOverview(props.projectId, value);

  if (!props.filters) {
    return null;
  }

  return (
    <Fragment>
      <TableHeader />
      <DataCollectorsPerformanceMapFilters
        onChange={handleFiltersChange}
        filters={props.filters}
        healthRisks={props.healthRisks}
        rtl={useRtlDirection}
      />
      <DataCollectorsPerformanceMap
        centerLocation={props.centerLocation}
        dataCollectorLocations={props.dataCollectorLocations}
      />
      <DataCollectorsPerformanceMapLegend rtl={useRtlDirection} />
    </Fragment>
  );
};

DataCollectorsMapOverviewPageComponent.propTypes = {
  getDataCollectorsMapOverview: PropTypes.func,
};

const mapStateToProps = (state, ownProps) => ({
  projectId: ownProps.match.params.projectId,
  dataCollectorLocations:
    state.dataCollectors.mapOverviewDataCollectorLocations,
  centerLocation: state.dataCollectors.mapOverviewCenterLocation,
  details: state.dataCollectors.mapOverviewDetails,
  detailsFetching: state.dataCollectors.mapOverviewDetailsFetching,
  filters: state.dataCollectors.mapOverviewFilters,
  healthRisks: state.dataCollectors.mapOverviewHealthRisks,
});

const mapDispatchToProps = {
  openDataCollectorsMapOverview: dataCollectorsActions.openMapOverview.invoke,
  getDataCollectorsMapOverview: dataCollectorsActions.getMapOverview.invoke,
  trackPage: tracking.actions.pageView,
};

export const DataCollectorsMapOverviewPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(DataCollectorsMapOverviewPageComponent),
);