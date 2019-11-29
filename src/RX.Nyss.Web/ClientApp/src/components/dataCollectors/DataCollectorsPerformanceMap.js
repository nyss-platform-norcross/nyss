import styles from "./DataCollectorsPerformanceMap.module.scss"

import React, { useRef, useState, Fragment } from 'react';
import { Map, TileLayer, CircleMarker, Popup } from 'react-leaflet';
import MarkerClusterGroup from 'react-leaflet-markercluster';
import L from 'leaflet'
import { Loading } from "../common/loading/Loading";
import CircleMarkerX from "./CircleMarkerX";
import Icon from "@material-ui/core/Icon";


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

  let points = dataCollectorLocations.map(loc => ({ lat: loc.location.latitude, lng: loc.location.longitude }));
  return [[getMinLat(points), getMinLng(points)], [getMaxLat(points), getMaxLng(points)]];
};

const createClusterCustomIcon = function (cluster) {
  const data = cluster.getAllChildMarkers().map(m => m.options.dataCollectorInfo);

  const isInvalid = data.some(d => d.countNotReporting || d.countReportingWithErrors);

  return L.divIcon({
    html: `<span>${cluster.getChildCount()}</span>`,
    className: `${styles.cluster} ${isInvalid ? styles.invalid : styles.valid}`,
    iconSize: L.point(40, 40, true),
  });
}

export const DataCollectorsPerformanceMap = ({ centerLocation, dataCollectorLocations, projectId, details, getMapDetails, detailsFetching }) => {
  const [pointData, setPointData] = useState(null);

  if (!centerLocation) {
    return null;
  }

  const bounds = dataCollectorLocations.length > 1 ? calculateBounds(dataCollectorLocations) : null;

  const handleMarkerClick = (e) => getMapDetails(projectId, e.latlng.lat, e.latlng.lng);

  const getIconFromStatus = (status) => {
    switch (status) {
      case "ReportingCorrectly": return "check";
      case "ReportingWithErrors": return "cancel";
      case "NotReporting": return "hourglass_empty";
      default: return "contact_support";
    }
  }

  return (
    <Map
      center={{ lat: centerLocation.latitude, lng: centerLocation.longitude }}
      length={4}
      bounds={bounds}
      zoom={5}
      maxZoom={25}
    >
      <TileLayer
        attribution='&amp;copy <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />

      <MarkerClusterGroup 
        showCoverageOnHover={false}
        iconCreateFunction={createClusterCustomIcon}>
          {dataCollectorLocations.map(dc => (
            <CircleMarkerX
              className={`${styles.marker} ${dc.countNotReporting || dc.countReportingWithErrors ? styles.markerInvalid : styles.markerValid}`}
              key={`marker_${dc.location.latitude}_${dc.location.longitude}`}
              center={{ lat: dc.location.latitude, lng: dc.location.longitude }}
              position={{ lat: dc.location.latitude, lng: dc.location.longitude }}
              onclick={handleMarkerClick}
              dataCollectorInfo={{
                countReportingCorrectly: dc.countReportingCorrectly,
                countReportingWithErrors: dc.countReportingWithErrors,
                countNotReporting: dc.countNotReporting
              }}
          >
            <Popup>
              <div className={styles.popup}>
                {!detailsFetching
                  ? (
                    <div>
                      {details.map(d => (
                        <div key={`dataCollector_${d.id}`} className={styles.dataCollectorDetails}>
                          <Icon>{getIconFromStatus(d.status)}</Icon>
                          {d.displayName}
                        </div>
                      ))}
                    </div>
                  )
                  : (<Loading inline />)
                }
              </div>
            </Popup>
          </CircleMarkerX>
        ))}
      </MarkerClusterGroup>
    </Map>
  );
}
