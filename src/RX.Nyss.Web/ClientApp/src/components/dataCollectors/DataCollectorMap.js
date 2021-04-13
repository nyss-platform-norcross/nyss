import React, { useState, useEffect } from 'react'
import { MapContainer, TileLayer, Marker, Popup, ScaleControl, useMapEvent, useMap } from 'react-leaflet'

const MapEventHandler = ({ onMarkerClick, onChange }) => {
  useMapEvent('click', onMarkerClick);
  useMapEvent('click', onChange);
  return null;
}

const ChangeCenterLocation = ({ center, zoom }) => {
  const map = useMap();
  map.setView(center, zoom);
  return null;
}

export const DataCollectorMap = ({ onChange, location, zoom, initialCenterLocation }) => {
  const [markerLocation, setMarkerLocation] = useState(null);
  const [centerLocation, setCenterLocation] = useState(null);
  const [zoomLevel, setZoomLevel] = useState(zoom || 13);

  useEffect(() => {
    if (!location?.lat || !location?.lng) {
      setMarkerLocation(null);
      return;
    }

    setMarkerLocation(location);
    setCenterLocation(location);
  }, [location]);

  useEffect(() => {
    if (initialCenterLocation) {
      setCenterLocation(initialCenterLocation);
    }
  }, [initialCenterLocation]);

  const handleClick = (e) =>
    onChange(e.latlng);

  const handleLocationFound = e =>
    setMarkerLocation(e.latlng);

  return !!centerLocation && (
    <MapContainer
      center={centerLocation}
      length={4}
      zoom={zoomLevel}
      scrollWheelZoom={false}
      onzoomend={(e) => setZoomLevel(e.target._zoom)}
    >
      <MapEventHandler
        onMarkerClick={handleLocationFound}
        onChange={handleClick} />

      <ChangeCenterLocation center={centerLocation} zoom={zoomLevel} />

      <TileLayer
        attribution='&amp;copy <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />
      {markerLocation && (
        <Marker position={markerLocation}>
          <Popup>You are here</Popup>
        </Marker>
      )}
      <ScaleControl imperial={false}></ScaleControl>
    </MapContainer>
  )
}