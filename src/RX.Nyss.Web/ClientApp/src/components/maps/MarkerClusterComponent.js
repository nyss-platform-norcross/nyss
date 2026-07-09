import styles from './MarkerClusterComponent.module.scss';
import { Loading } from '../common/loading/Loading';
import { Marker, Popup } from "react-leaflet";
import MarkerClusterGroup from 'react-leaflet-markercluster';
import { stringKeys, strings } from '../../strings';

export const MarkerClusterComponent = ({ data, createIcon, createClusterIcon, handleMarkerClick }) => {
  return (
    <MarkerClusterGroup
      maxClusterRadius={50}
      showCoverageOnHover={false}
      iconCreateFunction={createClusterIcon}
    >
      {data.filter(d => d.reportsCount > 0 && d.location && d.location.latitude != null && d.location.longitude != null).map((point, index) =>
        <Marker
          key={`marker_${point.location.latitude}_${point.location.longitude}_${index}`}
          position={point.displayPosition ? { lat: point.displayPosition.lat, lng: point.displayPosition.lng } : { lat: point.location.latitude, lng: point.location.longitude }}
          icon={createIcon(point.reportsCount)}
          reportsCount={point.reportsCount}
          eventHandlers={{
            click: (e) => {
              if (e.originalEvent) {
                e.originalEvent.stopPropagation();
              }
              handleMarkerClick(point.location.latitude, point.location.longitude);
            }
          }}
        >
          <Popup>
            <div
              className={styles.popup}
              onMouseDown={(e) => e.stopPropagation()}
              onClick={(e) => e.stopPropagation()}
            >
              <div>
                {(point.healthRisks && point.healthRisks.length > 0)
                  ? point.healthRisks.map(h => (
                      <div className={styles.reportHealthRiskDetails} key={`reportHealthRisk_${h.name}`}>
                        <div>{h.name}:</div>
                        <div>{h.value} {strings(h.value === 1 ? stringKeys.reportsMap.report : stringKeys.reportsMap.reports)}</div>
                      </div>
                    ))
                  : (
                      <div className={styles.reportHealthRiskDetails}>
                        <div>{point.reportsCount} {strings(point.reportsCount === 1 ? stringKeys.reportsMap.report : stringKeys.reportsMap.reports)}</div>
                      </div>
                    )}
              </div>
            </div>
          </Popup>
        </Marker>
      )}
    </MarkerClusterGroup>
  );
}