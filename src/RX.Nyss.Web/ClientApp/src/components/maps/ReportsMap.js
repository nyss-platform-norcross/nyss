import React, { useEffect, useState, useRef, useMemo } from "react";
import {
  MapContainer,
  TileLayer,
  ScaleControl,
  useMap,
  useMapEvent,
} from "react-leaflet";

import {
  calculateBounds,
  calculateCenter,
  calculateIconSize,
} from "../../utils/map";

import { TextIcon } from "../common/map/MarkerIcon";
import { MarkerClusterComponent } from "./MarkerClusterComponent";

const JITTER_RADIUS_DEGREES = 0.00012;

const addDisplayPositions = (data) => {
  if (!data) return [];

  const groups = data.reduce((acc, point) => {
    const key = `${point.location.latitude.toFixed(4)}:${point.location.longitude.toFixed(4)}`;
    if (!acc[key]) acc[key] = [];
    acc[key].push(point);
    return acc;
  }, {});

  const result = [];

  Object.values(groups).forEach((group) => {
    if (group.length === 1) {
      const point = group[0];
      result.push({
        ...point,
        displayPosition: {
          lat: point.location.latitude,
          lng: point.location.longitude,
        },
      });
      return;
    }

    const angleStep = (2 * Math.PI) / group.length;

    group.forEach((point, index) => {
      const angle = index * angleStep;
      const offsetLat = JITTER_RADIUS_DEGREES * Math.sin(angle);
      const offsetLng = JITTER_RADIUS_DEGREES * Math.cos(angle);

      result.push({
        ...point,
        displayPosition: {
          lat: point.location.latitude + offsetLat,
          lng: point.location.longitude + offsetLng,
        },
      });
    });
  });

  return result;
};

export const ReportsMap = ({ data }) => {
  const [bounds, setBounds] = useState(null);
  const [center, setCenter] = useState(null);
  const [zoom, setZoom] = useState(2);
  const [totalReports, setTotalReports] = useState(0);
  const [hasInteractedWithMap, setHasInteractedWithMap] = useState(false);

  const resizeObserver = useRef(null);

  useEffect(() => {
    if (!data) return;

    setHasInteractedWithMap(false);

    const total = data.reduce(
      (sum, d) => sum + (d.reportsCount || 0),
      0
    );

    setTotalReports(total);
    setZoom(total >= 1 ? 5 : 2);

    setBounds(data.length > 1 ? calculateBounds(data) : null);

    setCenter(
      data.length > 1
        ? null
        : calculateCenter(
            data.map((l) => ({
              lat: l.location.latitude,
              lng: l.location.longitude,
            }))
          )
    );
  }, [data]);

  const MapViewController = ({ center }) => {
    const map = useMap();

    useEffect(() => {
      if (hasInteractedWithMap) return;

      if (center) {
        const centerLatLong = totalReports > 0 ? center : [20, 0];
        map.setView(centerLatLong, zoom);
      }

      if (bounds) {
        map.fitBounds(bounds);
      }
    }, [center, map, bounds, zoom, hasInteractedWithMap, totalReports]);

    return null;
  };

  const MapInteractionHandler = () => {
    useMapEvent("movestart", () => setHasInteractedWithMap(true));
    useMapEvent("zoomstart", () => setHasInteractedWithMap(true));
    return null;
  };

  const createClusterIcon = (cluster) => {
    const count = cluster
      .getAllChildMarkers()
      .reduce(
        (sum, item) => sum + (item.options.reportsCount || 0),
        0
      );

    return TextIcon({
      size: calculateIconSize(count, totalReports),
      text: count,
    });
  };

  const createIcon = (count) =>
    TextIcon({
      size: calculateIconSize(count, totalReports),
      text: count,
    });

  const handleMarkerClick = () => {
    setHasInteractedWithMap(true);
  };

  const setMap = (map) => {
    resizeObserver.current = new ResizeObserver(() => {
      map.invalidateSize();
    });

    const container = document.getElementById("map-container");

    if (container) {
      resizeObserver.current.observe(container);
    }
  };

  useEffect(() => {
    return () => {
      if (resizeObserver.current) {
        resizeObserver.current.disconnect();
      }
    };
  }, []);

  const markersData = useMemo(
    () => addDisplayPositions(data),
    [data]
  );

  return (!!center || !!bounds) && (
    <MapContainer
      id="map-container"
      style={{ minHeight: "100%", borderRadius: 8 }}
      zoom={zoom}
      bounds={bounds || undefined}
      center={center || undefined}
      scrollWheelZoom={false}
      maxZoom={17}
      whenCreated={setMap}
    >
      <TileLayer
        attribution='&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
      />

      <MarkerClusterComponent
        data={markersData}
        createIcon={createIcon}
        createClusterIcon={createClusterIcon}
        handleMarkerClick={handleMarkerClick}
      />

      <ScaleControl imperial={false} />
      <MapInteractionHandler />
      <MapViewController center={center} />
    </MapContainer>
  );
};