import React, { Fragment, useRef } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import * as dataCollectorsActions from './logic/dataCollectorsActions';
import { useLayout } from '../../utils/layout';
import Layout from '../layout/Layout';
import { useMount } from '../../utils/lifecycle';
import { Map, TileLayer } from 'react-leaflet'

const DataCollectorsMapOverviewPageComponent = (props) => {
  useMount(() => {
    props.openDataCollectorsMapOverview(props.projectId, '2019-11-01', '2019-11-30' );
  });
  const mapRef = useRef(null);
  
  if (!props.centerLocation) {
    return null;
  }



  const calculateBounds = (dataCollectorLocations) => {  
    
    function getMinLat(points) {
      return points.reduce((min, p) => p.lat < min ? p.lat : min, points[0].lat);
    }
    function getMaxLat(points) {
      return points.reduce((max, p) => p.lat > max ? p.lat : max, points[0].lat);
    }
    function getMinLng(points) {
      return points.reduce((min, p) => p.lng < min ? p.lng : min, points[0].lng);
    }
    function getMaxLng(points) {
      return points.reduce((max, p) => p.lng > max ? p.lng : max, points[0].lng);
    }

    let points = dataCollectorLocations.map(loc => ({lat: loc.location.latitude, lng: loc.location.longitude}));
    return [[getMinLat(points), getMinLng(points)],[getMaxLat(points),getMaxLng(points)]];
  };

  const bounds = calculateBounds(props.dataCollectorLocations);

     
  return (
    <Fragment>
      <Map
         center={{lat: props.centerLocation.latitude, lng: props.centerLocation.longitude}}
         length={4}        
         bounds = {bounds}
         ref={mapRef}
         zoom={1}
        >
        <TileLayer
          attribution='&amp;copy <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        
      </Map>
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
});

const mapDispatchToProps = {
  openDataCollectorsMapOverview: dataCollectorsActions.openMapOverview.invoke,
  getDataCollectorsMapOverview: dataCollectorsActions.getMapOverview.invoke
};

export const DataCollectorsMapOverviewPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(DataCollectorsMapOverviewPageComponent)
);
