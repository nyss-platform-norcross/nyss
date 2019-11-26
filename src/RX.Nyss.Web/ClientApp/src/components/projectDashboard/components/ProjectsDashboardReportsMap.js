import L from 'leaflet';
import { Map, TileLayer, Marker, Popup } from 'react-leaflet'

import React from 'react';
import Card from '@material-ui/core/Card';
import CardContent from '@material-ui/core/CardContent';
import CardHeader from '@material-ui/core/CardHeader';

export const ProjectsDashboardReportsMap = ({ data }) => {

  const center = data && data.map((l => [l.location.latitude, l.location.longitude])).reduce((avg, value, _, { length }) => {
    return [avg[0] + value[0] / length, avg[1] + value[1] / length];
  }, [0, 0]);

  return (
    <Card>
      <CardHeader title="Map" />
      <CardContent>
        <Map
          center={center}
          length={4}
          zoom={6}>
          <TileLayer
            attribution='&amp;copy <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          {data &&
            data.map(point =>
              <Marker position={[point.location.latitude, point.location.longitude]}
                icon={L.divIcon({
                  html: `<?xml version="1.0" encoding="UTF-8" standalone="no"?>
                <svg width="100%" height="100%" viewBox="0 0 20 20">
                    <circle style="fill:#c02c2c;" cx="10" cy="10" r="10"/>
                </svg>`,
                  iconSize: [10 + (10 - (10 / point.reportsCount))],
                  iconAnchor: [10, 10],
                  className: "leaflet-marker-icon"
                })}>
                <Popup>{point.reportsCount}</Popup>
              </Marker>
            )}
        </Map>

      </CardContent>
    </Card>
  );
}
