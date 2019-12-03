import styles from "./ClusterIcon.module.scss"
import L from 'leaflet'

export const ClusterIcon = ({ cluster, isValid }) => L.divIcon({
  html: `<span>${cluster.getChildCount()}</span>`,
  className: `${styles.cluster} ${isValid ? styles.valid : styles.invalid}`,
  iconSize: L.point(40, 40, true),
});