import styles from "./ClusterIcon.module.scss"
import L from 'leaflet'

export const ClusterIcon = ({ cluster }) => {
  const data = cluster.getAllChildMarkers().map(m => m.options.dataCollectorInfo);

  const isInvalid = data.some(d => d.countNotReporting || d.countReportingWithErrors);

  return L.divIcon({
    html: `<span>${cluster.getChildCount()}</span>`,
    className: `${styles.cluster} ${isInvalid ? styles.invalid : styles.valid}`,
    iconSize: L.point(40, 40, true),
  });
}