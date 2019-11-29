import { Map, TileLayer, CircleMarker, Popup } from 'react-leaflet'
import styles from "./ProjectsDashboardReportsMap.module.scss";

import React from 'react';
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import CardHeader from '@material-ui/core/CardHeader';
import { strings, stringKeys } from '../../../strings';

const calculateBounds = (locations) => {
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

  let points = locations.map(loc => ({ lat: loc.location.latitude, lng: loc.location.longitude }));
  return [[getMinLat(points), getMinLng(points)], [getMaxLat(points), getMaxLng(points)]];
};

export const ProjectsDashboardReportsMap = ({ data }) => {

  const center = data && data.map((l => [l.location.latitude, l.location.longitude])).reduce((avg, value, _, { length }) => {
    return [avg[0] + value[0] / length, avg[1] + value[1] / length];
  }, [0, 0]);


  const bounds = data.length > 1 ? calculateBounds(data) : null

  return (
    <Card>
      <CardHeader title="Map" />
      <CardContent>
        <Map style={{ height: "800px" }}
          bounds={bounds}>
          <TileLayer
            attribution='&amp;copy <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          {data &&
            data.map(point =>
              <CircleMarker center={[point.location.latitude, point.location.longitude]} color="red" radius={8 + (10 - (10 / point.reportsCount))}>
                <Popup>
                  <div className={styles.popup}>
                    <div>
                      <div className={styles.reportCountMapDetails}>
                        {strings(stringKeys.project.dashboard.map.reportCount)}: {point.reportsCount}
                      </div>
                    </div>
                  </div>
                </Popup>
              </CircleMarker>
            )}
        </Map>
      </CardContent>
    </Card>
  );
}
