import React, { useState, useRef, useEffect } from 'react'
import { Map, TileLayer, Marker, Popup, ScaleControl } from 'react-leaflet'

export const DataCollectorMap = ({ onChange, location, zoom, centerLocation }) => {
  const [markerLocation, setMarkerLocation] = useState(null);
  const [zoomLevel, setZoomLevel] = useState(zoom || 13);

  useEffect(() => {
    if (!location?.lat || !location?.lng) {
      setMarkerLocation(null);
      return;
    }

    setMarkerLocation(location);
  }, [location]);

  const mapRef = useRef(null);

  const handleClick = (e) => {
    const map = mapRef.current
    if (!map) {
      return;
    }

    onChange(e.latlng);
  }

  const handleLocationFound = e =>
    setMarkerLocation(e.latlng);

  return (
    <Map
      center={centerLocation || markerLocation}
      length={4}
      onClick={handleClick}
      onLocationfound={handleLocationFound}
      ref={mapRef}
      zoom={zoomLevel}
      scrollWheelZoom={false}
      onzoomend={(e) => setZoomLevel(e.target._zoom)}
    >
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
    </Map>
  )
}