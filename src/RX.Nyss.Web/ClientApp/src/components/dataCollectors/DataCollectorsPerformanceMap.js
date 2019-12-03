import styles from "./DataCollectorsPerformanceMap.module.scss"

import React, { useState, useEffect } from 'react';
import { Map, TileLayer, Popup } from 'react-leaflet';
import MarkerClusterGroup from 'react-leaflet-markercluster';
import { Loading } from "../common/loading/Loading";
import Icon from "@material-ui/core/Icon";
import CircleMapMarker from "../common/map/CircleMapMarker";
import { ClusterIcon } from "../common/map/ClusterIcon";
import { calculateBounds } from "../../utils/map";

export const DataCollectorsPerformanceMap = ({ centerLocation, dataCollectorLocations, projectId, details, getMapDetails, detailsFetching }) => {
  const [bounds, setBounds] = useState(null);

  useEffect(() => {
    setBounds(dataCollectorLocations.length > 1 ? calculateBounds(dataCollectorLocations) : null)
  }, [dataCollectorLocations])

  if (!centerLocation) {
    return null;
  }

  const handleMarkerClick = e =>
    getMapDetails(projectId, e.latlng.lat, e.latlng.lng);

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
      className={styles.map}
    >
      <TileLayer attribution='' url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />

      <MarkerClusterGroup
        showCoverageOnHover={false}
        iconCreateFunction={cluster => ClusterIcon({ cluster })}>
        {dataCollectorLocations.map(dc => (
          <CircleMapMarker
            className={`${styles.marker} ${dc.countNotReporting || dc.countReportingWithErrors ? styles.markerInvalid : styles.markerValid}`}
            key={`marker_${dc.location.latitude}_${dc.location.longitude}`}
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
                      {details && details.map(d => (
                        <div key={`dataCollector_${d.id}`} className={styles.dataCollectorDetails}>
                          <Icon>{getIconFromStatus(d.status)}</Icon>
                          {d.displayName}
                        </div>
                      ))}
                    </div>
                  )
                  : (<Loading inline noWait />)
                }
              </div>
            </Popup>
          </CircleMapMarker>
        ))}
      </MarkerClusterGroup>
    </Map>
  );
}
