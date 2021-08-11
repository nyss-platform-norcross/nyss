import styles from './MarkerClusterComponent.module.scss';
import { Marker, Popup } from "react-leaflet";
import MarkerClusterGroup from 'react-leaflet-markercluster';
import { stringKeys, strings } from '../../strings';
import { Loading } from '../common/loading/Loading';

export const MarkerClusterComponent = ({ data, details, detailsFetching, createIcon, createClusterIcon, handleMarkerClick }) => {

  return (
    <MarkerClusterGroup
      maxClusterRadius={50}
      showCoverageOnHover={false}
      iconCreateFunction={createClusterIcon}
    >
      {data.filter(d => d.reportsCount > 0).map(point =>
        <Marker
          key={`marker_${point.location.latitude}${point.reportsCount}`}
          position={{ lat: point.location.latitude, lng: point.location.longitude }}
          icon={createIcon(point.reportsCount)}
          reportsCount={point.reportsCount}
          eventHandlers={{
            click: handleMarkerClick
          }}
        >
          <Popup>
            <div className={styles.popup}>
              {!detailsFetching
                ? (
                  <div>
                    {details && details.map(h => (
                      <div className={styles.reportHealthRiskDetails} key={`reportHealthRisk_${h.name}`}>
                        <div>{h.name}:</div>
                        <div>{h.value} {strings(h.value === 1 ? stringKeys.reportsMap.report : stringKeys.reportsMap.reports)}</div>
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
  );
}