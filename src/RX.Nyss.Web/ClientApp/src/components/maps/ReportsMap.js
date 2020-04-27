import styles from "./ReportsMap.module.scss";

import React, { useEffect, useState } from 'react';
import MarkerClusterGroup from 'react-leaflet-markercluster';
import { Map, TileLayer, Popup, Marker } from 'react-leaflet'
import { calculateBounds, calculateCenter, calculateIconSize } from '../../utils/map';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from "../../strings";
import { TextIcon } from "../common/map/MarkerIcon";

let totalReports = 0;

const createClusterIcon = (cluster) => {
  const count = cluster.getAllChildMarkers().reduce((sum, item) => item.options.reportsCount + sum, 0);

  return new TextIcon({
    size: calculateIconSize(count, totalReports),
    text: count
  });
}

const createIcon = (count) => {
  return new TextIcon({
    size: calculateIconSize(count, totalReports),
    text: count
  });
}

export const ReportsMap = ({ data, details, detailsFetching, onMarkerClick }) => {
  const [bounds, setBounds] = useState(null);
  const [center, setCenter] = useState(null);
  const [isMapLoading, setIsMapLoading] = useState(false);

  useEffect(() => {
    if (!data) {
      return;
    }

    totalReports = data.reduce((a, d) => a + d.reportsCount, 0);
    setIsMapLoading(true); // used to remove the component from the view and clean the marker groups

    setTimeout(() => {
      setBounds(data.length > 1 ? calculateBounds(data) : null)
      setCenter(data.length > 1 ? null : calculateCenter(data.map(l => ({ lat: l.location.latitude, lng: l.location.longitude }))));
      setIsMapLoading(false);
    }, 0);
  }, [data])

  const handleMarkerClick = e =>
    onMarkerClick(e.latlng.lat, e.latlng.lng);

  return (
    <Map
      style={{ height: "500px" }}
      zoom={5}
      bounds={bounds}
      center={center}
      scrollWheelZoom={false}
      maxZoom={25}
    >
      <TileLayer
        attribution=''
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />

      {(data && !isMapLoading) && (
        <MarkerClusterGroup
          maxClusterRadius={50}
          showCoverageOnHover={false}
          iconCreateFunction={createClusterIcon}>
          {data.filter(d => d.reportsCount > 0).map(point =>
            <Marker
              key={`marker_${point.location.latitude}_${point.location.longitude}`}
              position={{ lat: point.location.latitude, lng: point.location.longitude }}
              icon={createIcon(point.reportsCount)}
              reportsCount={point.reportsCount}
              onclick={handleMarkerClick}
            >
              <Popup>
                <div className={styles.popup}>
                  {!detailsFetching
                    ? (
                      <div>
                        {details && details.map(h => (
                          <div className={styles.reportHealthRiskDetails} key={`reportHealthRisk_${h.name}`}>
                            <div>{h.name}:</div>
                            <div>{h.value} {strings(h.value === 1 ? stringKeys.reportsMap.report : stringKeys.reportsMap.reports)}</div>
                          </div>
                        ))}
                      </div>
                    )
                    : (<Loading inline noWait />)
                  }
                </div>
              </Popup>
            </Marker>
          )}
        </MarkerClusterGroup>
      )}
    </Map>
  );
}
