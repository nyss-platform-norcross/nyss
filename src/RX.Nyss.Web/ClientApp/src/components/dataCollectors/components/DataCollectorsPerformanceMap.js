import styles from "./DataCollectorsPerformanceMap.module.scss";
import { useState, useEffect } from "react";
import {
  MapContainer,
  TileLayer,
  Popup,
  Marker,
  ScaleControl,
} from "react-leaflet";
import MarkerClusterGroup from "react-leaflet-markercluster";
import { calculateBounds } from "../../../utils/map";
import { SignIcon } from "../../common/map/MarkerIcon";
import { getIconFromStatus } from "../logic/dataCollectorsService";
import { performanceStatus } from "../logic/dataCollectorsConstants";

const JITTER_RADIUS_DEGREES = 0.00012;

function locationsWithDisplayPositions(dataCollectorLocations) {
  if (!dataCollectorLocations || dataCollectorLocations.length === 0) return [];
  const key = (loc) =>
    `${loc.location?.latitude ?? ""}_${loc.location?.longitude ?? ""}`;
  const byPosition = new Map();
  dataCollectorLocations.forEach((dc) => {
    if (!dc?.location) return;
    const k = key(dc);
    if (!byPosition.has(k)) byPosition.set(k, []);
    byPosition.get(k).push(dc);
  });
  return dataCollectorLocations.map((dc) => {
    if (!dc?.location) return null;
    const group = byPosition.get(key(dc));
    const index = group.findIndex(
      (x) => x.dataCollectorId === dc.dataCollectorId
    );
    const indexInGroup = index >= 0 ? index : group.indexOf(dc);
    const count = group.length;
    const displayPosition =
      count <= 1
        ? { lat: dc.location.latitude, lng: dc.location.longitude }
        : (() => {
            const angle = (indexInGroup / count) * 2 * Math.PI;
            return {
              lat:
                dc.location.latitude +
                JITTER_RADIUS_DEGREES * Math.cos(angle),
              lng:
                dc.location.longitude +
                JITTER_RADIUS_DEGREES * Math.sin(angle),
            };
          })();
    return { ...dc, displayPosition };
  }).filter(Boolean);
}

const createClusterIcon = (cluster) => {
  const data = cluster
    .getAllChildMarkers()
    .map((m) => m.options.dataCollectorInfo);

  // Sum up the counts from all data collectors in the cluster
  const aggregatedData = {
    countReportingCorrectly: data.reduce((sum, d) => sum + (d.countReportingCorrectly || 0), 0),
    countReportingWithErrors: data.reduce((sum, d) => sum + (d.countReportingWithErrors || 0), 0),
    countNotReporting: data.reduce((sum, d) => sum + (d.countNotReporting || 0), 0),
  };

  const status = getAggregatedStatus(aggregatedData);

  return SignIcon({
    icon: getIconFromStatus(status),
    className: styles[`marker_${status}`],
    size: 40,
    multiple: true,
  });
};

const createIcon = (info) => {
  const status = getAggregatedStatus(info);

  return SignIcon({
    icon: getIconFromStatus(status),
    className: styles[`marker_${status}`],
    size: 40,
  });
};

const getAggregatedStatus = (info) => {
  // Ensure we have numeric values (handle undefined/null)
  const correctCount = info.countReportingCorrectly || 0;
  const incorrectCount = info.countReportingWithErrors || 0;
  const notReportingCount = info.countNotReporting || 0;
  
  // If there are no reports at all (all data collectors are not reporting), show "not reporting"
  // This must be checked FIRST before comparing counts
  if (correctCount === 0 && incorrectCount === 0) {
    return performanceStatus.notReporting;
  }

  // Compare counts: if correct reports are equal to or greater than incorrect reports, show "reporting correctly"
  // Otherwise, show "reporting with errors"
  if (correctCount >= incorrectCount) {
    return performanceStatus.reportingCorrectly;
  }

  return performanceStatus.reportingWithErrors;
};

export const DataCollectorsPerformanceMap = ({
  centerLocation,
  dataCollectorLocations,
}) => {
  const [center, setCenter] = useState(null);
  const [bounds, setBounds] = useState(null);

  useEffect(() => {
    const hasLocations = dataCollectorLocations && dataCollectorLocations.length > 0;
    const hasValidCenter = centerLocation && centerLocation.latitude != null && centerLocation.longitude != null;

    if (hasLocations && hasValidCenter) {
      setCenter({
        lat: centerLocation.latitude,
        lng: centerLocation.longitude,
      });
      setBounds(calculateBounds(dataCollectorLocations));
    } else {
      setCenter(null);
      setBounds(null);
    }
  }, [dataCollectorLocations, centerLocation]);

  const markersWithPosition = locationsWithDisplayPositions(dataCollectorLocations);

  const shouldRender = (center || bounds) && dataCollectorLocations && dataCollectorLocations.length > 0;

  const clusterDataKey = dataCollectorLocations
    .reduce(
      (acc, loc) =>
        acc +
        (loc.countReportingCorrectly || 0) +
        (loc.countReportingWithErrors || 0) +
        (loc.countNotReporting || 0),
      0,
    )
    .toString();

  return (
    shouldRender && (
      <MapContainer
        center={center}
        length={4}
        bounds={bounds}
        zoom={5}
        maxZoom={17}
        className={styles.map}
      >
        <TileLayer
          attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />

        <MarkerClusterGroup
          key={clusterDataKey}
          showCoverageOnHover={false}
          iconCreateFunction={createClusterIcon}
        >
          {markersWithPosition.map((dc, i) => (
            <Marker
              className={`${styles.marker} ${
                dc.countNotReporting || dc.countReportingWithErrors
                  ? styles.markerInvalid
                  : styles.markerValid
              }`}
              key={`marker_${dc.dataCollectorId ?? i}`}
              position={dc.displayPosition}
              icon={createIcon(dc)}
              dataCollectorInfo={dc}
            >
              <Popup>
                <div className={styles.popupContent}>
                  {dc.displayName ?? ""}
                </div>
              </Popup>
            </Marker>
          ))}
        </MarkerClusterGroup>
        <ScaleControl imperial={false}></ScaleControl>
      </MapContainer>
    )
  );
};