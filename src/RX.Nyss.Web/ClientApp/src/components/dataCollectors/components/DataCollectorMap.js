import styles from './DataCollectorMap.module.scss';
import { useState, useEffect } from 'react';
import { MapContainer, TileLayer, Marker, Popup, ScaleControl, useMapEvent, useMap } from 'react-leaflet';
import { retrieveGpsLocation } from '../../../utils/map';
import MyLocationIcon from '@material-ui/icons/MyLocation';
import { Tooltip } from '@material-ui/core';
import { Fragment } from 'react';
import { stringKeys, strings } from '../../../strings';

const MapEventHandler = ({ onMarkerClick, onChange, onZoomChange }) => {
  useMapEvent('click', onMarkerClick);
  useMapEvent('click', onChange);
  const map = useMapEvent('zoomend', (e) => {
    const center = map.getCenter();
    onZoomChange(e, center);
  });
  return null;
}

const ChangeCenterLocation = ({ center, zoom }) => {
  const map = useMap();
  map.setView(center, zoom);
  return null;
}

const ZoomToCurrentLocation = ({ updateZoomLevel }) => {
  const map = useMap();
  const [isFetcingLocation, setIsFetchingLocation] = useState(false);

  const onRetrieveLocation = (event) => {
    setIsFetchingLocation(true);

    retrieveGpsLocation(location => {
      if (location === null) {
        return;
      }

      const lat = location.coords.latitude;
      const lng = location.coords.longitude;
      map.setView({ lat: lat, lng: lng }, 11);
      setIsFetchingLocation(false);
      updateZoomLevel(11);
    });
  }

  return (
    <div className="leaflet-top leaflet-left">
      <div className="leaflet-control leaflet-bar">
        <div className={styles.myLocationButton}>
          {!isFetcingLocation && (
            <Tooltip title={strings(stringKeys.dataCollectors.form.showYourLocation)} placement="right">
              <MyLocationIcon onClick={onRetrieveLocation} />
            </Tooltip>
          )}

          {isFetcingLocation && (
            <Fragment>
              <MyLocationIcon onClick={onRetrieveLocation} />

              <div className={styles.fetchingLocation}>
                <span>{strings(stringKeys.dataCollectors.form.retrievingLocation)}</span>
              </div>
            </Fragment>
          )}
        </div>
      </div>
    </div>
  );
}

export const DataCollectorMap = ({ onChange, location, zoom, initialCenterLocation }) => {
  const [markerLocation, setMarkerLocation] = useState(null);
  const [centerLocation, setCenterLocation] = useState(null);
  const [zoomLevel, setZoomLevel] = useState(zoom || 13);

  useEffect(() => {
    if (!location || (!location.lat && !location.lng)) {
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

  const clickedMyLocationButton = (e) =>
    typeof e.originalEvent.target.className === 'object'
    || e.originalEvent.target.className.indexOf('leaflet-container') === -1;

  const handleClick = (e) =>
    !clickedMyLocationButton(e) && onChange(e.latlng);

  const handleLocationFound = e =>
    !clickedMyLocationButton(e) && setMarkerLocation(e.latlng);

  const handleZoomChange = (e, center) => {
    setZoomLevel(e.target._zoom);
    setCenterLocation(center);
  }

  return !!centerLocation && (
    <MapContainer
      center={centerLocation}
      length={4}
      zoom={zoomLevel}
      scrollWheelZoom={false}
    >
      <MapEventHandler
        onMarkerClick={handleLocationFound}
        onChange={handleClick}
        onZoomChange={handleZoomChange} />

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
      <ZoomToCurrentLocation updateZoomLevel={setZoomLevel} />
    </MapContainer>
  )
}