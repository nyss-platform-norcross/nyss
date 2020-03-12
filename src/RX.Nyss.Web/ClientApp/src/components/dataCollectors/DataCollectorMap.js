import React, { useState, useRef, useEffect } from 'react'
import { Map, TileLayer, Marker, Popup } from 'react-leaflet'

export const DataCollectorMap = ({ onChange, location, zoom }) => {
  const [markerLocation, setMarkerLocation] = useState(location);

  useEffect(() => {
    setMarkerLocation(location);
  }, [location]);

  const mapRef = useRef(null);

  const handleClick = (e) => {
    const map = mapRef.current
    if (!map) {
      return;
    }
    setMarkerLocation(e.latlng);
    onChange(e.latlng);
  }

  const handleLocationFound = e =>
    setMarkerLocation(e.latlng);

  return (
    <Map
      center={location}
      length={4}
      onClick={handleClick}
      onLocationfound={handleLocationFound}
      ref={mapRef}
      zoom={zoom || 13}
      scrollWheelZoom={false}
    >
      <TileLayer
        attribution='&amp;copy <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />
      <Marker position={markerLocation}>
        <Popup>You are here</Popup>
      </Marker>
    </Map>
  )
}