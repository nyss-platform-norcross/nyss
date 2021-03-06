import styles from "./ReportsMap.module.scss";

import React, { useEffect, useState } from 'react';
import MarkerClusterGroup from 'react-leaflet-markercluster';
import { MapContainer, TileLayer, Popup, Marker, ScaleControl } from 'react-leaflet';
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

  useEffect(() => {
    if (!data) {
      return;
    }

    totalReports = data.reduce((a, d) => a + d.reportsCount, 0);

    setBounds(data.length > 1 ? calculateBounds(data) : null)
    setCenter(data.length > 1 ? null : calculateCenter(data.map(l => ({ lat: l.location.latitude, lng: l.location.longitude }))));
  }, [data])

  const handleMarkerClick = e =>
    onMarkerClick(e.latlng.lat, e.latlng.lng);

  return (!!center || !!bounds) && (
    <MapContainer
      style={{ height: "500px" }}
      zoom={5}
      bounds={bounds}
      center={center}
      scrollWheelZoom={false}
      maxZoom={19}
    >
      <TileLayer
        attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />

      {data && (
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
              eventHandlers={{
                click: handleMarkerClick
              }}
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
      <ScaleControl imperial={false}></ScaleControl>
    </MapContainer>
  );
}
