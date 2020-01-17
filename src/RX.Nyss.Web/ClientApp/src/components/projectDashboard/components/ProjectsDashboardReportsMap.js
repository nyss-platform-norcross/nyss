import styles from "./ProjectsDashboardReportsMap.module.scss";

import React, { useEffect, useState } from 'react';
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import CardHeader from '@material-ui/core/CardHeader';
import MarkerClusterGroup from 'react-leaflet-markercluster';
import { Map, TileLayer, Popup, Marker } from 'react-leaflet'
import { calculateBounds, calculateCenter } from '../../../utils/map';
import { Loading } from '../../common/loading/Loading';
import { strings, stringKeys } from "../../../strings";
import { TextIcon } from "../../common/map/MarkerIcon";

const createClusterIcon = (cluster) => {
  const count = cluster.getAllChildMarkers().reduce((sum, item) => item.options.reportsCount + sum, 0);

  return new TextIcon({
    size: 40,
    text: count,
    multiple: true
  });
}

const createIcon = (count) => {
  return new TextIcon({
    size: 40,
    text: count
  });
}

export const ProjectsDashboardReportsMap = ({ data, details, detailsFetching, projectId, getReportHealthRisks }) => {
  const [bounds, setBounds] = useState(null);
  const [center, setCenter] = useState(null);
  const [isMapLoading, setIsMapLoading] = useState(false);

  useEffect(() => {
    if (!data) {
      return;
    }

    setIsMapLoading(true); // used to remove the component from the view and clean the marker groups

    setTimeout(() => {
      setBounds(data.length > 1 ? calculateBounds(data) : null)
      setCenter(data.length > 1 ? null : calculateCenter(data.map(l => ({ lat: l.location.latitude, lng: l.location.longitude }))));
      setIsMapLoading(false);
    }, 0)
  }, [data])

  const handleMarkerClick = e =>
    getReportHealthRisks(projectId, e.latlng.lat, e.latlng.lng);

  return (
    <Card data-printable={true}>
      <CardHeader title={strings(stringKeys.project.dashboard.map.title)} />
      <CardContent>
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
              showCoverageOnHover={false}
              iconCreateFunction={createClusterIcon}>
              {data.map(point =>
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
                                <div>{h.value} {strings(h.value === 1 ? stringKeys.project.dashboard.map.report : stringKeys.project.dashboard.map.reports)}</div>
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
      </CardContent>
    </Card>
  );
}
